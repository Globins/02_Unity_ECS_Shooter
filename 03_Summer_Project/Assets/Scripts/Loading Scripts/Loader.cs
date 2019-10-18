using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;
using System;

public static class Loader
{
	public enum Scene
	{
		Grasslands,
		Grassland_Boss,
		Loading,
		MainMenu,
		Town
	}

	private static Action loaderCallbackAction;

	public static void Load(Scene scene)
	{
		loaderCallbackAction = () =>
		{
			SceneManager.LoadScene(scene.ToString());
		};
		
		SceneManager.LoadScene(Scene.Loading.ToString());
	}

	public static void LoaderCallback()
	{
		if(loaderCallbackAction != null)
		{
			loaderCallbackAction();
			loaderCallbackAction = null;
		}
	}
}
