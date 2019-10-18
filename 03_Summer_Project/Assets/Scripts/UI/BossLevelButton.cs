using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class BossLevelButton : MonoBehaviour
{
	public void BossLevel()
	{
			var entityManager = World.Active.EntityManager;
			var entityArray = entityManager.GetAllEntities();
			foreach (var entity in entityArray)
            {
                entityManager.AddComponentData(entity, new DeleteThis());
            }
			entityArray.Dispose();
			Bootstrap.Settings.isPaused = false;
            Loader.Load(Loader.Scene.Grassland_Boss);
	}
}
