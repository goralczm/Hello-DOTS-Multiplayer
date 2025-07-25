using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct PlayerAnimatorServerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRW<FloatAnimatorParameterRpcCommand> floatAnimatorParameter,
            RefRO<ReceiveRpcCommandRequest> rpcRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRW<FloatAnimatorParameterRpcCommand>,
                RefRO<ReceiveRpcCommandRequest>>()
                .WithEntityAccess())
        {
            Entity broadcastEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(broadcastEntity, floatAnimatorParameter.ValueRO);
            entityCommandBuffer.AddComponent(broadcastEntity, new SendRpcCommandRequest());

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
