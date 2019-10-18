/*
*   Function: System_Currency.cs
*   Author: Gordon Lobins Jr.
*   Description: Keeps track of the player's current currency amount. If the player presses Q, he will spend currency to heal itself.
*   according to the entity's. It will look for the gameobject with the correct tag (Will not work in OnCreate) and get the text component. 
*   It will then see if the player is pressing Q, if so, it will drain the currency according to the upgrade level. The currency is then 
*   displayed to the UI.
*
*   Input: Player Currency
*   Output: Health and UI information.
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class System_Player_Currency : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    private GameObject currencyObject;	
    private Text currencyText;

    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.Exclude<Dead>());
    }

    protected override void OnUpdate()
    {
        Entities.With(currentInputReceiverQuery).ForEach((Entity entity, ref ReceiveInput data, ref HealthData hp) =>
        {
            if(Input.GetKey(KeyCode.Q) && hp.CurrentHealth <= hp.MaxHealth && data.Currency > 0)
            {
                data.Currency -= 1;
                hp.CurrentHealth += 1*(data.CurrencyHealingLevel+1);
            }
        });

    	
        if (GameObject.FindGameObjectsWithTag("Currency") == null)
            return;
        currencyObject = GameObject.FindGameObjectsWithTag("Currency")[0];
        if (currencyObject.GetComponent<Text>() == null)
            return;
        currencyText = currencyObject.GetComponent<Text>();

        Entities.ForEach((Entity entity, ref ReceiveInput data) =>
        {
        	currencyText.text = data.Currency.ToString();
        });
    }
}
