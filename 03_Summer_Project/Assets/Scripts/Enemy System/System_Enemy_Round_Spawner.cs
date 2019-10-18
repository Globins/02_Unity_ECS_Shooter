/*
*   Function: System_Enemy_Circle_Spawner.cs
*   Author: Gordon Lobins Jr.
*   Description: Spawns objects on random points on a circle. And disables after all entities are spawned.
*
*   Input: Spawner Object.
*   Output: Spawned Entities.
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class System_Enemy_Circle_Spawner : JobComponentSystem
{
	private EndInitializationEntityCommandBufferSystem commandBuffer;

	protected override void OnCreate()
	{
		commandBuffer = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
	}

	[RequireComponentTag(typeof(SpawnerEnabled))]
	private struct SpawnJob : IJobForEachWithEntity<RoundSpawnerData, Translation>
	{
		public EntityCommandBuffer.Concurrent entityCommandBuffer;

		public void Execute(Entity entity, int index, [ReadOnly] ref RoundSpawnerData spawner, [ReadOnly] ref Translation translation)
		{
			Random rand = new Random(spawner.RandomSeed);
			for (int i = 0; i < spawner.Count; i++)
			{
				Entity instance = entityCommandBuffer.Instantiate(index, spawner.Prefab);
				float2 direction = rand.NextFloat2Direction()*spawner.Distance;

				entityCommandBuffer.SetComponent(index, instance, new Translation { Value = new float3(translation.Value.x+direction.x, translation.Value.y+1f, translation.Value.z+direction.y) });
			}
			entityCommandBuffer.RemoveComponent<SpawnerEnabled>(index, entity);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		JobHandle spawnJob = new SpawnJob()
		{
			entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
		}.ScheduleSingle(this, inputDeps);

		commandBuffer.AddJobHandleForProducer(spawnJob);
		return spawnJob;
	}
}