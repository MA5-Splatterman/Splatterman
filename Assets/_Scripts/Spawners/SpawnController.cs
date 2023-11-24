using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
	private List<Transform> _spawnLocations = new();
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
}
