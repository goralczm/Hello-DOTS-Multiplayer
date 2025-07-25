using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct PlayerStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<PhysicsVelocity> physicsVelocity,
            RefRO<PlayerSettings> playerSettings,
            RefRW<PlayerState> playerState,
            Entity entity)
            in SystemAPI.Query<
                RefRO<PhysicsVelocity>,
                RefRO<PlayerSettings>,
                RefRW<PlayerState>>()
                .WithEntityAccess())
        {
            float speed = math.abs(math.length(physicsVelocity.ValueRO.Linear));
            float normalizedSpeed = speed / playerSettings.ValueRO.SprintSpeed;
            float lerpedSpeed = math.lerp(playerState.ValueRO.Speed, normalizedSpeed, SystemAPI.Time.DeltaTime * playerSettings.ValueRO.SpeedDamping);
            playerState.ValueRW.Speed = lerpedSpeed;
        }
    }
}
