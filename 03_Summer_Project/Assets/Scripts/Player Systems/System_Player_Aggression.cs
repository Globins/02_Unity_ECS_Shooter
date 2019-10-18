/*
*   Function: System_Enemy_Aggression.cs
*   Author: Gordon Lobins Jr.
*   Description: If the player's position, within a 3x3 grid, is within the given entity's aggression radius, then the enemy will be locked
* 	onto the player. It will first get the player position and hashmapkey, then it will send those two pieces of information to the nearby 3x3 grids,
*	via a search function, where it will search for viable enemies (According to their aggression radius) and add the LockedToTarget component where it will follow the specific functions
*	to head toward the player and attack.
*
*   Input: Player entity Location, Enemy entity Location, HashKeyMap
*   Output: Enemy LockedToTarget
*
* This one has a bug
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class System_Player_Sphere : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;
    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(Player))]
    [ExcludeComponent(typeof(ProjectileData), typeof(Enemy), typeof(Dead))]
	private struct AddEnemiesToList : IJobForEachWithEntity<Translation, GridEntity>
	{
		[ReadOnly] public NativeMultiHashMap<int, GridData> GridMultiHashMap;
		[ReadOnly] public ComponentDataFromEntity<LockedToTarget> LockedToTarget;
		[ReadOnly] public ComponentDataFromEntity<ProjectileData> ProjectileData;
		[ReadOnly] public ComponentDataFromEntity<BossMechanic> BossMechanic;
		[ReadOnly] public ComponentDataFromEntity<Dead> Dead;
		[ReadOnly] public ComponentDataFromEntity<CollisionData> CollisionData;
		public EntityCommandBuffer.Concurrent entityCommandBuffer;
		
		public void Execute(Entity player, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref GridEntity gridEntity)
		{
			float3 position = translation.Value;
			int hashMapKey = System_Grid.GetPositionHashMapKey(position);
			FindTarget(hashMapKey, position, gridEntity, index, player);
			FindTarget(hashMapKey-1, position, gridEntity, index, player);
			FindTarget(hashMapKey+1, position, gridEntity, index, player);
			FindTarget(hashMapKey-System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);
			FindTarget(hashMapKey+System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);
			FindTarget(hashMapKey-1-System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);
			FindTarget(hashMapKey-1+System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);
			FindTarget(hashMapKey+1-System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);
			FindTarget(hashMapKey+1+System_Grid.GRIDZMULTIPLIER, position, gridEntity, index, player);

		}

		private void FindTarget(int hashMapKey, float3 position, GridEntity gridEntity, int index, Entity player)
		{
			GridData GridData;
			NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
			if(GridMultiHashMap.TryGetFirstValue(hashMapKey, out GridData, out nativeMultiHashMapIterator))
			{
				do
				{
					if(GridData.GridEntity.typeEnum != gridEntity.typeEnum && 
						math.distancesq(GridData.position, position) <= GridData.GridEntity.AggressionRadius && 
						!LockedToTarget.Exists(GridData.entity) && !BossMechanic.Exists(GridData.entity) && !Dead.Exists(GridData.entity))
					{
						entityCommandBuffer.AddComponent<LockedToTarget>(index, GridData.entity, new LockedToTarget{CurrentTarget = player});
					}
				}
				while(GridMultiHashMap.TryGetNextValue(out GridData, ref nativeMultiHashMapIterator));
			}
		}
	}


	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		AddEnemiesToList findEnemies = new AddEnemiesToList
		{
			GridMultiHashMap = System_Grid.GridMultiHashMap,
			entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
			ProjectileData = GetComponentDataFromEntity<ProjectileData>(),
			LockedToTarget = GetComponentDataFromEntity<LockedToTarget>(),
			Dead = GetComponentDataFromEntity<Dead>(),
			BossMechanic = GetComponentDataFromEntity<BossMechanic>(),
			CollisionData = GetComponentDataFromEntity<CollisionData>(),
		};
		JobHandle jobHandle = findEnemies.Schedule(this, inputDeps);
		jobHandle.Complete();

		commandBuffer.AddJobHandleForProducer(inputDeps);
		return jobHandle;
	}
}
