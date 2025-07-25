using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct PlayerEmoteClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (
            RefRO<GhostOwner> ghostOwner
            in SystemAPI.Query<
                RefRO<GhostOwner>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Entity rpcEntity = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(rpcEntity, new FloatAnimatorParameterRpcCommand
                {
                    NetworkId = ghostOwner.ValueRO.NetworkId,
                    ParameterName = "IsWaving",
                    ParameterValue = 1
                });
                entityCommandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest());
            }
        }

        foreach ((
            RefRO<FloatAnimatorParameterRpcCommand> floatAnimatorParameter,
            RefRO<ReceiveRpcCommandRequest> rpcRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<FloatAnimatorParameterRpcCommand>,
                RefRO<ReceiveRpcCommandRequest>>()
                 .WithEntityAccess())
        {
            foreach ((
                PlayerAnimatorReference animatorReference,
                RefRO<GhostOwner> ghostOwner)
                in SystemAPI.Query<
                    PlayerAnimatorReference,
                    RefRO<GhostOwner>>())
            {
                if (ghostOwner.ValueRO.NetworkId != floatAnimatorParameter.ValueRO.NetworkId) continue;

                animatorReference.Animator.SetFloat(floatAnimatorParameter.ValueRO.ParameterName.ToString(), floatAnimatorParameter.ValueRO.ParameterValue);
            }

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
