using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class Reconnect : MonoBehaviour {
    public string LobbyId;
    public GameObject ReconnectPrefab;
    public Button button;
    private void Awake() {
        button?.onClick.AddListener(ReconnectToLobby);
    }
    public void ReconnectData(string lobbyId) { 
        LobbyId = lobbyId;

    }
    public async void ReconnectToLobby() { 
        Lobby lobby = await LobbyManager.Reconnect(LobbyId);
        Instantiate(ReconnectPrefab,transform.parent).GetComponent<LobbyInstance>().SetLobbyData(lobby);
        Destroy(gameObject);
    }
}