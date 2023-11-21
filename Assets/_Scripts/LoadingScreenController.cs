using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] TMP_InputField code;
    [SerializeField] RelayManager relayManager;
	[SerializeField] string mapName = "BombTest";

    public async void StartHost()
    {
        await relayManager.CreateRelay(true);
        SceneManager.LoadSceneAsync( mapName );
        SceneManager.sceneLoaded += SceneManager_sceneLoaded_Host;
    }

    private void SceneManager_sceneLoaded_Host(Scene arg0, LoadSceneMode arg1) {
		NetworkManager.Singleton.StartHost();
		SetInterfaceJoinCode();

		SceneManager.sceneLoaded -= SceneManager_sceneLoaded_Host;
	}

	public async void StartClient () {
		if ( code.text != string.Empty ) { 
			await relayManager.JoinRelay( code.text );
			NetworkManager.Singleton.StartClient();
		}
    }

    public async void StartServer()
    {
        await relayManager.CreateRelay(false);
        SceneManager.LoadScene( mapName );
        SceneManager.sceneLoaded += SceneManager_sceneLoaded_Server;
    }

    private void SceneManager_sceneLoaded_Server(Scene arg0, LoadSceneMode arg1)
    {
        NetworkManager.Singleton.StartServer();
		SetInterfaceJoinCode();
		SceneManager.sceneLoaded -= SceneManager_sceneLoaded_Server;
	}
	/// <summary>
	/// Sets the join code displayed in the interface on the host/server
	/// </summary>
	private void SetInterfaceJoinCode () {
		var _interfaceController = FindAnyObjectByType<InterfaceController>();

		if ( _interfaceController != default ) {
			_interfaceController.SetJoinCode( relayManager.JoinCode );
		}
	}
}

