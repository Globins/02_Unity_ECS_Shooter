/*
*   Function: System_Health.cs
*   Author: Gordon Lobins Jr.
*   Description: 
*   Input:
*   Output:
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Jobs;

public class System_Health : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [ExcludeComponent(typeof(Dead), typeof(Invulnerable))]
    private struct Health_Job : IJobForEachWithEntity<HealthData>
    {
        public EntityCommandBuffer.Concurrent Ecb;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Damaged> DamageBufferInfo;
        public void Execute(Entity entity, int index, ref HealthData data)
        {
            for(int i = 0; i < DamageBufferInfo.Length; i++)
            {
                if(entity == DamageBufferInfo[i].Victim)
                {
                    data.CurrentHealth -= DamageBufferInfo[i].DamageAmount;
                }
            }
            if(data.CurrentHealth <= 0)
            {
                Ecb.AddComponent<Dead>(index, entity, new Dead());
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery damageBufferQuery = GetEntityQuery(typeof(Damaged));
        NativeArray<Damaged> damageBufferInfo = damageBufferQuery.ToComponentDataArray<Damaged>(Allocator.TempJob);
        NativeArray<Entity> damageBufferEntity = damageBufferQuery.ToEntityArray(Allocator.TempJob);

        Health_Job healthJob = new Health_Job
        {
            Ecb = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            DamageBufferInfo = damageBufferInfo,
        };
        inputDeps = healthJob.Schedule(this, inputDeps);
        inputDeps.Complete();

        for(int i = 0; i < damageBufferEntity.Length; i++)
        {
            World.Active.EntityManager.DestroyEntity(damageBufferEntity[i]);
        }
        damageBufferEntity.Dispose();

        commandBuffer.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
