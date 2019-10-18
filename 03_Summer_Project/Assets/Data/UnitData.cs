using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities;
using System;

//Can Receive Input from Player
public struct ReceiveInput : IComponentData
{
	public int Currency;
	public int CurrencyHealingLevel;

	public Entity Arrow_Projectile;

	public int SpellID_1;

	public int SpellID_2;
	
	public int SpellID_3;
	
	public int SpellID_4;
}

//Faction Tags
public struct Player : IComponentData{}
public struct Neutral : IComponentData{}
public struct Enemy : IComponentData{}

public struct HealthData : IComponentData
{
	public int MaxHealth;
	public int CurrentHealth;
}
public struct Dead : IComponentData{}
public struct Deleted : IComponentData{}

public struct DropCurrencyOnDeath : IComponentData
{
	public Entity Currency;
	public int EnemyLevel;
	public int Amount;
}
public struct Currency : IComponentData
{
	public int Value;
}
public struct MoveData : IComponentData
{
	public float Speed;
	public float AttackRange;
}
public struct JumpData : IComponentData
{
	public float JumpPower;
	public int JumpCount;
}
public struct LockedToTarget : IComponentData
{
	public Entity CurrentTarget;
}

public struct Aggressive :IComponentData
{
	public int AggressionRadius;
}
public struct AttackData :IComponentData
{
	public int AttackDamage;
	public float AttackRange;
	public float AttackSpeed;
	public float AttackTimer;
}
public struct RangeData : IComponentData
{
	public Entity Prefab;
	public float ProjectileSpeed;
}

public struct CanShootLine : IComponentData{}
public struct CanShootTarget : IComponentData{}
public struct HasAreaOfEffect : IComponentData{}
public struct Invulnerable : IComponentData{}
//Classes
public struct Bow : IComponentData{}
public struct Sword : IComponentData{}
public struct Scythe : IComponentData{}
//Statuses
public struct StunBuffer : IComponentData
{
	public float Duration;
	public Entity Victim;
}
public struct CurrencyBuffer : IComponentData
{
	public Entity Entity;
	public int Value;
}
public struct Damaged : IComponentData
{
	public int DamageAmount;
	public Entity Victim;
}
public struct Stunned : IComponentData
{
	public float Timer;
	public float Duration;
}
public struct SpawnerEnabled : IComponentData{}
public struct BossMechanic : IComponentData{}

//Relics
//If an entity has this tag, then it will be clickable by the player
// public struct Clickable : IComponentData
// {
//     public float Radius;
//     public float3 CenterOffset;
// }


//List of Units
public struct Slime : IComponentData{}
public struct ShooterSlime : IComponentData{}
public struct TrackerSlime : IComponentData{}
public struct GrasslandBossHealerSlime : IComponentData{}
public struct Grasslands_Boss : IComponentData
{
	public float PrimarySpellCooldown;
	public float SpawnerTimer;
	public float PrimarySpellDuration;
	public uint RandomSeed;
}

public struct FirstPhase : IComponentData{}
public struct SecondPhase : IComponentData
{
	public float SecondPhaseTimer;
}
public struct ThirdPhase : IComponentData
{
	public float ThirdPhaseTimer;
}

public struct TileAsStruct : IComponentData 
{
    public int2 position;
    public int IsWalkable;
}

public struct RotatingLoadingCube : IComponentData
{
	public float Timer;
}

public struct DeleteThis : IComponentData{}
public struct Paused : IComponentData{}