using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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
	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			var connections = NetworkManager.Singleton.ConnectedClients;
			foreach (var item in connections)
			{
				SpawnPlayer(item.Value.ClientId);
			}
			NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
			NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
		}
	}

	private void PlayerDisconnected(ulong id)
	{
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
		idPlayer.Despawn(true);
	}

	public override void OnNetworkDespawn()
	{
		NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
	}

	private void SpawnPlayer(ulong id)
	{
		Instantiate(_playerPrefab).GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
	}


}
