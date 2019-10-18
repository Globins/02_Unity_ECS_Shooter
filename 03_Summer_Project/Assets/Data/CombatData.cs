using UnityEngine;
using Unity.Transforms;
using Unity.Entities;

public struct ProjectileData : IComponentData
{
	public int Damage;
	public float Speed;
}
public struct TargetProjectile : IComponentData{}

public struct ApplyEffects : IComponentData{}

public struct TimeToLive : IComponentData
{
	public float Timer;
	public int Lifespan;
}

public struct AreaOfEffect : IComponentData
{
	public float Radius;
}