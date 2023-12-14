using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    private static string _joinCode = string.Empty;
    public static string JoinCode { get { return _joinCode; } }
    public bool IsLoggedIn { get { return AuthenticationService.Instance.IsSignedIn; } }

    // Start is called before the first frame update
    private void Awake()
    {
        AuthManager.OnServicesInitialized += OnServicesInitialized;
    }

    private void OnServicesInitialized()
    {
        // ?

    }

    public static async Task CreateRelay(bool host)
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(4);

        _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        //Debug.Log( _joinCode );

        var unityTranport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (host)
        {
            unityTranport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
        }
        else
        {
            unityTranport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
        }

    }

    internal static async Task JoinRelay(string text)
    {
        JoinAllocation joinAllocation = await RelayService
            .Instance.JoinAllocationAsync(text);

        _joinCode = text;

        var unityTranport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        unityTranport.SetClientRelayData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );
    }
}
