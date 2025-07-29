using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct DisconnectHandledTag : IComponentData
{

}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ClientDisconnectMonitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRO<ConnectionState> connectionState,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<ConnectionState>
                >()
                .WithNone<DisconnectHandledTag>()
                .WithEntityAccess()
                )
        {
            if (connectionState.ValueRO.CurrentState != ConnectionState.State.Disconnected)
                continue;

            Debug.Log($"Client disconnected: {connectionState.ValueRO.NetworkId}");
            NetworkEvents.s_OnClientDisconnected?.Invoke(connectionState.ValueRO.NetworkId);

            ecb.AddComponent(entity, typeof(DisconnectHandledTag));
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
