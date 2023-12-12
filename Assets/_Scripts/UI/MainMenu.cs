using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_Text unityStatusText;
    [SerializeField] TMP_Text accountStatusText;
    [SerializeField] GameObject ReconnectPrefab;
    [SerializeField] GameObject LobbyPanelPrefab;
    [SerializeField] Transform LobbyPanelParent;

    // Prevent Spamming Netwworking calls while waiting for a response
    bool isWorking = false;
    private void Awake()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            AccountText = AuthenticationService.Instance.IsSignedIn ? "Signed In : " + AuthenticationService.Instance.PlayerName + "( " + AuthenticationService.Instance.PlayerId + " )" : "Signed Out";
            OnServicesInitialized();
        }
        AuthManager.OnServicesInitialized += OnServicesInitialized;
        UnityText = UnityServices.State.ToString();
    }


    public string LobbyJoinCode { get; set; }
    public async void HostLobbyInput()
    {
        if (isWorking) return; // Prevent Spamming Netwworking calls while waiting for a response
        isWorking = true;
        var lobby = await LobbyManager.CreateLobby(false);
        if (lobby == null)
        {
            Debug.Log("Lobby is null");
            isWorking = false;
            return;
        }

        Instantiate(LobbyPanelPrefab, LobbyPanelParent).GetComponent<LobbyInstance>().SetLobbyData(lobby);
        isWorking = false;
    }

    public async void JoinLobbyInput()
    {
        if (isWorking) return; // Prevent Spamming Netwworking calls while waiting for a response
        isWorking = true;
        if (string.IsNullOrEmpty(LobbyJoinCode))
        {
            Debug.Log("LobbyJoinCode is null or empty");
            isWorking = false;
            return;
        }
        var lobby = await LobbyManager.JoinLobby(LobbyJoinCode);
        if (lobby == null)
        {
            Debug.Log("Lobby is null");
            isWorking = false;
            return;
        }
        Instantiate(LobbyPanelPrefab, LobbyPanelParent).GetComponent<LobbyInstance>().SetLobbyData(lobby);
        isWorking = false;
    }
    public async void QuickJoinLobbyInput()
    {
        if (isWorking) return; // Prevent Spamming Netwworking calls while waiting for a response
        isWorking = true;
        if (string.IsNullOrEmpty(LobbyJoinCode))
        {
            Debug.Log("LobbyJoinCode is null or empty");
            return;
        }
        var lobby = await LobbyManager.QuickJoin();
        if (lobby == null)
        {
            Debug.Log("Lobby is null");
            isWorking = false;
            return;
        }
        Instantiate(LobbyPanelPrefab, LobbyPanelParent).GetComponent<LobbyInstance>().SetLobbyData(lobby);
        isWorking = false;
    }

    public string AccountText
    {
        get => accountStatusText.text;
        set
        {
            if (accountStatusText.text != value)
            {
                accountStatusText.text = value;
                return;
            }
            accountStatusText.text = value;
        }
    }
    public string UnityText
    {
        get => unityStatusText?.text ?? "No Text";
        set
        {
            if (unityStatusText.text != value)
            {
                unityStatusText.text = value;
                return;
            }
            unityStatusText.text = value;
        }
    }

    private void OnServicesInitialized()
    {
        UnityText = "Unity Services Initialized";
        AuthenticationService.Instance.SignedIn += SignedIn;
        AuthenticationService.Instance.SignedOut += SignedOut;
        AuthenticationService.Instance.Expired += Expired;
        AuthenticationService.Instance.SignInFailed += SignedInFailed;
    }

    private void OnEnable()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            AuthenticationService.Instance.SignedIn += SignedIn;
            AuthenticationService.Instance.SignedOut += SignedOut;
            AuthenticationService.Instance.Expired += Expired;
            AuthenticationService.Instance.SignInFailed += SignedInFailed;
        }
    }
    private void OnDisable()
    {
        AuthenticationService.Instance.SignedIn -= SignedIn;
        AuthenticationService.Instance.SignedOut -= SignedOut;
        AuthenticationService.Instance.Expired -= Expired;
        AuthenticationService.Instance.SignInFailed -= SignedInFailed;
    }

    private void Expired()
    {
        AccountText = "Session Expired";
    }

    private void SignedInFailed(RequestFailedException exception)
    {
        AccountText = "Sign In Failed";
    }

    private  void SignedOut()
    {
        AccountText = "Signed Out";
    }

    private async void SignedIn()
    {
        AccountText = "Signed In : " + AuthenticationService.Instance.PlayerName + "( " + AuthenticationService.Instance.PlayerId + " )";
        var lobbies = await Lobbies.Instance.GetJoinedLobbiesAsync();
        foreach (var lobbyID in lobbies)
        {
            Instantiate(ReconnectPrefab, LobbyPanelParent).GetComponent<Reconnect>().ReconnectData(lobbyID);
        }
    }
}