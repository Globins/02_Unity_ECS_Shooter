/*
*   Function: System_Currency_Buffer.cs
*   Author: Gordon Lobins Jr.
*   Description: Currency Change handles any entity with the currencyBuffer component which will 
*   add or subtract the value from the data inside the player's ReceiveInput Component.
*
*   Input: CurrencyBuffer
*   Output: ReceiveInput Currency Amount
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class System_Currency_Buffer : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity cuurencyBuffer, ref CurrencyBuffer currencyBufferData) =>
        {
            int currencyAddAmount = currencyBufferData.Value;
            Entity currencyTarget = currencyBufferData.Entity;
            Entities.WithAll<ReceiveInput>().ForEach((Entity player, ref ReceiveInput receiveInputData) =>
            {
                if(currencyTarget == player)
                {
                    receiveInputData.Currency += currencyAddAmount;
                    PostUpdateCommands.AddComponent(cuurencyBuffer, new Deleted());
                }
            });
        });
    }
}