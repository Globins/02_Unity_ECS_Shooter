/*
*   Function: System_Player_Attack.cs
*   Author: Gordon Lobins Jr.
*   Description: This file contains three systems for the basic player attack according to each weapon the player can wield in game.
*   Spaghetti code at the momenet.
*   Input: Player input
*   Output: In Game attacks
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

//The Archer left click will fire a single projectile in a line.
//The weapon upgrade for the left click will add in more projectiles firing in an arc in front of the player.
//The Archer right click fires several projectiles in an arc at random angles all dealing Area of Effect Damage.
//The weapon upgrade for the right click will add in more arrows to fire.
//Both upgrades will include more damage.
public class System_Player_Archer_Attack : ComponentSystem
{
    private EntityQuery query;
    private EntityManager entityManager = World.Active.EntityManager;
    private float activeCooldown = -1;
    private bool activeOn = false;
    private int activeCounter = 0;
    private float attackTimer = 0;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.ReadOnly<Bow>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<Scythe>(),
            ComponentType.Exclude<Sword>());
    }

    protected override void OnUpdate()
    {
        Entities.With(query).ForEach((Entity player, ref Translation translation, ref Rotation rotation, ref ReceiveInput input) =>
        {
            Quaternion angle = rotation.Value;
            float3 dir = new float3(2*(angle.x*angle.z +angle.w*angle.y), 2*(angle.y*angle.z-angle.w*angle.x),1-2*(angle.x*angle.x +angle.y*angle.y))*5f;
            float3 angles = angle.eulerAngles;

            attackTimer += Time.deltaTime;
            if(activeCooldown == -1)
            	activeCooldown = 12-5*Bootstrap.Settings.BowLevel;
            else
            	activeCooldown += Time.deltaTime;


        	if(Input.GetButton("Fire1") && attackTimer >= .2f)
			{
				attackTimer = 0;
				for(int i = 0-(Bootstrap.Settings.BowLevel*2); i < 1+(Bootstrap.Settings.BowLevel*2); i++)
				{
				    Quaternion te = Quaternion.Euler(angles.x, angles.y+5*i, angles.z);
				    Rotation dupe = rotation;
				    dupe.Value = te;
                    SpawnProjectile(new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z), dupe, input.Arrow_Projectile, 5, false);
                    
				}
	        }
        	if(Input.GetMouseButtonDown(1) && activeCooldown >= 0)//12-5*Bootstrap.Settings.BowLevel)
			{
				activeCooldown = 0;
                activeOn = true;
	        }
            if(activeOn)
            {
                activeCounter++;
                Quaternion te = Quaternion.Euler(angles.x, angles.y+UnityEngine.Random.Range(-15,15), angles.z);
                Rotation dupe = rotation;
                dupe.Value = te;
                SpawnProjectile(new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z), dupe, input.Arrow_Projectile, 1, true);
                if(activeCounter >= (Bootstrap.Settings.BowLevel+1)*5)
                {
                    activeOn = false;
                    activeCounter = 0;
                }
            }
        });
    }
    private void SpawnProjectile(float3 position, Rotation rotation, Entity projectile, int damage, bool area)
    {
        Entity projectileBuffer = PostUpdateCommands.Instantiate(projectile);
        position.y -= .5f;
        PostUpdateCommands.SetComponent(projectileBuffer, new Translation{Value = position});
        PostUpdateCommands.SetComponent(projectileBuffer, new Rotation{Value = rotation.Value});
        PostUpdateCommands.AddComponent(projectileBuffer, new ProjectileData{Damage = damage*(Bootstrap.Settings.BowLevel+1), Speed = 5f});
        PostUpdateCommands.AddComponent(projectileBuffer, new GridEntity{typeEnum = GridEntity.TypeEnum.Player, AggressionRadius = 1000});
        PostUpdateCommands.AddComponent(projectileBuffer, new TimeToLive{Timer = 0, Lifespan = 1});
        PostUpdateCommands.AddComponent(projectileBuffer, new Player());
        if(area)
        {
            PostUpdateCommands.AddComponent(projectileBuffer, new AreaOfEffect(){Radius = 100});
        }
    }
}


//The Scythe left click hit all enemies within a radius around the player.
//The scythe upgrade will increase the range of this attack.
//The Scythe right click pulls in all the enemies within a range further than the attack to the player.
//The weapon upgrade will increase the stun time that the enemies will receive.
//Both upgrades will include more damage.
public class System_Player_Scythe_Attack : ComponentSystem
{
    private EntityQuery query;
    private EntityQuery enemyQuery;
    private EntityManager entityManager = World.Active.EntityManager;
    private float activeCooldown = -1;
    private float attackTimer = 0;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.ReadOnly<Scythe>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<Bow>(),
            ComponentType.Exclude<Sword>());
        enemyQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(),
            ComponentType.Exclude<Dead>());
    }

    protected override void OnUpdate()
    {
        Entities.With(query).ForEach((Entity player, ref Translation translation, ref Rotation rotation) =>
        {
        	float3 position = translation.Value;
            int damage = 5*(Bootstrap.Settings.ScytheLevel+1);
            float range = 25*(Bootstrap.Settings.ScytheLevel+1);
            attackTimer += Time.deltaTime;
            if(activeCooldown == -1)
                activeCooldown =  7-2*Bootstrap.Settings.ScytheLevel;
            else
                activeCooldown += Time.deltaTime;


        	if(Input.GetButton("Fire1") && attackTimer >= .25f-.05*Bootstrap.Settings.ScytheLevel)
			{
				attackTimer = 0;
				Entities.With(enemyQuery).ForEach((Entity enemy, ref Translation Victimtranslation) =>
				{
					if(math.distancesq(position, Victimtranslation.Value) <= range)
					{
						Entity damageBuffer = PostUpdateCommands.CreateEntity();
                    	PostUpdateCommands.AddComponent(damageBuffer, new Damaged{Victim = enemy, DamageAmount = damage});
					}
				});
	        }
        	if(Input.GetMouseButtonDown(1) && activeCooldown >= 7-2*Bootstrap.Settings.ScytheLevel)
			{
				activeCooldown = 0;
                Entities.With(enemyQuery).ForEach((Entity enemy, ref Translation Victimtranslation, ref PhysicsVelocity vVelocity) =>
                {
                    if(math.distancesq(position, Victimtranslation.Value) <= range*4)
                    {
/*                        Entity stunBuffer = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(stunBuffer, new StunBuffer{Victim = enemy, Duration = 3f});*/
                        Entity damageBuffer = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(damageBuffer, new Damaged{Victim = enemy, DamageAmount = damage});    
                        Vector3 direction = position-Victimtranslation.Value;
                        vVelocity.Linear = direction*5;
                    }
                });
	        }
        });
    }
}



//The Sword left click hit all enemies within an arc in front of the player.
//The Sword upgrade will increase the speed of this attack.
//The Sword right click pushes away all nearby enemies and stunning them for a period of time.
//The weapon upgrade will increase the stun time that the enemies will receive.
//Both upgrades will include more damage.
public class System_Player_Sword_Attack : ComponentSystem
{
    private EntityQuery query;
    private EntityQuery enemyQuery;
    private EntityManager entityManager = World.Active.EntityManager;
    private float activeCooldown = -1;
    private float attackTimer = 0;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.ReadOnly<Sword>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<Scythe>(),
            ComponentType.Exclude<Bow>());
    	enemyQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(),
            ComponentType.Exclude<Dead>());
    }

    protected override void OnUpdate()
    {   
        Entities.With(query).ForEach((Entity player, ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity) =>
        {
        	float3 position = translation.Value;
            int damage = 15*(Bootstrap.Settings.SwordLevel+1);
            float range = 10f;

        	Quaternion angle = rotation.Value;
        	int attackAngle = 120;
        	float3 dir = new float3(2*(angle.x*angle.z +angle.w*angle.y), 2*(angle.y*angle.z-angle.w*angle.x),1-2*(angle.x*angle.x +angle.y*angle.y))*range;

            attackTimer += Time.deltaTime;
           if(activeCooldown == -1)
            	activeCooldown = 10-2*Bootstrap.Settings.SwordLevel;
            else
            	activeCooldown += Time.deltaTime;
        	if(Input.GetButton("Fire1") && attackTimer >= 1/(Bootstrap.Settings.SwordLevel+1))
			{
				attackTimer = 0;
				Entities.With(enemyQuery).ForEach((Entity enemy, ref Translation Victimtranslation) =>
				{
					if(math.distancesq(position, Victimtranslation.Value) <= range)
					{
						Vector3 dif = Victimtranslation.Value-position;
						float deltaAngle = Vector3.Angle(dif, dir);
						if(attackAngle*.5f >= deltaAngle)
						{
							Entity damageBuffer = PostUpdateCommands.CreateEntity();
                			PostUpdateCommands.AddComponent(damageBuffer, new Damaged{Victim = enemy, DamageAmount = damage});	
						}

					}
				});
	        }
        	if(Input.GetMouseButtonDown(1) && activeCooldown >= 10-2*Bootstrap.Settings.SwordLevel)
			{
				activeCooldown = 0;
                Entities.With(enemyQuery).ForEach((Entity enemy, ref Translation Victimtranslation, ref PhysicsVelocity vVelocity) =>
                {
                    if(math.distancesq(position, Victimtranslation.Value) <= 10)
                    {
                        Entity stunBuffer = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(stunBuffer, new StunBuffer{Victim = enemy, Duration = 3f});
                        Entity damageBuffer = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(damageBuffer, new Damaged{Victim = enemy, DamageAmount = damage});    
                        Vector3 direction = Victimtranslation.Value-position;
                        vVelocity.Linear = direction*5;
                    }
                });
	        }
        });
    }
}