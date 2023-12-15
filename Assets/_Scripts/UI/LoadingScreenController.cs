using Eflatun.SceneReference;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
	[SerializeField] TMP_InputField code;
	RelayManager relayManager;
	[SerializeField] SceneReference mapRef;

	public async void StartHost()
	{
		await RelayManager.CreateRelay(true);
		StartHostInternal();
	}
	public void StartHostInternal()
	{
		NetworkManager.Singleton.StartHost();
		NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
		NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
		NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
		NetworkManager.Singleton.SceneManager.LoadScene(mapRef.Name, LoadSceneMode.Single);
	}
	public void StartClientInternal()
	{
		NetworkManager.Singleton.StartClient();
		NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
		NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
		NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
	}

	public async void StartClient()
	{
		if (code.text != string.Empty)
		{
			await RelayManager.JoinRelay(code.text);
			NetworkManager.Singleton.StartClient();
			NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
			NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
			NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
		}
	}

	public async void StartServer()
	{
		await RelayManager.CreateRelay(false);
		NetworkManager.Singleton.StartServer();
		NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
		NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
		NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
		NetworkManager.Singleton.SceneManager.LoadScene(mapRef.Name, LoadSceneMode.Single);
	}



}

