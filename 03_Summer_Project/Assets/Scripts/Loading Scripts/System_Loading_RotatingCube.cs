using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

public class System_Loading_RotatingCube : JobComponentSystem
{
    [BurstCompile]
    struct System_Loading_RotatingCubeJob : IJobForEach<Rotation, RotatingLoadingCube>
    {
        public float DeltaTime;        
        public void Execute(ref Rotation rotation, ref RotatingLoadingCube cubeData)
        {
            cubeData.Timer += DeltaTime;
            float currentAngle = 180+20*cubeData.Timer;
            rotation.Value =  Quaternion.AngleAxis(currentAngle, new float3(0,1,0));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        System_Loading_RotatingCubeJob rotateJob = new System_Loading_RotatingCubeJob
        {
            DeltaTime = UnityEngine.Time.deltaTime,
        };
        JobHandle jobHandle = rotateJob.Schedule(this, inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }
}