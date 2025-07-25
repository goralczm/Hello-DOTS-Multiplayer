using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class PlayerInputAuthoring : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _sprintSpeed = 4f;
    [SerializeField] float _rotationSpeed = 10f;
    [SerializeField] float _speedDamping = 5f;

    public class Baker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerInput());
            AddComponent(entity, new PlayerSettings
            {
                MoveSpeed = authoring._moveSpeed,
                SprintSpeed = authoring._sprintSpeed,
                RotationSpeed = authoring._rotationSpeed,
                SpeedDamping = authoring._speedDamping
            });
        }
    }
}

public struct PlayerInput : IInputComponentData
{
    public float3 InputVector;
    public bool IsSprinting;
}

public struct PlayerSettings : IComponentData
{
    public float MoveSpeed;
    public float SprintSpeed;
    public float RotationSpeed;
    public float SpeedDamping;
}