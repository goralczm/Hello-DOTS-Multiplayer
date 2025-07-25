using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<PlayerInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            RefRW<PlayerInput> playerInput
            in SystemAPI.Query<
                RefRW<PlayerInput>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            float3 inputVector = new float3();
            if (Input.GetKey(KeyCode.W))
                inputVector.z += 1f;

            if (Input.GetKey(KeyCode.S))
                inputVector.z -= 1f;

            if (Input.GetKey(KeyCode.A))
                inputVector.x -= 1f;

            if (Input.GetKey(KeyCode.D))
                inputVector.x += 1f;

            playerInput.ValueRW.InputVector = inputVector;
            playerInput.ValueRW.IsSprinting = Input.GetKey(KeyCode.LeftShift);
        }
    }
}
