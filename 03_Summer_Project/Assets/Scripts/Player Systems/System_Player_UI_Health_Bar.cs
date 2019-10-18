/*
*   Function: System_Player_Health_Bar.cs
*   Author: Gordon Lobins Jr.
*   Description: Displays the player's health bar to the UI.
*
*   Input: Player Health
*   Output: Health UI
*
*/
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class System_Player_Health_Bar : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    private GameObject healthBar;	
    private Image barImage;

    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadOnly<HealthData>(),
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>());
    }

    protected override void OnUpdate()
    {
        if(GameObject.FindGameObjectsWithTag("Health") == null)
            return;
    	healthBar = GameObject.FindGameObjectsWithTag("Health")[0];


        barImage = healthBar.transform.Find("Bar").GetComponent<Image>();
        Entities.With(currentInputReceiverQuery).ForEach((Entity entity, ref HealthData data) =>
        {
        	float damage = (float)data.CurrentHealth/(float)data.MaxHealth;
        	if(data.CurrentHealth >= 0)
        		barImage.fillAmount = damage;
        });
    }
}
