using System.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private World _clientWorld;
    private Entity _connectionRequest;
    private string _ipAddress;
    private ushort _port;

    private async void Awake()
    {
        InitializationOptions options = new InitializationOptions();

#if !UNITY_SERVER
        // Used for testing multiple clients on the same machine
        options.SetProfile($"Foo_{UnityEngine.Random.Range(-10000, 10000)}");
#endif
        await UnityServices.InitializeAsync(options);
#if !UNITY_SERVER
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif

        foreach (World world in World.All)
        {
            if (world.IsClient())
                _clientWorld = world;
        }

        if (_clientWorld != null)
            StartCoroutine(WaitUntilConnected());
    }

    private IEnumerator WaitUntilConnected()
    {
        EntityManager entityManager = _clientWorld.EntityManager;

        while (true)
        {
            EntityQuery query = entityManager.CreateEntityQuery(typeof(NetworkId));
            if (!query.IsEmpty)
            {
                NetworkEvents.s_OnClientConnectedLocal?.Invoke(this, new ClientConnectedLocalEventArgs
                {
                    IPv4 = _ipAddress,
                    Port = _port,
                });
                yield break;
            }

            yield return null;
        }
    }

    public void SetIPAddress(string address)
    {
        _ipAddress = address;
    }

    public void SetPort(string port)
    {
        _port = ushort.Parse(port);
    }

    public void ConnectWithInternal()
    {
        Connect(_ipAddress, _port);
    }

    public void Connect(string ip, ushort port)
    {
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);

        if (!_clientWorld.EntityManager.Exists(_connectionRequest))
            _connectionRequest = _clientWorld.EntityManager.CreateEntity(typeof(NetworkStreamRequestConnect));

        _clientWorld.EntityManager.SetComponentData(_connectionRequest, new NetworkStreamRequestConnect { Endpoint = endpoint });
    }
}
