using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoClientStart : MonoBehaviour
{
    [SerializeField] private NetworkManager manager;

    private void Start()
    {
        manager.StartClient();
    }
}
