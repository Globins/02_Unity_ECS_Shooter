/*
*   Function: System_Enemy_Grassland_Boss.cs
*   Author: Gordon Lobins Jr.
*   Description: Boss behavior for the grasslands boss. Spaghetti Code at the Moment.
*
*   Input: Boss Data
*   Output: Boss Actions
*
*/
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// public unsafe class System_Enemy_Grassland_Boss : ComponentSystem
// {
//     private EntityQuery bossQuery;
//     private bool sprayOn = false;
//     private float sprayTimer = 0;
//     private float sprayDuration = 0;
//     private bool secondPhase = false;
//     private bool thirdPhase = false;
//     private float secondPhaseTimer = 0;
//     private float thirdPhaseTimer = 0;
//     private float spawnTimer = 0;
//     private float spawnTimerMAX = 15;
//     protected override void OnCreate()
//     {
//         bossQuery = GetEntityQuery(
//         	ComponentType.ReadOnly<Grasslands_Boss>(),
//             ComponentType.Exclude<Dead>());
//     }

//     protected override void OnUpdate()
//     {
    /*    	float time = Time.deltaTime;
        	spawnTimer += Time.deltaTime;
    		EntityQuery chaserQuery = GetEntityQuery(typeof(GrasslandBossHealerSlime),ComponentType.Exclude<Dead>());

            Entities.With(bossQuery).ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref HealthData health, ref LockedToTarget target, ref RangeData rdata) =>
            {
            	if(sprayTimer >= 15 && !secondPhase)
            	{
            		sprayOn = true;
            	}
            	else
            	{
            		sprayTimer += Time.deltaTime;
            	}
            	float3 boss_pos = translation.Value;
            	float boss_health = health.CurrentHealth;
            	float boss_max_hp = health.MaxHealth;
            	float boss_percentage = boss_health/boss_max_hp;
            	int health_boost = 0;
        		Quaternion angle = rotation.Value;
            	float3 angles = angle.eulerAngles;
    	    	Entities.WithNone<SpawnerEnabled>().WithAll<RoundSpawnerData>().ForEach((Entity spawner) =>
    			{
    				if((chaserQuery.CalculateEntityCount() <= 0 || spawnTimer >= spawnTimerMAX))
    				{
    					PostUpdateCommands.AddComponent(spawner, new SpawnerEnabled());
    					spawnTimer = 0;
    				}
    			});

    	        Entities.WithAll<GrasslandBossHealerSlime>().ForEach((Entity healer, ref Translation position, ref HealthData data, ref MoveData move) =>
    	        {
    	        	if(math.distancesq(position.Value, boss_pos) <= 75)
    	        	{
    	        		health_boost += data.CurrentHealth;
    	        		PostUpdateCommands.DestroyEntity(healer);
    	        	}
    	        });
    	        if(health.CurrentHealth < health.MaxHealth)
    	        	health.CurrentHealth += health_boost;
            	float currentAngle = 180;
                //Normal Combat
                if(sprayOn && sprayDuration <= 3 && !secondPhase)
                {
                	sprayDuration += Time.deltaTime;
                    for(int i = 0; i < 15; i++)
                    {
    	                Quaternion te = Quaternion.Euler(angles.x+UnityEngine.Random.Range(-10,10), angles.y+UnityEngine.Random.Range(-10,10), angles.z+UnityEngine.Random.Range(-10,10));
    	                Rotation dupe = rotation;
    	                dupe.Value = te;
                        SpawnProjectile(new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z), dupe, rdata.Prefab, target.CurrentTarget);
                    }
                    if(sprayDuration >= 3)
                    {
    	                sprayTimer = 0;
    	                sprayOn = false;
    	                sprayDuration = 0;
                    }
                }
     
            	if(boss_percentage >= .85f && secondPhase == true)
            	{
            		secondPhase = false;
            		PostUpdateCommands.RemoveComponent<Invulnerable>(entity);
            		PostUpdateCommands.RemoveComponent<Stunned>(entity);
            		secondPhaseTimer = 0;
            		spawnTimerMAX = 15;
            	}
            	if(boss_percentage <= .5f && secondPhase == false)
            	{
            		translation.Value = new float3(0,5f,0);
            		rotation.Value = Quaternion.AngleAxis(0, new float3(0,1,0)); 
            		secondPhase = true;
                    Entity stunBuffer = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(stunBuffer, new StunBuffer{Victim = entity, Duration = 30f});
                    PostUpdateCommands.AddComponent(entity, new Invulnerable());
                    spawnTimerMAX = 10;
            	}



            	if(secondPhase && secondPhaseTimer <= 30)
            	{
            		if(secondPhaseTimer <= .25)
            		{
    			        Entities.WithNone<SpawnerEnabled>().WithAll<RoundSpawnerData>().ForEach((Entity spawner) =>
    			        {
    			        	PostUpdateCommands.AddComponent(spawner, new SpawnerEnabled());
    			        });
            		}
            		secondPhaseTimer += Time.deltaTime;
                	for(int i = 0; i < 6; i++)
                	{
    				    Quaternion te = Quaternion.Euler(angles.x, angles.y+60*i, angles.z);
    				    Rotation dupe = rotation;
    				    dupe.Value = te;
                        Translation dupe2 = translation;
                    	dupe2.Value = new float3(translation.Value.x, translation.Value.y-4, translation.Value.z);
                        SpawnProjectile(dupe2.Value, dupe, rdata.Prefab, target.CurrentTarget);
                	}
                	currentAngle += 40*secondPhaseTimer;
                	rotation.Value =  Quaternion.AngleAxis(currentAngle, new float3(0,1,0));
                	if(secondPhaseTimer >= 29)
                		PostUpdateCommands.RemoveComponent<Invulnerable>(entity);
            	}




            	//Third Phase
            	if(boss_percentage <= .15f && thirdPhase == false)
            	{
            		thirdPhaseTimer += Time.deltaTime;
            		if(thirdPhaseTimer <= .35)
            		{
    			        Entities.WithNone<SpawnerEnabled>().WithAll<RoundSpawnerData>().ForEach((Entity spawner) =>
    			        {
    			        	PostUpdateCommands.AddComponent(spawner, new SpawnerEnabled());
    			        });

            		}
            		else
            		{
            			thirdPhase = true;
            		}
            	}


            });*/
//     }

//     private void SpawnProjectile(float3 position, Rotation rotation, Entity projectile, Entity target)
//     {
//         Entity projectileBuffer = PostUpdateCommands.Instantiate(projectile);
//         PostUpdateCommands.SetComponent(projectileBuffer, new Translation{Value = position});
//         PostUpdateCommands.SetComponent(projectileBuffer, new Rotation{Value = rotation.Value});
//         PostUpdateCommands.AddComponent(projectileBuffer, new Enemy());
//         PostUpdateCommands.AddComponent(projectileBuffer, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = 1000});
//         PostUpdateCommands.AddComponent(projectileBuffer, new ProjectileData{Damage = 10, Speed = 5});
//         PostUpdateCommands.AddComponent(projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 2});
//         PostUpdateCommands.AddComponent(projectileBuffer, new LockedToTarget{CurrentTarget = target});
//     }
// }

