using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
partial struct PlayerAnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            PlayerModels playerModels,
            RefRO<GhostOwner> ghostOwner,
            Entity entity)
            in SystemAPI.Query<
                PlayerModels,
                RefRO<GhostOwner>>()
                .WithNone<PlayerAnimatorReference>()
                .WithEntityAccess())
        {
            GameObject playerPrefab = Object.Instantiate(playerModels.Models[ghostOwner.ValueRO.NetworkId % playerModels.Models.Length]);
            PlayerAnimatorReference animatorReference = new PlayerAnimatorReference
            {
                Animator = playerPrefab.GetComponent<Animator>()
            };

            entityCommandBuffer.AddComponent(entity, animatorReference);
        }

        foreach ((
            LocalTransform localTransform,
            PlayerAnimatorReference animatorReference,
            RefRO<PlayerState> playerState)
            in SystemAPI.Query<
                LocalTransform,
                PlayerAnimatorReference,
                RefRO<PlayerState>>())
        {
            animatorReference.Animator.SetFloat("Speed", playerState.ValueRO.Speed);
            animatorReference.Animator.transform.position = localTransform.Position;
            animatorReference.Animator.transform.rotation = localTransform.Rotation;
        }

        foreach ((
            PlayerAnimatorReference animatorReference,
            Entity entity)
            in SystemAPI.Query<
                PlayerAnimatorReference>()
                .WithNone<PlayerModels, LocalTransform>()
                .WithEntityAccess())
        {
            Object.Destroy(animatorReference.Animator.gameObject);
            entityCommandBuffer.RemoveComponent<PlayerAnimatorReference>(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
