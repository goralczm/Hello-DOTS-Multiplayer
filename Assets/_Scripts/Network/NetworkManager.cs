using System;
using System.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static event Action s_OnClientConnected;

    private World _clientWorld;
    private string _ipAddress;
    private ushort _port;

    private void Start()
    {
        foreach (World world in World.All)
        {
            if (world.IsClient())
            {
                _clientWorld = world;
                break;
            }
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
                s_OnClientConnected?.Invoke();
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

    private void Connect(string ip, ushort port)
    {
        World clientWorld = World.All[0];
        foreach (World world in World.All)
        {
            if (world.IsClient())
            {
                clientWorld = world;
                break;
            }
        }

        var entityManager = clientWorld.EntityManager;
        var connectionEntity = entityManager.CreateEntity();

        var endpoint = NetworkEndpoint.Parse(ip, port);

        var connectRequest = clientWorld.EntityManager.CreateEntity(typeof(NetworkStreamRequestConnect));
        clientWorld.EntityManager.SetComponentData(connectRequest, new NetworkStreamRequestConnect { Endpoint = endpoint });
    }
}
