/*
*   Function: ShootingSlimeProxy.cs
*   Author: Gordon Lobins Jr.
*   Description: 
*   Input:
*   Output:
*
*/

using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Physics;

public class GrasslandBossHealerSlimeProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
#pragma warning disable 0649
	[SerializeField] private float _speed;
	[SerializeField] private int _currentHealth;
	[SerializeField] private int _attackDamage;
	[SerializeField] private float _attackRange;
	[SerializeField] private float _attackSpeed;
	[SerializeField] private GameObject _currency;
#pragma warning restore 0649

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(_currency);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new Enemy());
		dstManager.AddComponentData(entity, new BossMechanic());
		dstManager.AddComponentData(entity, new GrasslandBossHealerSlime());
		dstManager.AddComponentData(entity, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = 1000000});
		dstManager.AddComponentData(entity, new HealthData()
		{
			MaxHealth = _currentHealth,
			CurrentHealth = _currentHealth,
		});
		dstManager.AddComponentData(entity, new MoveData()
		{
			AttackRange = _attackRange,
			Speed = _speed
		});
		dstManager.AddComponentData(entity, new DropCurrencyOnDeath()
		{
			EnemyLevel = 1,
			Currency = conversionSystem.GetPrimaryEntity(_currency),
			Amount = 5
		});
	}
}
