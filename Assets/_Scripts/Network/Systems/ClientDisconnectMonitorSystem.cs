using Unity.Entities;
using Unity.NetCode;

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
            
            NetworkEvents.s_OnClientDisconnected?.Invoke(connectionState.ValueRO.NetworkId);
            ecb.AddComponent(entity, typeof(DisconnectHandledTag));
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
