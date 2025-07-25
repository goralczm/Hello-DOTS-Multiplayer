using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public class ConnectToServer : MonoBehaviour
{
    [SerializeField] private string _ip;
    [SerializeField] private ushort _port;

    public void ConnectWithInternal()
    {
        Connect(_ip, _port);
    }

    public void SetIPAddress(string address)
    {
        _ip = address;
    }

    public void SetPort(string port)
    {
        _port = ushort.Parse(port);
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
