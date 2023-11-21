using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoClientStart : MonoBehaviour
{
    [SerializeField] private NetworkManager manager;

    private void Start()
    {
#if !UNITY_EDITOR
        manager.StartClient();
#endif
#if UNITY_EDITOR
        manager.StartHost();
#endif
    }
}
