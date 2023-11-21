using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
	private List<Transform> _spawnLocations = new(); 
	private int _spawnIndex = 0;
	private bool _nextSpawnIsRed = true;
	private TeamColor _teamColor;

	private void Awake () {
		foreach (Transform t in transform) {  
			_spawnLocations.Add(t); 
		}
	}

	private void IncreaseSpawnIndex() {
		_spawnIndex = (_spawnIndex + 1 < _spawnLocations.Count ) ? _spawnIndex + 1: 0;
	}
	/// <summary>
	/// Accepts GameObject playerPrefab to spawn, should flip every other time it is run. 
	/// </summary>
	/// <param name="_playerPrefab"></param>
	//[ServerRpc]
	//public void SpawnPlayerServerRpc(GameObject _playerPrefab) {
	public void SpawnPlayer(GameObject _playerPrefab) {
		Vector2 _spawnLocation = _spawnLocations[_spawnIndex].position;
		
		if ( _nextSpawnIsRed ) {
			_teamColor = TeamColor.RED;
			_nextSpawnIsRed = !_nextSpawnIsRed;

		} else { 
			_teamColor = TeamColor.BLUE;
			_nextSpawnIsRed = !_nextSpawnIsRed;

		}

		GameObject _gameObject = Instantiate( _playerPrefab, _spawnLocation, Quaternion.identity );
		PlayerController _playerController = _gameObject.GetComponent<PlayerController>();
		_playerController.AssignTeam(_teamColor);

		if (_gameObject.TryGetComponent( out NetworkObject netObject)) {
			netObject.Spawn();

		}

		IncreaseSpawnIndex();
	}
}
