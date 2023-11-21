using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    TMP_InputField code;
    [SerializeField]
    RelayManager relayManager;

    public async void StartHost()
    {
        await relayManager.CreateRelay(true);
        SceneManager.LoadSceneAsync("Playing");
        SceneManager.sceneLoaded += SceneManager_sceneLoaded_Host;
    }

    private void SceneManager_sceneLoaded_Host(Scene arg0, LoadSceneMode arg1)
    {
        NetworkManager.Singleton.StartHost();
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded_Host;
    }

    public async void StartClient()
    {
        await relayManager.JoinRelay(code.text);
        NetworkManager.Singleton.StartClient();
    }

    public async void StartServer()
    {
        await relayManager.CreateRelay(false);
        SceneManager.LoadScene("Playing");
        SceneManager.sceneLoaded += SceneManager_sceneLoaded_Server;
    }

    private void SceneManager_sceneLoaded_Server(Scene arg0, LoadSceneMode arg1)
    {
        NetworkManager.Singleton.StartServer();
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded_Server;
    }
}

