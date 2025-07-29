using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.Rendering.UI;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    private int LastNetworkConnectionsCount;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        foreach ((
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<ReceiveRpcCommandRequest>
                >()
                .WithAll<GoInGameRequestRpc>()
                .WithEntityAccess()
                )
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);

            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReferences.PlayerEntityPrefab);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(
                UnityEngine.Random.Range(-5f, 5f), 0, 0
            )));

            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection);

            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value
            });

            entityCommandBuffer.AppendToBuffer(receiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup
            {
                Value = playerEntity
            });

            entityCommandBuffer.DestroyEntity(entity);
        }

        /*foreach ((
            RefRO<NetworkStreamConnection> networkStreamConnection,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<NetworkStreamConnection>
                >()
                .WithNone<ConnectionHandledTag>()
                .WithEntityAccess()
                )
        {
            if (networkStreamConnection.ValueRO.CurrentState != ConnectionState.State.Connected)
                continue;

            entityCommandBuffer.AddComponent(entity, typeof(ConnectionHandledTag));
            NetworkEvents.s_OnClientConnected?.Invoke(null, new NetworkEvents.OnClientConnectedEventArgs());
        }*/

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
