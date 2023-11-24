using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public delegate void RoundCallback(TeamColor color);
    public RoundCallback OnPlayerKilled;
    public RoundCallback OnPlayerJoinTeam;
    public RoundCallback OnGameEnd;

    [SerializeField] private int roundDurationSeconds;

    private NetworkVariable<int> startedTime = new NetworkVariable<int>(0);
    private NetworkVariable<int> curTimeInSeconds = new NetworkVariable<int>(0);
    private NetworkVariable<int> curRedPlayers = new NetworkVariable<int>(0);
    private NetworkVariable<int> curBluePlayers = new NetworkVariable<int>(0);


    public override void OnNetworkSpawn()
    {   
        base.OnNetworkSpawn();
        
    }

    private void StartRound()
    {
        curTimeInSeconds.Value = roundDurationSeconds;
        startedTime.Value = (int)Time.time;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (curRedPlayers.Value <= 0)
            {
                EndRoundServerRpc(TeamColor.BLUE);
                return;
            }
            if (curBluePlayers.Value <= 0)
            {
                EndRoundServerRpc(TeamColor.RED);
                return;
            }
            if (curTimeInSeconds.Value > 0)
            {
                curTimeInSeconds.Value = roundDurationSeconds - ((int)Time.time - startedTime.Value);
            }
        }
    }

    private void EndRoundServerRpc(TeamColor color)
    {
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

    public void RaiseOnPlayerKilled(TeamColor color)
    {
        switch (color)
        {
            case TeamColor.RED:
                curRedPlayers.Value--;
                break;

            case TeamColor.BLUE:
                curBluePlayers.Value--;
                break;
        }
        OnPlayerKilled?.Invoke(color);
    }

    public void RaiseOnPlayerJoinTeam(TeamColor color)
    {
        switch (color)
        {
            case TeamColor.RED:
                curRedPlayers.Value++;
                break;

            case TeamColor.BLUE:
                curBluePlayers.Value++;
                break;
        }
        OnPlayerJoinTeam?.Invoke(color);
    }
}
