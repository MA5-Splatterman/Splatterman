using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


// This class is used to render a lobby in the UI and handle all the lobby events
public class LobbyInstance : MonoBehaviour
{
    public Lobby Lobby { get; set; }
    [SerializeField] private TMP_Text LobbyCode;
    [SerializeField] private TMP_Text JoinHostButton;
    [SerializeField] private Button JoinHostButtonEvent;
    [SerializeField] private GameObject LobbyPlayerPrefab;
    [SerializeField] private Transform LobbyPlayerList;
    private LobbyEventCallbacks callbacks;
    public string RelayCode { get; set; }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(2f);
            if (Lobby == null) continue;
            // if (Lobby.HostId == AuthenticationService.Instance.PlayerId) HandleHearthBeat();
            HandlePolling();
        }
    }
    async void HandlePolling()
    {
        if (Lobby == null) return;
        try
        {
            Lobby = await LobbyService.Instance.GetLobbyAsync(Lobby.Id);
            RelayCode = Lobby.Data.TryGetValue("RelayCode", out var relayCode) ? relayCode.Value : "";
            Debug.Log($"Lobby {Lobby.Id} has {Lobby.Players.Count} players and relay code {RelayCode}");
            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    async void HandleHearthBeat()
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
    }
    public async void SetLobbyData(Lobby lobby)
    {
        Lobby = lobby;
        LobbyCode.text = lobby.LobbyCode;
        callbacks = new LobbyEventCallbacks();
        callbacks.LobbyDeleted += CallbacksOnLobbyDeleted;
        callbacks.LobbyChanged += CallbacksOnLobbyChanged;
        callbacks.KickedFromLobby += CallbacksOnLobbyDeleted;
        callbacks.PlayerJoined += CallbacksOnPlayerJoined;
        callbacks.DataChanged += CallbacksOnLobbyDataChanged;
        callbacks.PlayerLeft += CallbacksOnPlayerLeft;


        try
        {
            await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{Lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
        RenderPlayerList();

        if (lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            JoinHostButton.text = "Start Game";
            JoinHostButtonEvent.onClick.AddListener(async () =>
            {
                try
                {
                    await RelayManager.CreateRelay(true);
                    UpdateLobbyOptions options = new UpdateLobbyOptions
                    {
                        HostId = AuthenticationService.Instance.PlayerId,
                        Data = new Dictionary<string, DataObject>()
                        {
                            {
                                "RelayCode", new DataObject(
                                    visibility: DataObject.VisibilityOptions.Member,
                                    value: RelayManager.JoinCode,
                                    index: DataObject.IndexOptions.S1)
                            }
                        }
                    };

                    try
                    {
                        await LobbyService.Instance.UpdateLobbyAsync(Lobby.Id, options);
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.Log(e);
                        return;
                    }
                    FindFirstObjectByType<LoadingScreenController>()?.StartHostInternal();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            });
        }
        else
        {
            JoinHostButton.text = "Leave Lobby";
            JoinHostButtonEvent.onClick.AddListener(() =>
            {
                Debug.Log("Leave Lobby");
                LobbyService.Instance.RemovePlayerAsync(Lobby.Id, AuthenticationService.Instance.PlayerId);
            });
        }

        RelayCode = Lobby.Data.TryGetValue("RelayCode", out var relayCode) ? relayCode.Value : "";
        StartClient();
    }

    private void CallbacksOnPlayerLeft(List<int> list)
    {
        RenderPlayerList();
    }

    bool hasStartedClient = false;
    public async void StartClient()
    {
        if (hasStartedClient || string.IsNullOrEmpty(RelayCode))
        {
            return;
        }
        try
        {
            await RelayManager.JoinRelay(RelayCode);
            Debug.Log("Joined Relay");
        }
        catch (Exception e) { return; }
        hasStartedClient = true;
        Debug.Log("Trying to Starting Client");
        FindFirstObjectByType<LoadingScreenController>()?.StartClientInternal();
    }
    private async void CallbacksOnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> dictionary)
    {
        if (dictionary.TryGetValue("RelayCode", out var relayCode))
        {
            Debug.Log("RelayCode changed (DataChanged)" + relayCode.Value.Value);
            // extract data
            string code = relayCode.Value.Value;
            RelayCode = code;

        }
    }

    private void OnDisable()
    {
        if (callbacks != null)
        {
            callbacks.LobbyDeleted -= CallbacksOnLobbyDeleted;
            callbacks.KickedFromLobby -= CallbacksOnLobbyDeleted;
            callbacks.LobbyChanged -= CallbacksOnLobbyChanged;
            callbacks.PlayerJoined -= CallbacksOnPlayerJoined;
            callbacks.DataChanged -= CallbacksOnLobbyDataChanged;
            callbacks.DataAdded -= CallbacksOnLobbyDataChanged;
            callbacks.PlayerLeft -= CallbacksOnPlayerLeft;
        }
    }
    private void CallbacksOnLobbyDeleted()
    {
        Debug.Log("Lobby Deleted");
        Destroy(gameObject);
    }

    public void RenderPlayerList()
    {


        Debug.Log($"Lobby {Lobby.Id} has {Lobby.Players.Count} players");
        foreach (Transform child in LobbyPlayerList)
        {
            Destroy(child.gameObject);
        }
        foreach (var player in Lobby.Players)
        {
            var game = Instantiate(LobbyPlayerPrefab, LobbyPlayerList);
            game.GetComponent<LobbyPlayer>().SetPlayer(player, this);
        }

    }
    private void CallbacksOnPlayerJoined(List<LobbyPlayerJoined> list)
    {
        Debug.Log("Player Joined");
        LobbyService.Instance.GetLobbyAsync(Lobby.Id).ContinueWith((task) =>
        {
            Lobby = task.Result;
            RenderPlayerList();
        });
    }

    private async void CallbacksOnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            CallbacksOnLobbyDeleted();
            return;
        }
        if (AuthenticationService.Instance.PlayerId == Lobby.HostId)
        {
            RenderPlayerList();
            return;
        }

        if (changes.Data.Value.TryGetValue("RelayCode", out var relayCode))
        {
            Debug.Log("RelayCode changed (LobbyChanged)" + relayCode.Value.Value);
            // extract data
            string code = relayCode.Value.Value;
            if (string.IsNullOrEmpty(code))
            {
                Debug.Log("RelayCode is empty");
                return;
            }
            RelayCode = code;
            await RelayManager.JoinRelay(RelayCode);
            FindFirstObjectByType<LoadingScreenController>()?.StartClient();
        }
        RenderPlayerList();
    }



    public async Task KickPlayer(string playerId)
    {
        if (playerId == AuthenticationService.Instance.PlayerId)
        {
            await LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
            return;
        }
        try
        {
            foreach (var item in Lobby.Players)
            {
                if (item.Id == playerId)
                {
                    await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, playerId);

                    Debug.Log($"Kicked player {item.Id}");
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}