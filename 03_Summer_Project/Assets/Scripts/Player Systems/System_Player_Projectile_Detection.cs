/*
*   Function: System_Player_Projectile_Detection.cs
*   Author: Gordon Lobins Jr.
*   Description: Player Projectiles will lock on to the closest enemy to apply their damage to.
*
*   Input: Player Projectile entity Location, Enemy entity Location
*   Output: LockedToTarget
*
*/
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class System_Player_Projectile_Detetction : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;
    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(Player), typeof(ProjectileData), typeof(TargetProjectile))]
    [ExcludeComponent(typeof(Enemy), typeof(CollisionData))]
    [BurstCompile]
	private struct TrackEnemy : IJobForEachWithEntity<Translation, GridEntity>
	{
		[ReadOnly] public NativeMultiHashMap<int, GridData> GridMultiHashMap;
		public NativeArray<Entity> closestTargetArray;
		public EntityCommandBuffer.Concurrent entityCommandBuffer;
		[ReadOnly] public ComponentDataFromEntity<ProjectileData> ProjectileData;
		[ReadOnly] public ComponentDataFromEntity<Currency> Currency;
		[ReadOnly] public ComponentDataFromEntity<Dead> Dead;
		public void Execute(Entity player, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref GridEntity gridEntity)
		{
			float3 position = translation.Value;
			int hashMapKey = System_Grid.GetPositionHashMapKey(position);
			Entity closest = Entity.Null;
			float closestDist = float.MaxValue;
			FindTarget(hashMapKey, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey-1, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey+1, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey-System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey+System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey-1-System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey-1+System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey+1-System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			FindTarget(hashMapKey+1+System_Grid.GRIDZMULTIPLIER, position, gridEntity, ref closest, ref closestDist);
			closestTargetArray[index] = closest;

		}

		private void FindTarget(int hashMapKey, float3 position, GridEntity gridEntity, ref Entity closest, ref float closestDist)
		{
			GridData GridData;
			NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
			if(GridMultiHashMap.TryGetFirstValue(hashMapKey, out GridData, out nativeMultiHashMapIterator))
			{
				do
				{
					if(GridData.GridEntity.typeEnum != gridEntity.typeEnum && !ProjectileData.Exists(GridData.entity) && !Currency.Exists(GridData.entity) && !Dead.Exists(GridData.entity))
					{
						if(closest == Entity.Null)
						{
							closest = GridData.entity;
							closestDist = math.distancesq(GridData.position, position);
						}
						else if(math.distancesq(GridData.position, position) < closestDist)
						{
							closest = GridData.entity;
							closestDist = math.distancesq(GridData.position, position);
						}
					}
				}
				while(GridMultiHashMap.TryGetNextValue(out GridData, ref nativeMultiHashMapIterator));
			}
		}
	}
	[RequireComponentTag(typeof(Player), typeof(ProjectileData), typeof(TargetProjectile))]
	[ExcludeComponent(typeof(Enemy), typeof(CollisionData))]
	private struct AddEnemies : IJobForEachWithEntity<Translation, GridEntity>
	{
		[DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetArray;
		public EntityCommandBuffer.Concurrent entityCommandBuffer;
		[ReadOnly] public ComponentDataFromEntity<Dead> Dead;
		[ReadOnly] public ComponentDataFromEntity<Enemy> Enemy;
		[ReadOnly] public ComponentDataFromEntity<LockedToTarget> LockedToTarget;
		public void Execute(Entity projectile, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref GridEntity gridEntity)
		{
			if(closestTargetArray[index] != Entity.Null)
			{
				if(!Dead.Exists(closestTargetArray[index]))
				{
					if(LockedToTarget.Exists(projectile))
					{
						entityCommandBuffer.RemoveComponent<LockedToTarget>(index, projectile);
					}
					entityCommandBuffer.AddComponent(index, projectile, new LockedToTarget{CurrentTarget = closestTargetArray[index]});
				}
			}
		}
	}
	
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		EntityQuery projectilePlayer = GetEntityQuery(typeof(TargetProjectile), ComponentType.ReadOnly<ProjectileData>(), ComponentType.ReadOnly<Player>(), ComponentType.Exclude<CollisionData>());
		NativeArray<Entity> closestTargetArray = new NativeArray<Entity>(projectilePlayer.CalculateEntityCount(), Allocator.TempJob);
		TrackEnemy findEnemies = new TrackEnemy
		{
			GridMultiHashMap = System_Grid.GridMultiHashMap,
			entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
			closestTargetArray = closestTargetArray,
			ProjectileData = GetComponentDataFromEntity<ProjectileData>(),
			Currency = GetComponentDataFromEntity<Currency>(),
			Dead = GetComponentDataFromEntity<Dead>(),
		};
		JobHandle jobHandle = findEnemies.Schedule(this, inputDeps);
		jobHandle.Complete();
		AddEnemies addEnemies = new AddEnemies
		{
			entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
			closestTargetArray = closestTargetArray,
			Dead = GetComponentDataFromEntity<Dead>(),
			Enemy = GetComponentDataFromEntity<Enemy>(),
         	LockedToTarget = GetComponentDataFromEntity<LockedToTarget>(),

		};
		jobHandle = addEnemies.Schedule(this, jobHandle);
		jobHandle.Complete();
		commandBuffer.AddJobHandleForProducer(jobHandle);
		return jobHandle;
	}
}


