/*
*   Function: RoundSpawnProxy.cs
*   Author: Gordon Lobins Jr.
*   Description: Converts the gameobject within the monobehaviour object into an entity. Entities will spawn in a circle pattern.
*
*   Input: Object Prefab
*   Output: Entity
*
*/
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RoundSpawnProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
#pragma warning disable 0649
	[SerializeField] private GameObject _prefab;
	[SerializeField] private int _count;
	[SerializeField] private float _distance;
#pragma warning restore 0649

	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(_prefab);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new RoundSpawnerData()
		{
			Prefab = conversionSystem.GetPrimaryEntity(_prefab),
			Count = _count,
			Distance = _distance,
			RandomSeed = (uint)Random.Range(0, int.MaxValue)
		});
		dstManager.AddComponentData(entity, new SpawnerEnabled());
	}
}