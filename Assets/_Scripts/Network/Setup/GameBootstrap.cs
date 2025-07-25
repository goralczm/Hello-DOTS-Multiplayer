using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
#if UNITY_EDITOR
        AutoConnectPort = 7979;
        return base.Initialize(defaultWorldName);
#elif UNITY_SERVER
        World server = CreateServerWorld("Server");

        ushort port = ushort.Parse(Environment.GetEnvironmentVariable("PORT") ?? "7979");

        Entity listenRequest = server.EntityManager.CreateEntity(typeof(NetworkStreamRequestListen));
        server.EntityManager.SetComponentData(listenRequest, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4.WithPort(port) });
#else
        CreateClientWorld("Client");
#endif
        return true;
    }
}