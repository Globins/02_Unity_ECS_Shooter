/*
*   Function: CollisionEventsSystem.cs
*   Author: Gordon Lobins Jr.
*   Description: Activates if the entity has collision events enabled and it is colliding with another entity.
*
*   Input: CollisonEventEnabled
*   Output: CollisionData
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(StepPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public class CollisionEventsSystem : JobComponentSystem
{
	private EndSimulationEntityCommandBufferSystem _entityCommandBuffer;
	private BuildPhysicsWorld _buildPhysicsWorldSystem;
	private StepPhysicsWorld _stepPhysicsWorldSystem;
	private EndFramePhysicsSystem _endFramePhysicsSystem;

	protected override void OnCreate()
	{
		_entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		_buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
		_endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
	}
	[ExcludeComponent(typeof(DeleteThis))]
	private struct CollisionEventsJob : ICollisionEventsJob
	{
		[ReadOnly] public EntityCommandBuffer CommandBuffer;
		[ReadOnly] public PhysicsWorld PhysicsWorld;
		[ReadOnly] public ComponentDataFromEntity<Currency> Currency;
		[ReadOnly] public ComponentDataFromEntity<Player> Player;

		public unsafe void Execute(CollisionEvent collisionEvent)
		{
			RigidBody bodyA = PhysicsWorld.Bodies[collisionEvent.BodyIndices.BodyAIndex];
			RigidBody bodyB = PhysicsWorld.Bodies[collisionEvent.BodyIndices.BodyBIndex];
			bool IsCollisionEnabled(Collider* collider)
			{
				return ((ConvexColliderHeader*)collider)->Material.EnableCollisionEvents;
			}
			if (IsCollisionEnabled(bodyA.Collider) && Currency.Exists(bodyA.Entity) && Player.Exists(bodyB.Entity))
			{
				CommandBuffer.RemoveComponent(bodyA.Entity, typeof(CollisionData));
				CommandBuffer.AddComponent(bodyA.Entity, new CollisionData { CollidedEntity = bodyB.Entity });
			}
			if (IsCollisionEnabled(bodyB.Collider) && Currency.Exists(bodyB.Entity) && Player.Exists(bodyA.Entity))
			{
				CommandBuffer.RemoveComponent(bodyB.Entity, typeof(CollisionData));
				CommandBuffer.AddComponent(bodyB.Entity, new CollisionData { CollidedEntity = bodyA.Entity });
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		inputDeps = JobHandle.CombineDependencies(inputDeps, _buildPhysicsWorldSystem.FinalJobHandle, _stepPhysicsWorldSystem.FinalSimulationJobHandle);

		JobHandle CollisionEventsJob = new CollisionEventsJob
		{
			CommandBuffer = _entityCommandBuffer.CreateCommandBuffer(),
			PhysicsWorld = _buildPhysicsWorldSystem.PhysicsWorld,
			Currency = GetComponentDataFromEntity<Currency>(),
			Player = GetComponentDataFromEntity<Player>(),
		}.Schedule(_stepPhysicsWorldSystem.Simulation, ref _buildPhysicsWorldSystem.PhysicsWorld, inputDeps);

		_endFramePhysicsSystem.HandlesToWaitFor.Add(CollisionEventsJob);

		_entityCommandBuffer.AddJobHandleForProducer(CollisionEventsJob);

		return CollisionEventsJob;
	}
}