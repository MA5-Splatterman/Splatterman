using System;
using System.Collections;
using System.Linq;
using Eflatun.SceneReference;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGamePopup : MonoBehaviour
{
	[SerializeField] private Animator _animator;

	[SerializeField] private SceneReference _mainMenuScene;

	[SerializeField] private SceneReference _currentScene;

	[SerializeField] private GameManager _gameManager;

	[SerializeField] private GameObject _rematchButton;

	private void OnEnable()
	{
		_animator.SetBool("MenuOpen", true);
		if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
		{
			_rematchButton.SetActive(false);
			NetworkManager.Singleton.OnServerStopped += OnServerStopped;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
		}
	}

	private void OnClientDisconnectCallback(ulong ClientId)
	{
		NetworkManager.Singleton.Shutdown(true);
		StartCoroutine(ToMainMenuCoroutine());
	}

	private void OnServerStopped(bool obj)
	{
		NetworkManager.Singleton.Shutdown(true);
		StartCoroutine(ToMainMenuCoroutine());
	}

	private void OnDisable()
	{
		_animator.SetBool("MenuOpen", false);
		if(NetworkManager.Singleton == null) return;
		if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
		{
			NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
		}
	}
	/// <summary>
	/// SceneLoad Main Menu
	/// </summary>
	public void ToMainMenu()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			var list = NetworkManager.Singleton.ConnectedClients.ToArray();
			foreach (var item in list)
			{
				// Disconnect the clients
				if (item.Value.ClientId == NetworkManager.Singleton.LocalClientId)
				{
					continue;
				}
				NetworkManager.Singleton.DisconnectClient(item.Key);
			}
			NetworkManager.Singleton.Shutdown(true);
			StartCoroutine(ToMainMenuCoroutine());
		} else {
			NetworkManager.Singleton.Shutdown( true );
			StartCoroutine( ToMainMenuCoroutine() );
		}
	}

	private IEnumerator ToMainMenuCoroutine()
	{
		while (NetworkManager.Singleton.ShutdownInProgress) { yield return null; }
		Destroy(NetworkManager.Singleton.gameObject);
		SceneManager.LoadScene(_mainMenuScene.Name, LoadSceneMode.Single);
	}

	/// <summary>
	/// SceneLoad This Scene
	/// /// </summary>
	public void ReloadScene()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			foreach (var item in NetworkManager.Singleton.ConnectedClients)
			{
				Debug.Log("Despawning player : " + item.Key);
				item.Value.PlayerObject.Despawn(true);
			}

			NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
			NetworkManager.Singleton.SceneManager.LoadScene(_currentScene.Name, LoadSceneMode.Single);
		}
	}
	/// <summary>
	/// Just like Quit the game.
	/// </summary>
	public void QuitGame()
	{
		Application.Quit();
	}
}
