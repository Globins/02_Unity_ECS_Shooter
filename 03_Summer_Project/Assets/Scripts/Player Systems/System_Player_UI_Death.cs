/*
*   Function: System_Player_UI_Death.cs
*   Author: Gordon Lobins Jr.
*   Description: UI Actions after player death.
*
*   Input: Player Death
*   Output: Death UI
*
*/
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class System_Player_UI_Death : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    private GameObject gameOverScreen;

    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>());

    }

    protected override void OnUpdate()
    {
        if(GameObject.Find("GameOverScreen") != null)
            gameOverScreen = GameObject.Find("GameOverScreen");
        if(gameOverScreen != null)
        {
            Entities.With(currentInputReceiverQuery).ForEach((Entity entity) =>
            {
                if(GetComponentDataFromEntity<Dead>().Exists(entity))
                    gameOverScreen.SetActive(true);
                else
                    gameOverScreen.SetActive(false);
            });
        }
    }
}