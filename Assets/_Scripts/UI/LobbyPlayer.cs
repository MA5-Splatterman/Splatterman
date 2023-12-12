using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Button kickButton;
    LobbyInstance lobbyInstance;
    Player player;
    public void SetPlayer(Player player, LobbyInstance lobby)
    {
        this.player = player;
        playerName.text = player.Id;
        if(player.Id == AuthenticationService.Instance.PlayerId)
        {
            playerName.text += " (You)";
        }
        if (player.Id == lobby.Lobby.HostId)
        {
            playerName.text += " (Host)";
        }
        lobbyInstance = lobby;
        kickButton.interactable = lobby.Lobby.HostId == AuthenticationService.Instance.PlayerId;

        kickButton.onClick.AddListener(async () =>
        {
            await lobbyInstance.KickPlayer(this.player.Id);
        });
    }
}