/*
*   Function: System_Currency_Move.cs
*   Author: Gordon Lobins Jr.
*   Description: Any entity with the Currency Component will move toward the player with the Currency Move System. When the currency collides with the player, it will delete itself and create a 
*   currencyBuffer which will carry the amount to give to the player (Resolved in ColliderResolverSystem). 
*
*   Input: Player Position
*   Output: Currency Movement
*
*/
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;


[UpdateBefore(typeof(BuildPhysicsWorld))]
public class System_Currency_Move : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(Currency), typeof(LockedToTarget))]
    [BurstCompile]
    private struct Currency_Move_To_Player_Job : IJobForEachWithEntity<Translation, PhysicsVelocity, Rotation>
    {
        public float DeltaTime;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> TargetPositionArray;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, ref PhysicsVelocity velocity, ref Rotation rotation)
        {
            if(math.distance(TargetPositionArray[index].Value, float3.zero) != 0)
            {
                float3 dir = TargetPositionArray[index].Value - translation.Value;
                velocity.Linear = math.normalize(dir)*1000 * DeltaTime * 1f;
                rotation.Value = Quaternion.LookRotation(dir);
            }
        }
    }

    [RequireComponentTag(typeof(Currency))]
    private struct Currency_Clean_Up_Job : IJobForEachWithEntity<PhysicsVelocity, LockedToTarget>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        public void Execute(Entity entity, int index, ref PhysicsVelocity velocity, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(Dead.Exists(lockedToTargetData.CurrentTarget))
            {
                velocity.Linear = float3.zero;
                entityCommandBuffer.RemoveComponent<LockedToTarget>(index, entity);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery currencyWithTargetQuery = GetEntityQuery(ComponentType.ReadOnly<LockedToTarget>(), ComponentType.ReadOnly<Currency>());
        NativeArray<LockedToTarget> currencyLockedToTargetArray = currencyWithTargetQuery.ToComponentDataArray<LockedToTarget>(Allocator.TempJob);
        NativeArray<Translation> targetPositionArray = new NativeArray<Translation>(currencyWithTargetQuery.CalculateEntityCount(),Allocator.TempJob);
        for(int i = 0; i < currencyLockedToTargetArray.Length; i++)
        {
            if(World.Active.EntityManager.Exists(currencyLockedToTargetArray[i].CurrentTarget))
                targetPositionArray[i] = World.Active.EntityManager.GetComponentData<Translation>(currencyLockedToTargetArray[i].CurrentTarget);
        }
        currencyLockedToTargetArray.Dispose();

        Currency_Move_To_Player_Job moveJob = new Currency_Move_To_Player_Job
        {
            DeltaTime = Time.deltaTime,
            TargetPositionArray = targetPositionArray,
        };
        inputDeps = moveJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        Currency_Clean_Up_Job giveJob = new Currency_Clean_Up_Job
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            Dead = GetComponentDataFromEntity<Dead>(),
        };
        inputDeps = giveJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        return inputDeps;
    }
}