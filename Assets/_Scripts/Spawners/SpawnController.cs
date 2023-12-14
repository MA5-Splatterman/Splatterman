using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnController : NetworkBehaviour
{
	private List<Transform> _spawnLocations = new();
	[SerializeField] GameObject _playerPrefab;
	public static SpawnController Instance { get; private set; }
	public static Vector3 GetSpawnLocation(int index)
	{
		if (Instance == null) return Vector3.zero;
		return Instance._spawnLocations[index % Instance._spawnLocations.Count].position;
	}
	private void Awake()
	{
		Instance = this;
		foreach (Transform child in transform)
		{
			_spawnLocations.Add(child);
		}
	}

	private void OnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
	{
		var connections = clientsCompleted.ToArray().Distinct();
		Debug.Log("Connections" + clientsCompleted.Count());
		Debug.Log("Unique Connections" + connections.Count());
		foreach (var item in connections)
		{
			Debug.Log("[OnLoadComplete] Spawning player : " + item);
			SpawnPlayer(item);
		}
	}

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			Debug.Log("OnNetworkSpawn");
			var connections = NetworkManager.Singleton.ConnectedClientsIds;
			// foreach (var id in connections)
			// {
			// 	Debug.Log("Spawning player on networkSpawn: " + id);
			// 	SpawnPlayer(id);
			// }
			NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnect;
			NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadComplete;

		}
	}

	private void PlayerConnect(ulong id)
	{
		Debug.Log("[CONNECTION] Spawning player on networkSpawn: " + id);
		SpawnPlayer(id);
	}

	private void PlayerDisconnected(ulong id)
	{
		Debug.Log("[DISCONNECTION] Despawning player on networkSpawn: " + id);
		NetworkObject idPlayer = null;
		foreach (var player in PlayerController.players)
		{
			var playerNetworkObject = player.GetComponent<NetworkObject>();
			if (playerNetworkObject.OwnerClientId == id)
			{
				idPlayer = playerNetworkObject;
				break;
			}
		}

		if (idPlayer != null)
		{
			idPlayer.Despawn(true);
		}
	}

	public override void OnNetworkDespawn()
	{
		if (IsServer)
		{
			Debug.Log("OnNetworkDespawn");
			NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
			NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnected;

			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
		}
		base.OnNetworkDespawn();
	}

	private void SpawnPlayer(ulong id)
	{

		Instantiate(_playerPrefab).GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
	}


}
