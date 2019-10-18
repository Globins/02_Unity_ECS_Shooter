/*
*   Function: CollisionResultResolverSystem.cs
*   Author: Gordon Lobins Jr.
*   Description: Handles all collision events for entities that have triggers or collison events on (Currency, Projectiles, etc.).
*
*   Input: Collision Data
*   Output: Intended effect of collision
*
*/
using Unity.Entities;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class CollisionResultResolverSystem : ComponentSystem
{
	private EndSimulationEntityCommandBufferSystem _entityCommandBuffer;
	private EntityQuery _projectileQuery;
	private EntityQuery _currencyQuery;
	protected override void OnCreate()
	{
		_entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		_projectileQuery = GetEntityQuery(ComponentType.ReadOnly<CollisionData>(), ComponentType.ReadOnly<ProjectileData>(), ComponentType.Exclude<DeleteThis>());
		_currencyQuery = GetEntityQuery(ComponentType.ReadOnly<CollisionData>(), ComponentType.ReadOnly<Currency>(), ComponentType.Exclude<DeleteThis>());
	}

	protected override void OnUpdate()
	{
		EntityCommandBuffer commandBuffer = _entityCommandBuffer.CreateCommandBuffer();
		Entities.With(_projectileQuery).ForEach((Entity entity, ref CollisionData collisionData, ref ProjectileData projectileData) =>
		{
			if(!EntityManager.HasComponent<ProjectileData>(collisionData.CollidedEntity))
			{
				if(EntityManager.HasComponent<Player>(entity) && EntityManager.HasComponent<Enemy>(collisionData.CollidedEntity))
				{
	                Entity damageBuffer = commandBuffer.CreateEntity();
	                commandBuffer.AddComponent(damageBuffer, new Damaged{Victim = collisionData.CollidedEntity, DamageAmount = projectileData.Damage});
				}
				else 
				if (EntityManager.HasComponent<Enemy>(entity) && EntityManager.HasComponent<Player>(collisionData.CollidedEntity))
				{
	                Entity damageBuffer = commandBuffer.CreateEntity();
	                commandBuffer.AddComponent(damageBuffer, new Damaged{Victim = collisionData.CollidedEntity, DamageAmount = projectileData.Damage});
				}
				commandBuffer.AddComponent(entity, new Deleted());
			}

		});
		Entities.With(_currencyQuery).ForEach((Entity entity, ref CollisionData collisionData, ref Currency currencyData) =>
		{
			if(EntityManager.HasComponent<Player>(collisionData.CollidedEntity) && !EntityManager.HasComponent<ProjectileData>(collisionData.CollidedEntity))
			{
                Entity currencyBuffer = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(currencyBuffer, new CurrencyBuffer{Entity = collisionData.CollidedEntity, Value = currencyData.Value});
                commandBuffer.AddComponent(entity, new Deleted());
			}
		});
	}
}