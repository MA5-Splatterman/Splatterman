using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


// This class is used to render a lobby in the UI and handle all the lobby events
public class LobbyInstance : MonoBehaviour
{
    public Lobby Lobby { get; set; }
    [SerializeField] private TMP_Text LobbyCode;
    [SerializeField] private TMP_Text JoinHostButton;

    [SerializeField] private GameObject LobbyPlayerPrefab;
    [SerializeField] private Transform LobbyPlayerList;
    private LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
    public string RelayCode { get; set; }
    public bool HasStarted { get; set; }

    public async void SetLobbyData(Lobby lobby)
    {
        Lobby = lobby;
        LobbyCode.text = lobby.LobbyCode;
        // RelayCode = lobby.Data["RelayCode"].Value;
        await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
        callbacks.LobbyDeleted += CallbacksOnLobbyDeleted;
        callbacks.LobbyChanged += CallbacksOnLobbyChanged;
        callbacks.KickedFromLobby += CallbacksOnLobbyDeleted;
        callbacks.PlayerJoined += CallbacksOnPlayerJoined;
        callbacks.DataChanged += CallbacksOnLobbyDataChanged;
        RenderPlayerList();
    }

    private void CallbacksOnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> dictionary)
    {
        RelayCode = Lobby.Data["RelayCode"].Value;
        HasStarted = Lobby.Data["HasStarted"].Value == "true" ? true : false;
        Debug.Log("Data changed");
        if (HasStarted)
        {
            Debug.Log("Game has started");
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
        }
    }
    private void CallbacksOnLobbyDeleted()
    {
        Debug.Log("Lobby Deleted");
        Destroy(gameObject);
    }

    public void RenderPlayerList()
    {
        foreach (Transform child in LobbyPlayerList)
        {
            Destroy(child.gameObject);
        }
        
        LobbyService.Instance.GetLobbyAsync(Lobby.Id).ContinueWith((task) =>
        {
            Lobby = task.Result;
            foreach (var player in Lobby.Players)
            {
                Instantiate(LobbyPlayerPrefab, LobbyPlayerList).GetComponent<LobbyPlayer>().SetPlayer(player, this);
            }
        });
    }
    private void CallbacksOnPlayerJoined(List<LobbyPlayerJoined> list)
    {
        Debug.Log("Player Joined");
        RenderPlayerList();
    }

    private void CallbacksOnLobbyChanged(ILobbyChanges changes)
    {
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