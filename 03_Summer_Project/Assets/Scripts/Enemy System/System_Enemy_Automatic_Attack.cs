/*
*   Function: System_Automatic_Attack.cs
*   Author: Gordon Lobins Jr.
*   Description: Handles the attack actions when an object is close enough to attack.
*
*   Input: Attack Data
*   Output: Enemy Attack
*
*/
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public class System_Enemy_Automatic_Attack : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

	[RequireComponentTag(typeof(GridEntity), typeof(Enemy))]
    [ExcludeComponent(typeof(RangeData), typeof(Dead), typeof(Stunned))]
    private struct Default_Attack_Job : IJobForEachWithEntity<AttackData, Translation, LockedToTarget>
    {
    	public float DeltaTime;
    	public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        [ReadOnly] public NativeArray<Translation> targetPositionArray;
        public void Execute(Entity entity, int index, ref AttackData attackData, [ReadOnly] ref Translation translation, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(math.distancesq(translation.Value, targetPositionArray[index].Value) <= attackData.AttackRange && !Dead.Exists(lockedToTargetData.CurrentTarget))
            {
                attackData.AttackTimer += DeltaTime;
                if (attackData.AttackTimer >= attackData.AttackSpeed)
                {
                    attackData.AttackTimer = 0f;
                    Entity damageBuffer = entityCommandBuffer.CreateEntity(index);
                    entityCommandBuffer.AddComponent(index, damageBuffer, new Damaged{Victim = lockedToTargetData.CurrentTarget, DamageAmount = attackData.AttackDamage});
                }
            }
            else
            {
                attackData.AttackTimer = 0f;
            }
        }
    }

    [RequireComponentTag(typeof(GridEntity), typeof(Enemy))]
    [ExcludeComponent(typeof(Dead), typeof(Stunned))]
    private struct Line_Projectile_Attack_Job : IJobForEachWithEntity<AttackData, RangeData, Translation, Rotation, LockedToTarget>
    {
        public float DeltaTime;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        [ReadOnly] public ComponentDataFromEntity<CanShootLine> CanShootLine;
        [ReadOnly] public ComponentDataFromEntity<CanShootTarget> CanShootTarget;
        [ReadOnly] public NativeArray<Translation> targetPositionArray;

        public void Execute(Entity entity, int index, ref AttackData data, [ReadOnly] ref RangeData rdata, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(math.distancesq(translation.Value, targetPositionArray[index].Value) <= data.AttackRange && !Dead.Exists(lockedToTargetData.CurrentTarget))
            {
                data.AttackTimer += DeltaTime;
                if (data.AttackTimer >= data.AttackSpeed)
                {
                    data.AttackTimer = 0f;
                    Entity projectileBuffer = entityCommandBuffer.Instantiate(index, rdata.Prefab);
                    entityCommandBuffer.SetComponent(index, projectileBuffer, new Translation{Value = new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z)});
                    entityCommandBuffer.SetComponent(index, projectileBuffer, new Rotation{Value = rotation.Value});
                    entityCommandBuffer.AddComponent(index, projectileBuffer, new Enemy());
                    entityCommandBuffer.AddComponent(index, projectileBuffer, new LockedToTarget{CurrentTarget = lockedToTargetData.CurrentTarget});
                    entityCommandBuffer.AddComponent(index, projectileBuffer, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = 1000});
                    entityCommandBuffer.AddComponent(index, projectileBuffer, new ProjectileData{Damage = data.AttackDamage, Speed = rdata.ProjectileSpeed});
                    if(CanShootLine.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent<TimeToLive>(index, projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 3});
                    }
                    else if(CanShootTarget.Exists(entity))
                    {
                        entityCommandBuffer.AddComponent(index, projectileBuffer, new TargetProjectile());
                        entityCommandBuffer.AddComponent<TimeToLive>(index, projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 7}); 
                    }
                }
            }
            else
            {
                data.AttackTimer = 0f;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery enemyLockedToTargetQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(), 
            ComponentType.ReadOnly<LockedToTarget>(), 
            ComponentType.ReadOnly<GridEntity>(), 
            ComponentType.ReadOnly<AttackData>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<ProjectileData>());
        //Need to clean this part up so enemies dont reach each other's data
        NativeArray<LockedToTarget> lockedToTargetArray = enemyLockedToTargetQuery.ToComponentDataArray<LockedToTarget>(Allocator.TempJob);
        NativeArray<Translation> targetPositionArray = new NativeArray<Translation>(enemyLockedToTargetQuery.CalculateEntityCount(),Allocator.TempJob);
        for(int i = 0; i < lockedToTargetArray.Length; i++)
        {
            if(World.Active.EntityManager.Exists(lockedToTargetArray[i].CurrentTarget) && !GetComponentDataFromEntity<Dead>().Exists(lockedToTargetArray[i].CurrentTarget))
                targetPositionArray[i] = World.Active.EntityManager.GetComponentData<Translation>(lockedToTargetArray[i].CurrentTarget);
        }
        lockedToTargetArray.Dispose();


        Default_Attack_Job attackJob = new Default_Attack_Job
        {
        	DeltaTime = Time.deltaTime,
        	entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            Dead = GetComponentDataFromEntity<Dead>(),
            targetPositionArray = targetPositionArray
        };
        inputDeps = attackJob.Schedule(this, inputDeps);
        inputDeps.Complete();


        Line_Projectile_Attack_Job lineProjectileJob = new Line_Projectile_Attack_Job
        {
            DeltaTime = Time.deltaTime,
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            Dead = GetComponentDataFromEntity<Dead>(),
            CanShootTarget = GetComponentDataFromEntity<CanShootTarget>(),
            CanShootLine= GetComponentDataFromEntity<CanShootLine>(),
            targetPositionArray = targetPositionArray,
        };


        inputDeps = lineProjectileJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        targetPositionArray.Dispose();
        commandBuffer.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}