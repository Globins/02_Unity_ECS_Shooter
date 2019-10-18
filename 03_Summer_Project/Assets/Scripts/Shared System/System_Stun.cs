/*
*   Function: System_Stun.cs
*   Author: Gordon Lobins Jr.
*   Description: When a stunbuffer is created, the component Stunned is applied to the target entity and is deleted. The stun timer will keep track
*   of its timer and delete the component when it reaches the end of its lifespan.
*
*   Input: StunBuffer
*   Output: Stunned Component
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

public class System_Stun : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct StunJob : IJobForEachWithEntity<StunBuffer>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Stunned> Stunned;
        public void Execute(Entity entity, int index, ref StunBuffer data)
        {
        	if(Stunned.Exists(data.Victim))
        	{
        		entityCommandBuffer.RemoveComponent<Stunned>(index, data.Victim);
        	}
        	entityCommandBuffer.AddComponent(index, data.Victim, new Stunned{Duration = data.Duration, Timer = 0});
        	entityCommandBuffer.DestroyEntity(index, entity);
        }
    }

    [ExcludeComponent(typeof(Dead))]
    private struct StunTimer : IJobForEachWithEntity<Stunned>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float DeltaTime;
        public void Execute(Entity entity, int index, ref Stunned data)
        {
        	data.Timer += DeltaTime;
        	if(data.Timer >= data.Duration)
        	{
        		entityCommandBuffer.RemoveComponent<Stunned>(index, entity);
        	}
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        StunJob applyJob = new StunJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            Stunned = GetComponentDataFromEntity<Stunned>()
        };
        inputDeps = applyJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        StunTimer timeJob = new StunTimer
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            DeltaTime = Time.deltaTime,
        };
        inputDeps = timeJob.Schedule(this, inputDeps);
        inputDeps.Complete();
        commandBuffer.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
