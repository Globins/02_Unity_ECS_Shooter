/*
*   Function: System_Targeted_Projectile_Move.cs
*   Author: Gordon Lobins Jr.
*   Description: Moves the projectile toward the target entity. The Collision is handled in CollisionResolverSystem.
*
*   Input:ProjectileData, LockedToTarget
*   Output: Velocity
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
public class System_Targeted_Projectile_Move : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(TargetProjectile))]
    [BurstCompile]
    private struct Target_Projectile_Move_Job : IJobForEachWithEntity<ProjectileData, Translation, PhysicsVelocity, Rotation, LockedToTarget>
    {
        public float DeltaTime;
        [ReadOnly] public NativeArray<Translation> targetPositionArray;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        public void Execute(Entity entity, int index, [ReadOnly] ref ProjectileData projectileData,
            [ReadOnly] ref Translation translation, ref PhysicsVelocity velocity, ref Rotation rotation, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(math.distance(targetPositionArray[index].Value, float3.zero) != 0 || !Dead.Exists(lockedToTargetData.CurrentTarget))
            {
                float3 direction = targetPositionArray[index].Value - translation.Value;
                velocity.Linear = math.normalize(direction)*1000 * DeltaTime * projectileData.Speed;
                rotation.Value = Quaternion.LookRotation(direction);
            }

        }
    }
    [RequireComponentTag(typeof(TargetProjectile))]
    private struct Target_Projectile_Clean_Up : IJobForEachWithEntity<ProjectileData, LockedToTarget>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> targetPositionArray;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref ProjectileData data, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(Dead.Exists(lockedToTargetData.CurrentTarget))
            {
                entityCommandBuffer.RemoveComponent<TargetProjectile>(index, entity);
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dead = GetComponentDataFromEntity<Dead>();
        EntityQuery chaserQuery = GetEntityQuery(ComponentType.ReadOnly<LockedToTarget>(), ComponentType.ReadOnly<TargetProjectile>());
        NativeArray<LockedToTarget> lockedToTargetArray = chaserQuery.ToComponentDataArray<LockedToTarget>(Allocator.TempJob);
        NativeArray<Translation> targetPositionArray = new NativeArray<Translation>(chaserQuery.CalculateEntityCount(),Allocator.TempJob);
        for(int i = 0; i < lockedToTargetArray.Length; i++)
        {
            if(World.Active.EntityManager.Exists(lockedToTargetArray[i].CurrentTarget) && !dead.Exists(lockedToTargetArray[i].CurrentTarget))
                targetPositionArray[i] = World.Active.EntityManager.GetComponentData<Translation>(lockedToTargetArray[i].CurrentTarget);
        }
        lockedToTargetArray.Dispose();

        Target_Projectile_Move_Job moveJob = new Target_Projectile_Move_Job
        {
            DeltaTime = Time.deltaTime,
            targetPositionArray = targetPositionArray,
            Dead = dead
        };
        inputDeps = moveJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        Target_Projectile_Clean_Up cleanJob = new Target_Projectile_Clean_Up
        {
            targetPositionArray = targetPositionArray,
            Dead = dead,
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
        };
        inputDeps = cleanJob.Schedule(this, inputDeps);
        inputDeps.Complete();
        return inputDeps;
    }
}