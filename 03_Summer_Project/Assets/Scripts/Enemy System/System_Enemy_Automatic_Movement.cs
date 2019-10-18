/*
*   Function: System_Automatic_Movement.cs
*   Author: Gordon Lobins Jr.
*   Description: When an enemy has a LockedToTarget component, it will automatically move toward the player. Pathfinding is not implemented yet.
*
*   Input: LockedToTarget Position
*   Output: Entity velocity
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
public class System_Enemy_Automatic_Movement : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    [ExcludeComponent(typeof(Dead), typeof(Stunned), typeof(ProjectileData))]
    [RequireComponentTag(typeof(Enemy))]
    private struct Check_Target_Status_Job : IJobForEachWithEntity<GridEntity, Translation, PhysicsVelocity, LockedToTarget>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public NativeArray<Translation> targetPositionArray;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        public void Execute(Entity entity, int index, [ReadOnly] ref GridEntity gridData,  [ReadOnly] ref Translation translation,
            ref PhysicsVelocity velocity, [ReadOnly] ref LockedToTarget lockedToTargetData)
        {
            if(Dead.Exists(lockedToTargetData.CurrentTarget) || Dead.Exists(entity) || 
                math.distancesq(targetPositionArray[index].Value, float3.zero) == 0 ||
                math.distancesq(targetPositionArray[index].Value, translation.Value) > gridData.AggressionRadius)
            {
                entityCommandBuffer.RemoveComponent<LockedToTarget>(index, entity);
                velocity.Linear.x = 0;
                velocity.Linear.z = 0;
            }
        }
    }
	[RequireComponentTag(typeof(Enemy), typeof(LockedToTarget))]
    [ExcludeComponent(typeof(Dead), typeof(Stunned), typeof(ProjectileData))]
	[BurstCompile]
    private struct Enemy_Movement_Job : IJobForEachWithEntity<MoveData, Translation, PhysicsVelocity, Rotation>
    {
    	public float DeltaTime;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> targetPositionArray;
        public void Execute(Entity entity, int index, [ReadOnly] ref MoveData moveData,
            [ReadOnly] ref Translation translation, ref PhysicsVelocity velocity, ref Rotation rotate)
        {
            float3 direction = targetPositionArray[index].Value - translation.Value;
            if(math.distance(targetPositionArray[index].Value, float3.zero) != 0)
                rotate.Value = Quaternion.LookRotation(direction);
            direction = math.normalize(direction) * DeltaTime * moveData.Speed;
            if(math.distancesq(targetPositionArray[index].Value, float3.zero) != 0 &&
                math.distancesq(targetPositionArray[index].Value, translation.Value) > moveData.AttackRange)
            {
                velocity.Linear.x = direction.x;
                velocity.Linear.z = direction.z;
            }
            else
            {
                velocity.Linear.x = 0;
                velocity.Linear.z = 0;
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery enemyLockedToTargetQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(), 
            ComponentType.ReadOnly<LockedToTarget>(), 
            ComponentType.ReadOnly<GridEntity>(), 
            ComponentType.ReadOnly<MoveData>(),
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
        
        Check_Target_Status_Job assignjob = new Check_Target_Status_Job
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            targetPositionArray = targetPositionArray,
            Dead = GetComponentDataFromEntity<Dead>()
        };
        JobHandle jobHandle = assignjob.Schedule(this, inputDeps);
        inputDeps.Complete();

        Enemy_Movement_Job movejob = new Enemy_Movement_Job
        {
        	DeltaTime = Time.deltaTime,
            targetPositionArray = targetPositionArray,
        };
        jobHandle = movejob.Schedule(this, jobHandle);
        jobHandle.Complete();

        commandBuffer.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}