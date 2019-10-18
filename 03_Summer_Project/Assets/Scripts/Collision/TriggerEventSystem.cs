/*
*   Function: TriggerEventsSystem.cs
*   Author: Gordon Lobins Jr.
*   Description: Activates if the entity is a trigger and it is colliding with another entity.
*
*   Input: Trigger
*   Output: CollisionData
*
*/
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(StepPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public class TriggerEventsSystem : JobComponentSystem
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
	private struct TriggerEventsJob : ITriggerEventsJob
	{
		[ReadOnly] public EntityCommandBuffer CommandBuffer;
		[ReadOnly] public PhysicsWorld PhysicsWorld;
		[ReadOnly] public ComponentDataFromEntity<ProjectileData> ProjectileData;
		[ReadOnly] public ComponentDataFromEntity<Currency> Currency;
		[ReadOnly] public ComponentDataFromEntity<Dead> Dead;

		public unsafe void Execute(TriggerEvent triggerEvent)
		{
			RigidBody bodyA = PhysicsWorld.Bodies[triggerEvent.BodyIndices.BodyAIndex];
			RigidBody bodyB = PhysicsWorld.Bodies[triggerEvent.BodyIndices.BodyBIndex];
			bool IsTriggerEnabled(Collider* collider)
			{
				return ((ConvexColliderHeader*)collider)->Material.IsTrigger;
			}
			if (IsTriggerEnabled(bodyA.Collider))
			{
				if(!ProjectileData.Exists(bodyB.Entity) && !Currency.Exists(bodyB.Entity) && !Dead.Exists(bodyB.Entity))
				{
					CommandBuffer.RemoveComponent(bodyA.Entity, typeof(CollisionData));
					CommandBuffer.AddComponent(bodyA.Entity, new CollisionData { CollidedEntity = bodyB.Entity });
				}
			}
			if(IsTriggerEnabled(bodyB.Collider))
			{
				if(!ProjectileData.Exists(bodyA.Entity) && !Currency.Exists(bodyA.Entity) && !Dead.Exists(bodyA.Entity))
				{
					CommandBuffer.RemoveComponent(bodyB.Entity, typeof(CollisionData));
					CommandBuffer.AddComponent(bodyB.Entity, new CollisionData { CollidedEntity = bodyA.Entity });
				}
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		inputDeps = JobHandle.CombineDependencies(inputDeps, _buildPhysicsWorldSystem.FinalJobHandle, _stepPhysicsWorldSystem.FinalSimulationJobHandle);

		JobHandle TriggerEventsJob = new TriggerEventsJob
		{
			CommandBuffer = _entityCommandBuffer.CreateCommandBuffer(),
			PhysicsWorld = _buildPhysicsWorldSystem.PhysicsWorld,
			ProjectileData = GetComponentDataFromEntity<ProjectileData>(),
			Currency = GetComponentDataFromEntity<Currency>(),
			Dead = GetComponentDataFromEntity<Dead>(),
		}.Schedule(_stepPhysicsWorldSystem.Simulation, ref _buildPhysicsWorldSystem.PhysicsWorld, inputDeps);

		_endFramePhysicsSystem.HandlesToWaitFor.Add(TriggerEventsJob);

		_entityCommandBuffer.AddJobHandleForProducer(TriggerEventsJob);

		return TriggerEventsJob;
	}
}