using Eflatun.SceneReference;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {
	public delegate void RoundCallback ( TeamColor color );
	public RoundCallback OnGameEnd;

	[SerializeField] private int roundDurationSeconds;

	private NetworkVariable<int> startedTime = new NetworkVariable<int>( 0 );
	public NetworkVariable<int> curTimeInSeconds = new NetworkVariable<int>( 1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	public NetworkVariable<int> curRedPlayers = new NetworkVariable<int>( 0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	public NetworkVariable<int> curBluePlayers = new NetworkVariable<int>( 0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );

	// i dont like this, but speed.
	[SerializeField] PaintableTileManager paintableTileManager;
	static public GameManager instance;
	
	[SerializeField] private SceneReference _mainMenuScene;

    public override void OnNetworkSpawn()
    {
        instance = this;

        base.OnNetworkSpawn();
        RecalculateGameState();


        curBluePlayers.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue == 0 && gameIsActive.Value)
            {
                EndRoundServerRpc(TeamColor.RED);
            }
        };
        curRedPlayers.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue == 0 && gameIsActive.Value)
            {
                EndRoundServerRpc(TeamColor.BLUE);
            }
        };
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += StartLogic;
            StartLogic(0);
        }

		NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvents;
	}

	private void OnDisable () {
		//NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvents;
	}
	/// <summary>
	/// Handling sceneEvents
	/// </summary>
	/// <param name="sceneEvent"></param>
	private void HandleSceneEvents ( SceneEvent sceneEvent ) {
		if (_processingSceneEvents) { return; }
		_processingSceneEvents = true;
		Debug.Log( $"HandleSceneEvents: {sceneEvent.SceneEventType}" );
		switch ( sceneEvent.SceneEventType ) {
			case SceneEventType.LoadEventCompleted:
				if ( sceneEvent.SceneName == _mainMenuScene.Name ) {
					/** This runs when the main menu scene has loaded the second time. 
					 *	- We are getting rid of all client connections here, 
					 *		- The second time a client connects to the same server, it will duplicate the connection (?)
					 */
					if ( NetworkManager.Singleton.IsServer ) {
						var connections = NetworkManager.Singleton.ConnectedClients;
						Debug.Log( $"HandleSceneEvents.Disconnected Before: {NetworkManager.Singleton.ConnectedClients.Count}" );
						List<NetworkClient> _networkClients = new();
						foreach ( var connection in connections ) {
							_networkClients.Add( connection.Value );
						}
						foreach ( var connection in _networkClients ) {
							NetworkManager.Singleton.DisconnectClient( connection.ClientId );
						}
						_networkClients.Clear();
						Debug.Log( $"HandleSceneEvents.Disconnected After: {NetworkManager.Singleton.ConnectedClients.Count}" );
					}

					// This shuts down the connection between the machines.
					NetworkManager.Singleton.Shutdown(false);
				}
				break;
		}
		_processingSceneEvents = false;
	}
	public void StartLogic(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !gameIsActive.Value)
        {
            StartRound();
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            instance = null;
        }
    }
    public NetworkVariable<bool> gameIsActive = new NetworkVariable<bool>(false);
	private bool _processingSceneEvents;

	private void StartRound()
    {
        gameIsActive.Value = true;
        curTimeInSeconds.Value = roundDurationSeconds;
        startedTime.Value = (int)Time.time;
    }

    public void RecalculateGameState()
    {
        int redPlayers = 0;
        int bluePlayers = 0;
        foreach (var player in PlayerController.players)
        {
            if (player.isDead.Value) continue;
            if (player.team.Value == TeamColor.RED)
            {
                redPlayers++;
            }
            else
            {
                bluePlayers++;
            }
        }
        if (IsServer)
        {
            curRedPlayers.Value = redPlayers;
            curBluePlayers.Value = bluePlayers;
        }
    }

    public TeamColor CalculateCurrentWinningTeam()
    {
        TeamColor winnerBasedOnPlayerCount = curRedPlayers.Value > curBluePlayers.Value ? TeamColor.RED : curRedPlayers.Value < curBluePlayers.Value ? TeamColor.BLUE : TeamColor.NONE;
        TeamColor winnerBasedOnTilePaint = paintableTileManager.CheckWinningTeam();
        return winnerBasedOnPlayerCount != TeamColor.NONE ? winnerBasedOnPlayerCount : winnerBasedOnTilePaint;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (!gameIsActive.Value) return;
            if (curTimeInSeconds.Value > 0)
            {
                curTimeInSeconds.Value = roundDurationSeconds - ((int)Time.time - startedTime.Value);
                return;
            }
            EndRoundServerRpc(CalculateCurrentWinningTeam());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndRoundServerRpc(TeamColor color)
    {
        gameIsActive.Value = false;
        Debug.Log("Game ended" + color.ToString() + " won!");
        if (IsHost)
        {
            EndRoundClientRpc(color);
        }
        else
        {
            RaiseOnGameEnd(color);
            EndRoundClientRpc(color);
        }
    }
    [ClientRpc]
    private void EndRoundClientRpc(TeamColor color)
    {
        Debug.Log("Game ended" + color.ToString() + " won!");
        RaiseOnGameEnd(color);
    }


    public void RaiseOnGameEnd(TeamColor color)
    {
        OnGameEnd?.Invoke(color);
    }
}
