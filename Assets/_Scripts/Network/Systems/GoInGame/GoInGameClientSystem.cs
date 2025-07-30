using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRO<NetworkId> networkId,
            Entity entity
            ) in SystemAPI.Query<
                RefRO<NetworkId>>()
                .WithNone<NetworkStreamInGame>()
                .WithEntityAccess())
        {
            RequestGoInGame(entityCommandBuffer, entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void RequestGoInGame(EntityCommandBuffer entityCommandBuffer, Entity entity)
    {
        entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

        Entity rpcEntity = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(rpcEntity, new GoInGameRequestRpc());
        entityCommandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest());
    }
}
