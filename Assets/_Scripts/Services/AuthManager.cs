using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static Action OnServicesInitialized;
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        OnServicesInitialized?.Invoke();
        await Login();
    }
    public static async Task<bool> Login()
    {
        // Login anonymously (change if we want to use a different login method)
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Failed To sign in");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }
}
