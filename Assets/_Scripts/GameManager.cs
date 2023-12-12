using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public delegate void RoundCallback(TeamColor color);
	public RoundCallback OnGameEnd;

    [SerializeField] private int roundDurationSeconds;

    private NetworkVariable<int> startedTime = new NetworkVariable<int>(0);
    public NetworkVariable<int> curTimeInSeconds = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> curRedPlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> curBluePlayers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // i dont like this, but speed.
    [SerializeField] PaintableTileManager paintableTileManager;
    static public GameManager instance;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            instance = this;
        }

        base.OnNetworkSpawn();
        RecalculateGameState();

        curBluePlayers.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue == 0)
            {
                EndRoundServerRpc(TeamColor.RED);
            }
        };
        curRedPlayers.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue == 0)
            {
                EndRoundServerRpc(TeamColor.BLUE);
            }
        };
        StartRound();
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            instance = null;
        }
    }
    NetworkVariable<bool> hasStarted = new NetworkVariable<bool>(false);

	private void StartRound()
    {
        hasStarted.Value = true;
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
        curRedPlayers.Value = redPlayers;
        curBluePlayers.Value = bluePlayers;
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
            if (!hasStarted.Value) return;
            if (curTimeInSeconds.Value > 0)
            {
                curTimeInSeconds.Value = roundDurationSeconds - ((int)Time.time - startedTime.Value);
                return;
            }
            EndRoundServerRpc(CalculateCurrentWinningTeam());
        }
    }

    [ServerRpc]
    private void EndRoundServerRpc(TeamColor color)
    {
        Debug.Log("Game ended" + color.ToString() + " won!");
        RaiseOnGameEnd(color);
        switch (color)
        {
            case TeamColor.RED:
                break;

            case TeamColor.BLUE:

                break;
        }
    }


    public void RaiseOnGameEnd(TeamColor color)
    {
        OnGameEnd?.Invoke(color);
    }
}
