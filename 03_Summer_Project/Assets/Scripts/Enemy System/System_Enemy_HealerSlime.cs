using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

public class System_SlimeHealer : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(GrasslandBossHealerSlime))]
    [ExcludeComponent(typeof(Dead), typeof(LockedToTarget))]
    private struct TargetAcquireBehaviorJob : IJobForEachWithEntity<Translation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<LockedToTarget> LockedToTarget;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> grasslandBossLocation;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> grasslandBossEntity;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            float CLOSEST_DISTANCE = 100000;
            Entity closest = Entity.Null;
            if(!LockedToTarget.Exists(entity))
            {
                for(int i = 0; i < grasslandBossEntity.Length; i++)
                {
                    if(math.distancesq(grasslandBossLocation[i].Value, float3.zero) != 0 &&
                        math.distancesq(grasslandBossLocation[i].Value, translation.Value) < CLOSEST_DISTANCE)
                    {
                        closest = grasslandBossEntity[i];
                        CLOSEST_DISTANCE = math.distancesq(grasslandBossLocation[i].Value, translation.Value);
                    }
                }
                if(closest != Entity.Null)
                {
                    entityCommandBuffer.AddComponent<LockedToTarget>(index, entity, new LockedToTarget{CurrentTarget = closest});
                }
            }
        }
    }

    [RequireComponentTag(typeof(GrasslandBossHealerSlime))]
    [ExcludeComponent(typeof(Dead))]
    struct BasicBehaviorJob : IJobForEachWithEntity<Translation, LockedToTarget>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> accurateGrasslandBossLocation;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(math.distancesq(accurateGrasslandBossLocation[index].Value, float3.zero) != 0 &&
                math.distancesq(accurateGrasslandBossLocation[index].Value, translation.Value) < 40)
            {
                Entity damageBuffer = entityCommandBuffer.CreateEntity(index);
                entityCommandBuffer.AddComponent(index, damageBuffer, new Damaged{Victim = lockedToTargetData.CurrentTarget, DamageAmount = -1});
                entityCommandBuffer.DestroyEntity(index, entity);
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery enemyGrasslandBossQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Grasslands_Boss>(), 
            ComponentType.ReadOnly<GridEntity>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<ProjectileData>());
        NativeArray<Entity> grasslandBossEntity = enemyGrasslandBossQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> grasslandBossLocation = enemyGrasslandBossQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        TargetAcquireBehaviorJob targetJob = new TargetAcquireBehaviorJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            LockedToTarget = GetComponentDataFromEntity<LockedToTarget>(),
            grasslandBossEntity = grasslandBossEntity,
            grasslandBossLocation = grasslandBossLocation,
        };
        JobHandle jobHandle = targetJob.Schedule(this, inputDeps);
        inputDeps.Complete();


        EntityQuery healerLockedToTargetQuery = GetEntityQuery(
            ComponentType.ReadOnly<GrasslandBossHealerSlime>(), 
            ComponentType.ReadOnly<LockedToTarget>(), 
            ComponentType.Exclude<Dead>());

        NativeArray<LockedToTarget> lockedToTargetArray = healerLockedToTargetQuery.ToComponentDataArray<LockedToTarget>(Allocator.TempJob);
        NativeArray<Translation> accurateGrasslandBossLocation = new NativeArray<Translation>(healerLockedToTargetQuery.CalculateEntityCount(),Allocator.TempJob);
        for(int i = 0; i < lockedToTargetArray.Length; i++)
        {
            if(World.Active.EntityManager.Exists(lockedToTargetArray[i].CurrentTarget))
                accurateGrasslandBossLocation[i] = World.Active.EntityManager.GetComponentData<Translation>(lockedToTargetArray[i].CurrentTarget);
        }
        lockedToTargetArray.Dispose();


        BasicBehaviorJob basicJob = new BasicBehaviorJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            accurateGrasslandBossLocation = accurateGrasslandBossLocation,
        };
        jobHandle = basicJob.Schedule(this, jobHandle);
        inputDeps.Complete();

        commandBuffer.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }

}