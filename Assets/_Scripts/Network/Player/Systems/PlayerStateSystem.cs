using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEditor;
using UnityEngine.Playables;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct PlayerStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRW<PlayerState> playerState,
            RefRO<PlayerSettings> playerSettings,
            RefRO<PhysicsVelocity> physicsVelocity,
            Entity entity)
            in SystemAPI.Query<
                RefRW<PlayerState>,
                RefRO<PlayerSettings>,
                RefRO<PhysicsVelocity>>()
                .WithEntityAccess())
        {
            UpdatePlayerSpeed(playerState, physicsVelocity, playerSettings.ValueRO.SprintSpeed, SystemAPI.Time.DeltaTime * playerSettings.ValueRO.SpeedDamping);
        }
    }

    private void UpdatePlayerSpeed(RefRW<PlayerState> playerState, RefRO<PhysicsVelocity> playerVelocity, float maxSpeed, float speedDamping)
    {
        float speed = math.abs(math.length(playerVelocity.ValueRO.Linear));
        float normalizedSpeed = speed / maxSpeed;
        float lerpedSpeed = math.lerp(playerState.ValueRO.Speed, normalizedSpeed, speedDamping);
        playerState.ValueRW.Speed = lerpedSpeed;
    }
}
