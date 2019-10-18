/*
*   Function: SlimeProxy.cs
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

public class SlimeProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
#pragma warning disable 0649
	[SerializeField] private float _speed;
	[SerializeField] private int _attackDamage;
	[SerializeField] private int _attackRange;
	[SerializeField] private int _attackSpeed;
	[SerializeField] private int _currentHealth;
	[SerializeField] private int _aggressionRadius;
	[SerializeField] private GameObject _currency;

#pragma warning restore 0649

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(_currency);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new Enemy());
		dstManager.AddComponentData(entity, new GridEntity{typeEnum = GridEntity.TypeEnum.Enemy, AggressionRadius = _aggressionRadius});
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
		dstManager.AddComponentData(entity, new AttackData()
		{
			AttackDamage =_attackDamage,
			AttackRange = _attackRange,
			AttackSpeed = _attackSpeed
		});
		dstManager.AddComponentData(entity, new DropCurrencyOnDeath()
		{
			EnemyLevel = 1,
			Currency = conversionSystem.GetPrimaryEntity(_currency),
			Amount = 5
		});
	}
}
