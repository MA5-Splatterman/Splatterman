using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
	private string _joinCode = string.Empty;
	public string JoinCode {  get { return _joinCode; }}

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log(AuthenticationService.Instance.AccessToken);

    }

    public async Task CreateRelay(bool host)
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(4);

		_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log( _joinCode );

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

    internal async Task JoinRelay(string text)
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
