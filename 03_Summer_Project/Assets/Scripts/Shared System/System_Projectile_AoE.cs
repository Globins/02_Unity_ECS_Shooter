/*
*   Function: System_Projectile_Area_Of_Effect.cs
*   Author: Gordon Lobins Jr.
*   Description: If an entity has projectileData, LockedToTarget, AreaOfEffect and CollisionData, then it will search all objects in a 3x3 grid to
*   find targets within a given radius.
*
*   Input: ProjectileData, AreaofEffect, Native HashMap
*   Output: Area of Effect Damage
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class System_Projectile_Area_Of_Effect : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(CollisionData))]
    private struct Projectile_Area_Of_Effect_Job : IJobForEachWithEntity<ProjectileData, AreaOfEffect, Translation, GridEntity>
    {
        [ReadOnly] public NativeMultiHashMap<int, GridData> GridMultiHashMap;
        [ReadOnly] public ComponentDataFromEntity<Dead> Dead;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref ProjectileData projectileData, [ReadOnly] ref AreaOfEffect areaOfEffectData, 
            [ReadOnly] ref Translation translation, [ReadOnly] ref GridEntity gridEntity)
        {
            int hashMapKey = System_Grid.GetPositionHashMapKey(translation.Value);
            ApplyAoEToSurrounding(index, hashMapKey, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey-1, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey+1, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey-System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey+System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey-1-System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey-1+System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey+1-System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
            ApplyAoEToSurrounding(index, hashMapKey+1+System_Grid.GRIDZMULTIPLIER, translation.Value, gridEntity, areaOfEffectData.Radius, projectileData.Damage);
        }
        private void ApplyAoEToSurrounding(int index, int hashMapKey, float3 position, GridEntity gridEntity, float radius, int damage)
        {
            GridData GridData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(GridMultiHashMap.TryGetFirstValue(hashMapKey, out GridData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if(GridData.GridEntity.typeEnum != gridEntity.typeEnum && math.distancesq(GridData.position, position) <= radius && !Dead.Exists(GridData.entity))
                    {
                        Entity damageBuffer = entityCommandBuffer.CreateEntity(index);
                        entityCommandBuffer.AddComponent(index, damageBuffer, new Damaged{Victim = GridData.entity, DamageAmount = damage});
                    }
                }
                while(GridMultiHashMap.TryGetNextValue(out GridData, ref nativeMultiHashMapIterator));
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Projectile_Area_Of_Effect_Job applyAreaOfEffectJob = new Projectile_Area_Of_Effect_Job
        {
            GridMultiHashMap = System_Grid.GridMultiHashMap,
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            Dead = GetComponentDataFromEntity<Dead>(),
        };
        inputDeps = applyAreaOfEffectJob.Schedule(this, inputDeps);
        inputDeps.Complete();


        commandBuffer.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}