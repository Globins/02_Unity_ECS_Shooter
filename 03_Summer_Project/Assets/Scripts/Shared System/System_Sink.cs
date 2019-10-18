/*
*   Function: System_Sink.cs
*   Author: Gordon Lobins Jr.
*   Description: When an entity is dead, it sinks its body to below the surface and deletes it.
*
*   Input: Entity with Dead Component
*   Output: Entity Deletion
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Jobs;

public class System_Sink : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    [RequireComponentTag(typeof(Dead))]
    private struct DeathSinkJob : IJobForEachWithEntity<PhysicsVelocity, Translation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
    
        public void Execute(Entity entity, int index, ref PhysicsVelocity velocity, [ReadOnly] ref Translation position)
        {
            velocity.Linear.x = 0;
            velocity.Linear.z = 0;
            entityCommandBuffer.RemoveComponent<PhysicsCollider>(index, entity);
            if(position.Value.y < -3)
                entityCommandBuffer.DestroyEntity(index, entity);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new DeathSinkJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
        };
        inputDeps = job.Schedule(this, inputDeps);
        inputDeps.Complete();
        commandBuffer.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
