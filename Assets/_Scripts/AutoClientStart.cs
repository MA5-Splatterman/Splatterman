using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoClientStart : MonoBehaviour
{
    [SerializeField] private NetworkManager manager;

    public void HostGame()
    {
        manager.StartHost();
    }

    public void JoinGame()
    {
        manager.StartClient();
    }
}
