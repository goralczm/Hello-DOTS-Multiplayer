using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<PlayerInput> playerInput,
            RefRO<PlayerSettings> playerSettings,
            RefRW<LocalTransform> localTransform,
            RefRW<PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<
                RefRO<PlayerInput>,
                RefRO<PlayerSettings>,
                RefRW<LocalTransform>,
                RefRW<PhysicsVelocity>>()
                .WithAll<Simulate>())
        {
            RotatePlayer(localTransform, playerInput, playerSettings.ValueRO.RotationSpeed * SystemAPI.Time.DeltaTime);

            float speed = playerInput.ValueRO.IsSprinting ? playerSettings.ValueRO.SprintSpeed : playerSettings.ValueRO.MoveSpeed;
            MovePlayer(physicsVelocity, playerInput.ValueRO.InputVector, speed);
        }
    }

    private void RotatePlayer(RefRW<LocalTransform> playerTransform, RefRO<PlayerInput> playerInput, float rotationSpeed)
    {
        bool isMoving = playerInput.ValueRO.InputVector.x != 0 || playerInput.ValueRO.InputVector.z != 0;
        if (!isMoving) return;

        float3 targetPosition = playerTransform.ValueRO.Position + playerInput.ValueRO.InputVector;
        float3 moveDirection = targetPosition - playerTransform.ValueRO.Position;

        quaternion targetRotation = quaternion.LookRotation(moveDirection, math.up());
        quaternion slerpedRotation = math.slerp(playerTransform.ValueRO.Rotation, targetRotation,rotationSpeed);
        playerTransform.ValueRW.Rotation = slerpedRotation;
    }

    private void MovePlayer(RefRW<PhysicsVelocity> playerVelocity, float3 direction, float speed)
    {
        float3 targetVelocioty = direction * speed;
        targetVelocioty.y = playerVelocity.ValueRO.Linear.y;
        playerVelocity.ValueRW.Linear = targetVelocioty;
        playerVelocity.ValueRW.Angular = float3.zero;
    }
}
