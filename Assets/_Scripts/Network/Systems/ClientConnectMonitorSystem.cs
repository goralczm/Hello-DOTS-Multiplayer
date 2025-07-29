using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct ConnectionHandledTag : IComponentData
{

}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ClientConnectMonitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRO<NetworkStreamConnection> networkStreamConnection,
            RefRO<NetworkId> networkId,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<NetworkStreamConnection>,
                RefRO<NetworkId>
                >()
                .WithNone<ConnectionHandledTag>()
                .WithEntityAccess()
                )
        {
            NetworkEvents.s_OnClientConnected?.Invoke(networkId.ValueRO.Value);
            Debug.Log($"Client connected: [{networkId.ValueRO.Value}]");
            ecb.AddComponent(entity, typeof(ConnectionHandledTag));
            ecb.AddComponent(entity, typeof(ConnectionState));
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
