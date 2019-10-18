/*
*   Function: System_Enemy_Square_Spawner.cs
*   Author: Gordon Lobins Jr.
*   Description: Spawns objects on random points in a square. And disables after all entities are spawned.
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
public class System_Enemy_Square_Spawner : JobComponentSystem
{
	private EndInitializationEntityCommandBufferSystem commandBuffer;

	protected override void OnCreate()
	{
		commandBuffer = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
	}

	[RequireComponentTag(typeof(SpawnerEnabled))]
	private struct SpawnJob : IJobForEachWithEntity<SpawnerData, Translation>
	{
		public EntityCommandBuffer.Concurrent entityCommandBuffer;

		public void Execute(Entity entity, int index, [ReadOnly] ref SpawnerData spawner, [ReadOnly] ref Translation translation)
		{
			Random rand = new Random(spawner.RandomSeed);
			for (int i = 0; i < spawner.Count; i++)
			{
				Entity instance = entityCommandBuffer.Instantiate(index, spawner.Prefab);
				float3 position = math.transform(float4x4.identity, new float3(rand.NextFloat(translation.Value.x-spawner.Distance, translation.Value.x+spawner.Distance), translation.Value.y+1f, rand.NextFloat(translation.Value.z-spawner.Distance, translation.Value.z+spawner.Distance)));

				entityCommandBuffer.SetComponent(index, instance, new Translation { Value = position });
			}
			entityCommandBuffer.RemoveComponent<SpawnerEnabled>(index, entity);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		JobHandle spawnJob = new SpawnJob()
		{
			entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent()
		}.ScheduleSingle(this, inputDeps);

		commandBuffer.AddJobHandleForProducer(spawnJob);
		return spawnJob;
	}
}