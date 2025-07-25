using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

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
            RefRW <PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<
                RefRO<PlayerInput>,
                RefRO<PlayerSettings>,
                RefRW<LocalTransform>,
                RefRW<PhysicsVelocity>>()
                .WithAll<Simulate>())
        {
            float3 targetPosition = localTransform.ValueRO.Position + playerInput.ValueRO.InputVector;
            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            if (playerInput.ValueRO.InputVector.x != 0 || playerInput.ValueRO.InputVector.z != 0)
            {
                moveDirection = math.normalize(moveDirection);
                quaternion targetRotation = quaternion.LookRotation(moveDirection, math.up());
                quaternion slerpedRotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * playerSettings.ValueRO.RotationSpeed);
                localTransform.ValueRW.Rotation = slerpedRotation;
            }

            float speed = playerInput.ValueRO.IsSprinting ? playerSettings.ValueRO.SprintSpeed : playerSettings.ValueRO.MoveSpeed;
            physicsVelocity.ValueRW.Linear = moveDirection * speed;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }
    }
}
