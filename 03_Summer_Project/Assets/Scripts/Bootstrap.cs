using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public static Settings Settings;

    public static void NewGame()
    {
    	EntityManager entityManager = World.Active.EntityManager;
        string sceneName = SceneManager.GetActiveScene().name;
        if(sceneName != "MainMenu")
        {
       	    Object.Instantiate(Settings.PlayerPrefab);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var settingsGo = GameObject.Find("Settings");
        Settings = settingsGo?.GetComponent<Settings>();
        Assert.IsNotNull(Settings);
    }
}
