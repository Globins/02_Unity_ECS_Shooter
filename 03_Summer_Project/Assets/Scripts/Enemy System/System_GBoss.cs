using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

public class System_GBoss : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBuffer;
    private const int RANDOMSEEDLENGTH = 15;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [ExcludeComponent(typeof(Dead))]
    struct BasicBehaviorJob : IJobForEachWithEntity<Grasslands_Boss, Translation, Rotation, HealthData, LockedToTarget, RangeData>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float DeltaTime;
        [ReadOnly] public ComponentDataFromEntity<SecondPhase> SecondPhase;
        [ReadOnly] public ComponentDataFromEntity<ThirdPhase> ThirdPhase;
        [ReadOnly] public ComponentDataFromEntity<Stunned> Stunned;
        [ReadOnly] public NativeArray<Entity> roundSpawnerEntityArray;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> healerArray;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<uint> randomSeedValuesForPrimarySpell;
        public void Execute(Entity entity, int index, ref Grasslands_Boss bossData, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation, [ReadOnly] ref HealthData health, [ReadOnly] ref LockedToTarget target, [ReadOnly] ref RangeData rdata)
        {
            float boss_percentage = (float)health.CurrentHealth/health.MaxHealth;
            bossData.PrimarySpellCooldown += DeltaTime;
            bossData.SpawnerTimer += DeltaTime;
    		Quaternion angle = rotation.Value;
	       	float3 angles = angle.eulerAngles;
	       	
			if(bossData.SpawnerTimer >= 10 || healerArray.Length == 0)
      		{
      			bossData.SpawnerTimer = 0;
      			for(int i = 0; i < roundSpawnerEntityArray.Length; i++)
      			{
      				entityCommandBuffer.AddComponent(index, roundSpawnerEntityArray[i], new SpawnerEnabled());
      			}
      		}
      		if(!Stunned.Exists(entity))
      		{
		      	if(bossData.PrimarySpellCooldown >= 15 )
		      	{
  		          	bossData.PrimarySpellDuration += DeltaTime;
					for(int i = 0; i < RANDOMSEEDLENGTH; i++)
					{
						Unity.Mathematics.Random rand = new Unity.Mathematics.Random(randomSeedValuesForPrimarySpell[i]);
						Quaternion te = Quaternion.Euler(angles.x+rand.NextFloat(-10, 10), angles.y+rand.NextFloat(-10, 10), angles.z+rand.NextFloat(-10, 10));
						Rotation dupe = rotation;
						dupe.Value = te;
					  	SpawnProjectile(new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z), dupe, rdata.Prefab, target.CurrentTarget, index);
					}
					if(bossData.PrimarySpellDuration >= 3)
					{
						bossData.PrimarySpellCooldown = 0;
						bossData.PrimarySpellDuration = 0;
					}
		      	}
		      	else
		      	{
      				bossData.PrimarySpellCooldown += DeltaTime;
		      	}
      		}

            if(boss_percentage > 1)
            {
                health.CurrentHealth = health.MaxHealth;
            }
            if(boss_percentage <= .5f && !SecondPhase.Exists(entity))
            {
                translation.Value = new float3(0,5f,0);
                rotation.Value = Quaternion.AngleAxis(0, new float3(0,1,0)); 
                Entity stunBuffer = entityCommandBuffer.CreateEntity(index);
                entityCommandBuffer.AddComponent(index, stunBuffer, new StunBuffer{Victim = entity, Duration = 30f});
                entityCommandBuffer.AddComponent<Invulnerable>(index, entity);
                entityCommandBuffer.AddComponent(index, entity, new SecondPhase{SecondPhaseTimer = 0});
            }
            else if(boss_percentage >= .85f && SecondPhase.Exists(entity))
            {
                entityCommandBuffer.RemoveComponent<SecondPhase>(index, entity);
                entityCommandBuffer.RemoveComponent<Invulnerable>(index, entity);
                entityCommandBuffer.RemoveComponent<Stunned>(index, entity);
            }
            if(boss_percentage <= .15f && !ThirdPhase.Exists(entity))
                entityCommandBuffer.AddComponent<ThirdPhase>(index, entity, new ThirdPhase{ThirdPhaseTimer = 0});
            else if(boss_percentage >= .25f && ThirdPhase.Exists(entity))
                entityCommandBuffer.RemoveComponent<ThirdPhase>(index, entity);
        }

        private void SpawnProjectile(float3 position, Rotation rotation, Entity projectile, Entity target, int index)
        {
            Entity projectileBuffer = entityCommandBuffer.Instantiate(index, projectile);
            entityCommandBuffer.SetComponent(index, projectileBuffer, new Translation{Value = position});
            entityCommandBuffer.SetComponent(index, projectileBuffer, new Rotation{Value = rotation.Value});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new Enemy());
            entityCommandBuffer.AddComponent(index, projectileBuffer, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = 1000});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new ProjectileData{Damage = 10, Speed = 5});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 2});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new LockedToTarget{CurrentTarget = target});
        }
    }
    [RequireComponentTag(typeof(Grasslands_Boss))]
    [ExcludeComponent(typeof(Dead))]
    private struct SecondPhaseJob : IJobForEachWithEntity<Translation, Rotation, LockedToTarget, RangeData, SecondPhase>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float DeltaTime;
        [ReadOnly] public NativeArray<Entity> roundSpawnerEntityArray;
        public void Execute(Entity entity, int index, ref Translation translation, [ReadOnly] ref Rotation rotation, 
        	[ReadOnly] ref LockedToTarget target, [ReadOnly] ref RangeData rdata, ref SecondPhase secondPhaseData)
        {
        	secondPhaseData.SecondPhaseTimer += DeltaTime;
	       	float currentAngle = 180+20*secondPhaseData.SecondPhaseTimer;
        	Quaternion angle = rotation.Value;
	       	float3 angles = angle.eulerAngles;
            translation.Value = new float3(0, 5, 0);
        	if(secondPhaseData.SecondPhaseTimer <= 30)
        	{
	      		if(secondPhaseData.SecondPhaseTimer <= .25)
	      		{
	      			for(int i = 0; i < roundSpawnerEntityArray.Length; i++)
	      			{
	      				entityCommandBuffer.AddComponent(index, roundSpawnerEntityArray[i], new SpawnerEnabled());
	      			}
	      		}
			    for(int i = 0; i < 6; i++)
	        	{
				    Quaternion te = Quaternion.Euler(angles.x, angles.y+60*i, angles.z);
				    Rotation dupe = rotation;
				    dupe.Value = te;
	                Translation dupe2 = translation;
	            	dupe2.Value = new float3(translation.Value.x, translation.Value.y-4, translation.Value.z);
	                SpawnProjectile(dupe2.Value, dupe, rdata.Prefab, target.CurrentTarget, index);
	        	}
	        	rotation.Value =  Quaternion.AngleAxis(currentAngle, new float3(0,1,0));
	        	if(secondPhaseData.SecondPhaseTimer  >= 29)
	        		entityCommandBuffer.RemoveComponent<Invulnerable>(index, entity);
        	}

        }

        private void SpawnProjectile(float3 position, Rotation rotation, Entity projectile, Entity target, int index)
        {
            Entity projectileBuffer = entityCommandBuffer.Instantiate(index, projectile);
            entityCommandBuffer.SetComponent(index, projectileBuffer, new Translation{Value = position});
            entityCommandBuffer.SetComponent(index, projectileBuffer, new Rotation{Value = rotation.Value});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new Enemy());
            entityCommandBuffer.AddComponent(index, projectileBuffer, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = 1000});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new ProjectileData{Damage = 10, Speed = 5});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 2});
            entityCommandBuffer.AddComponent(index, projectileBuffer, new LockedToTarget{CurrentTarget = target});
        }
    }
    [RequireComponentTag(typeof(Grasslands_Boss))]
    [ExcludeComponent(typeof(Dead))]
    struct ThirdPhaseJob : IJobForEachWithEntity<Translation, Rotation, LockedToTarget, RangeData, ThirdPhase>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float DeltaTime;
        [ReadOnly] public NativeArray<Entity> roundSpawnerEntityArray;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation, [ReadOnly] ref LockedToTarget target, [ReadOnly] ref RangeData rdata, ref ThirdPhase thirdPhaseData)
        {
        	thirdPhaseData.ThirdPhaseTimer += DeltaTime;
	  		if(thirdPhaseData.ThirdPhaseTimer <= .35)
	  		{
	  			for(int i = 0; i < roundSpawnerEntityArray.Length; i++)
	  			{
	  				entityCommandBuffer.AddComponent(index, roundSpawnerEntityArray[i], new SpawnerEnabled());
	  			}
	  		}
        }
    }
    [RequireComponentTag(typeof(Grasslands_Boss), typeof(Dead))]
    struct DeathPulseJob : IJobForEachWithEntity<Translation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> enemiesAroundBossButNotBoss;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            for(int i = 0; i < enemiesAroundBossButNotBoss.Length; i++)
            {
                entityCommandBuffer.AddComponent(index, enemiesAroundBossButNotBoss[i], new Dead());
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery enemiesAroundBossButNotBossQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(), 
            ComponentType.ReadOnly<GridEntity>(), 
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<ProjectileData>(),
            ComponentType.Exclude<Grasslands_Boss>());

    	EntityQuery spawnerQuery = GetEntityQuery(
            ComponentType.ReadOnly<RoundSpawnerData>());
    	NativeArray<Entity> roundSpawnerEntityArray = spawnerQuery.ToEntityArray(Allocator.TempJob);
    	EntityQuery healerQuery = GetEntityQuery(
            ComponentType.ReadOnly<GrasslandBossHealerSlime>(),
            ComponentType.Exclude<Dead>());
    	NativeArray<Entity> healerArray = healerQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> enemiesAroundBossButNotBoss = enemiesAroundBossButNotBossQuery.ToEntityArray(Allocator.TempJob);
    	NativeArray<uint> randomSeedValuesForPrimarySpell = new NativeArray<uint>(RANDOMSEEDLENGTH, Allocator.TempJob);
    	for(int i = 0; i < randomSeedValuesForPrimarySpell.Length; i++)
    	{
    		randomSeedValuesForPrimarySpell[i] = (uint)UnityEngine.Random.Range(0, int.MaxValue);
    	}
        


        BasicBehaviorJob basicJob = new BasicBehaviorJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            SecondPhase = GetComponentDataFromEntity<SecondPhase>(),
            ThirdPhase = GetComponentDataFromEntity<ThirdPhase>(),
            Stunned = GetComponentDataFromEntity<Stunned>(),
            roundSpawnerEntityArray = roundSpawnerEntityArray,
            randomSeedValuesForPrimarySpell = randomSeedValuesForPrimarySpell,
            healerArray = healerArray,
            DeltaTime = Time.deltaTime,

        };
        JobHandle jobHandle = basicJob.Schedule(this, inputDeps);
        jobHandle.Complete();

        SecondPhaseJob secondPhaseJob = new SecondPhaseJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            DeltaTime = Time.deltaTime,
            roundSpawnerEntityArray = roundSpawnerEntityArray,
        };
        jobHandle = secondPhaseJob.Schedule(this, jobHandle);
        jobHandle.Complete();

        ThirdPhaseJob thirdPhaseJob = new ThirdPhaseJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            roundSpawnerEntityArray = roundSpawnerEntityArray,
            DeltaTime = Time.deltaTime,
        };
        jobHandle = thirdPhaseJob.Schedule(this, jobHandle);
        jobHandle.Complete();

        DeathPulseJob deathPulseJob = new DeathPulseJob
        {
            entityCommandBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent(),
            enemiesAroundBossButNotBoss = enemiesAroundBossButNotBoss
        };
        jobHandle = deathPulseJob.Schedule(this, jobHandle);
        jobHandle.Complete();

        roundSpawnerEntityArray.Dispose();
        commandBuffer.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
