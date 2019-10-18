using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButtonHandler : MonoBehaviour
{
	string currentScene;
	void Awake()
	{
		currentScene = SceneManager.GetActiveScene().name;
	}
	public void ResetLevel()
	{
		
		var entityManager = World.Active.EntityManager;
		var entityArray = entityManager.GetAllEntities();
		foreach (var entity in entityArray)
        {
            entityManager.AddComponentData(entity, new DeleteThis());
        }
		entityArray.Dispose();
		Bootstrap.Settings.isPaused = false;
		if(currentScene == "Grasslands")
        	Loader.Load(Loader.Scene.Grasslands);
        else if(currentScene == "Grassland_Boss")
        {
        	Loader.Load(Loader.Scene.Grassland_Boss);
        }
        else
        {
        	Loader.Load(Loader.Scene.MainMenu);
        }
	}
}
