/*
*   Function: System_Player_Other_Buttons.cs
*   Author: Gordon Lobins Jr.
*   Description: Manages the other buttons in the game.
*
*   Input: Keyboard inputs.
*   Output: Player Keyboard Functions
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class System_Player_Other_Buttons : ComponentSystem
{
    private GameObject pauseScreen;
    protected override void OnUpdate()
    {
        if(GameObject.Find("PauseScreen") != null)
            pauseScreen = GameObject.Find("PauseScreen");
        var entityManager = World.Active.EntityManager;
        if(pauseScreen != null)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(Bootstrap.Settings.isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
            else if(!Bootstrap.Settings.isPaused)
            {
                Time.timeScale = 1f;
                pauseScreen.SetActive(false);
                World.Active.GetExistingSystem<StepPhysicsWorld>().Enabled = true;
            }
        }


    }
    private void Pause()
    {
    	World.Active.GetExistingSystem<StepPhysicsWorld>().Enabled = false;
        Bootstrap.Settings.isPaused = true;
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);
    }
    private void Resume()
    {
    	World.Active.GetExistingSystem<StepPhysicsWorld>().Enabled = true;
        Bootstrap.Settings.isPaused = false;
        pauseScreen.SetActive(false);
    }

}