using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PlayerVisualsAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject[] _playerPrefabs;

    private class Baker : Baker<PlayerVisualsAuthoring>
    {
        public override void Bake(PlayerVisualsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new PlayerModels
            {
                Models = authoring._playerPrefabs
            });
            AddComponent(entity, new PlayerState());
        }
    }
}

public class PlayerModels : IComponentData
{
    public GameObject[] Models;
}

public class PlayerAnimatorReference : ICleanupComponentData
{
    public Animator Animator;
}

public struct PlayerState : IComponentData
{
    [GhostField] public float Speed;
}