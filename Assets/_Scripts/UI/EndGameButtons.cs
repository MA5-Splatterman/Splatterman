using Eflatun.SceneReference;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGamePopup : MonoBehaviour {
	[SerializeField] private Animator _animator;

	[SerializeField] private SceneReference _mainMenuScene;

	[SerializeField] private SceneReference _currentScene;

	[SerializeField] private GameManager _gameManager;

	private void OnEnable () {
		_animator.SetBool( "MenuOpen", true );
	}
	private void OnDisable () {
		_animator.SetBool( "MenuOpen", false );
	}

	/// <summary>
	/// SceneLoad Main Menu
	/// </summary>
	public void ToMainMenu () {
		NetworkManager.Singleton.SceneManager.LoadScene( _mainMenuScene.Name, LoadSceneMode.Single );
		NetworkManager.Singleton.Shutdown();
	}
	/// <summary>
	/// SceneLoad This Scene
	/// </summary>
	public void ReloadScene () {
		NetworkManager.Singleton.SceneManager.LoadScene( _currentScene.Name, LoadSceneMode.Single );
		_gameManager.StartLogic(0);
	}
	/// <summary>
	/// Just like Quit the game.
	/// </summary>
	public void QuitGame () {
		Application.Quit();
	}
}
