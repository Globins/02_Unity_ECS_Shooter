/*
*   Function: Player_Proxy.cs
*   Author: Gordon Lobins Jr.
*   Description: Sets up the player information needed.
*
*   Input: Player Information
*   Output: Player Entity
*
*/
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;

public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
#pragma warning disable 0649
	[SerializeField] private float _speed;
	[SerializeField] private int _currentHealth;
	[SerializeField] private float _jumpPower;
	[SerializeField] private int _jumpCount;
	[SerializeField] private GameObject _prefab;
	[SerializeField] private bool Invulnerable;
	[SerializeField] private float3 _spawnPosition;
#pragma warning restore 0649

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(_prefab);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new Player());
		dstManager.AddComponentData(entity, new GridEntity{typeEnum = GridEntity.TypeEnum.Player});

		dstManager.AddComponentData(entity, new ReceiveInput()
		{
			Currency = 0,
			Arrow_Projectile = conversionSystem.GetPrimaryEntity(_prefab),
			SpellID_1 = 1,
			SpellID_2 = 2,
			SpellID_3 = 3,
			SpellID_4 = 4
		});
		dstManager.AddComponentData(entity, new HealthData()
		{
			MaxHealth = _currentHealth,
			CurrentHealth = _currentHealth,
		});
		dstManager.AddComponentData(entity, new MoveData()
		{
			Speed = _speed
		});
		dstManager.AddComponentData(entity, new JumpData()
		{
			JumpPower = _jumpPower,
			JumpCount = _jumpCount
		});

		dstManager.AddComponentData(entity, new Bow());
		//dstManager.AddComponentData(entity, new Bow());
		//dstManager.AddComponentData(entity, new Sword());
		if(Invulnerable)
		{
			dstManager.AddComponentData(entity, new Invulnerable());
		}
	}
}
