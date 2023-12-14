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
		NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
		NetworkManager.Singleton.StartHost();
		NetworkManager.Singleton.SceneManager.LoadScene(mapRef.Name, LoadSceneMode.Single);
	}

	public async void StartClient()
	{
		if (code.text != string.Empty)
		{
			await RelayManager.JoinRelay(code.text);
			NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
			NetworkManager.Singleton.StartClient();
		}
	}

	public async void StartServer()
	{
		await RelayManager.CreateRelay(false);
		NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
		NetworkManager.Singleton.StartServer();
		NetworkManager.Singleton.SceneManager.LoadScene(mapRef.Name, LoadSceneMode.Single);
	}



}

