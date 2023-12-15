using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    static LobbyManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    public static async Task<Lobby> CreateLobby(bool IsPrivate)
    {
        string lobbyName = "new lobby";
        int maxPlayers = 4;
        CreateLobbyOptions options = new CreateLobbyOptions();

        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "RelayCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: "",
                        index: DataObject.IndexOptions.S1)
                }
            };
        options.IsPrivate = IsPrivate;
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        Debug.Log($"Created lobby {lobby.Id} with code {lobby.LobbyCode} and host {lobby.HostId}");
        return lobby;
    }


    public static async Task<Lobby> JoinLobby(string lobbyCode)
    {
        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
        if (lobby == null)
        {
            Debug.Log("Lobby is null");
            return null;
        }
        Debug.Log($"Joined lobby {lobby.Id} with code {lobby.LobbyCode} and host {lobby.HostId}");
        return lobby;
    }
    public static async Task<Lobby> Reconnect(string lobbyID)
    {
        Lobby lobby = await LobbyService.Instance.ReconnectToLobbyAsync(lobbyID);
        if (lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
        }
        if (lobby == null)
        {
            Debug.Log("Lobby is null");
            return null;
        }
        Debug.Log($"Joined lobby {lobby.Id} with code {lobby.LobbyCode} and host {lobby.HostId}");
        return lobby;
    }


    public static async Task<Lobby> QuickJoin()
    {
        try
        {
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    static async Task<Player> GetPlayer()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthManager.Login();
        }
        return new Player(AuthenticationService.Instance.PlayerId, null, data: new Dictionary<string, PlayerDataObject>());
    }

}
