/*
*   Function: System_Line_Projectile_Move.cs
*   Author: Gordon Lobins Jr.
*   Description: Moves an entity with ProjectileData in a straight line toward its rotation forward vector. The Collision is handled in CollisionResolverSystem.
*	This system serves as the default movement for any projectile.
*
*   Input: ProjectileData, Rotation
*   Output: Velocity
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
public class System_Line_Projectile_Move : JobComponentSystem
{
    [ExcludeComponent(typeof(TargetProjectile))]
    [BurstCompile]
    private struct Line_Projectile_Move_Job : IJobForEach<ProjectileData, PhysicsVelocity, Rotation>
    {
        public float DeltaTime;
        public void Execute([ReadOnly] ref ProjectileData data, ref PhysicsVelocity velocity, [ReadOnly] ref Rotation rotation)
        {
            Quaternion readableRotation = rotation.Value;
            float3 velocityDirection = new float3(2*(readableRotation.x*readableRotation.z +readableRotation.w*readableRotation.y), 
                                    2*(readableRotation.y*readableRotation.z-readableRotation.w*readableRotation.x),
                                    1-2*(readableRotation.x*readableRotation.x +readableRotation.y*readableRotation.y)) * 1000f;
            
            velocity.Linear = velocityDirection * DeltaTime * data.Speed;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Line_Projectile_Move_Job moveJob = new Line_Projectile_Move_Job
        {
            DeltaTime = Time.deltaTime,
        };
        inputDeps = moveJob.Schedule(this, inputDeps);
        inputDeps.Complete();
        return inputDeps;
    }
}