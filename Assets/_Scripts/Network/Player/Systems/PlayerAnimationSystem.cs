using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Playables;

[UpdateInGroup(typeof(PresentationSystemGroup))]
partial struct PlayerAnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        HandlePlayersWithoutVisuals(ref state, entityCommandBuffer);
        HandlePlayerAnimation(ref state);
        HandleDisconnectedPlayers(ref state, entityCommandBuffer);

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void HandlePlayersWithoutVisuals(ref SystemState state, EntityCommandBuffer entityCommandBuffer)
    {
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
            CreatePlayerVisuals(playerModels, ghostOwner, entity, entityCommandBuffer);
        }
    }

    private void CreatePlayerVisuals(PlayerModels playerModels, RefRO<GhostOwner> ghostOwner, Entity entity, EntityCommandBuffer entityCommandBuffer)
    {
        GameObject playerPrefab = Object.Instantiate(playerModels.Models[ghostOwner.ValueRO.NetworkId % playerModels.Models.Length]);
        PlayerAnimatorReference animatorReference = new PlayerAnimatorReference
        {
            Animator = playerPrefab.GetComponent<Animator>()
        };

        entityCommandBuffer.AddComponent(entity, animatorReference);
    }

    private void HandlePlayerAnimation(ref SystemState state)
    {
        foreach ((
            LocalTransform localTransform,
            PlayerAnimatorReference animatorReference,
            RefRO<PlayerState> playerState)
            in SystemAPI.Query<
                LocalTransform,
                PlayerAnimatorReference,
                RefRO<PlayerState>>())
        {
            AnimatePlayer(animatorReference, playerState, localTransform);
        }
    }

    private void AnimatePlayer(PlayerAnimatorReference animatorReference, RefRO<PlayerState> playerState, LocalTransform playerTransform)
    {
        animatorReference.Animator.SetFloat("Speed", playerState.ValueRO.Speed);
        animatorReference.Animator.transform.position = playerTransform.Position;
        animatorReference.Animator.transform.rotation = playerTransform.Rotation;
    }

    private void HandleDisconnectedPlayers(ref SystemState state, EntityCommandBuffer entityCommandBuffer)
    {
        foreach ((
            PlayerAnimatorReference animatorReference,
            Entity entity)
            in SystemAPI.Query<
                PlayerAnimatorReference>()
                .WithNone<PlayerModels, LocalTransform>()
                .WithEntityAccess())
        {
            DestroyDisconnectedPlayerVisuals(animatorReference, entity, entityCommandBuffer);
        }
    }

    private void DestroyDisconnectedPlayerVisuals(PlayerAnimatorReference animatorReference, Entity entity, EntityCommandBuffer entityCommandBuffer)
    {
        Object.Destroy(animatorReference.Animator.gameObject);
        entityCommandBuffer.RemoveComponent<PlayerAnimatorReference>(entity);
    }
}
