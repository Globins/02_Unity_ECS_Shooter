using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(BuildPhysicsWorld))]
public class System_Removal : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(DeleteThis))]
    struct System_RemovalJob : IJobForEachWithEntity<Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, ref Translation translation, [ReadOnly] ref Rotation rotation)
        {
            entityCommandBuffer.DestroyEntity(index, entity);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        System_RemovalJob removeJob = new System_RemovalJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
        };
        JobHandle jobHandle = removeJob.Schedule(this, inputDependencies);
        jobHandle.Complete();
        return jobHandle;
    }
}