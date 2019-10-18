/*
*   Function: System_Enemy_Currency_Spawn.cs
*   Author: Gordon Lobins Jr.
*
*   System_Enemy_Currency_Spawn Description: If an object has the component "DropCurrencyOnDeath", when it "dies", it will instantiate X amount of 
*   currency giving an X amount (According to the enemy's level) to the player. The object will be given the Currency component and use the 
*   Player Detection system to lock onto the player. 
*
*   Input: Enemy Death
*   Output: Currency Objects
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class System_Enemy_Currency_Spawn : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    //Gets all enemies that has the Dead Component and can drop orbs.
    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadOnly<Enemy>(),
            ComponentType.ReadOnly<DropCurrencyOnDeath>(),
            ComponentType.ReadOnly<Dead>());
    }
    //Instantiates currency at the dead enemies' location.
    protected override void OnUpdate()
    {
        Entities.With(currentInputReceiverQuery).ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref DropCurrencyOnDeath currencyData) =>
        {
        	for(int i = 0; i < currencyData.Amount; i++)
        	{
                Entity currency = PostUpdateCommands.Instantiate(currencyData.Currency);
                PostUpdateCommands.SetComponent(currency, new Translation{Value = new float3(translation.Value.x, translation.Value.y+.5f, translation.Value.z)});
                PostUpdateCommands.SetComponent(currency, new Rotation{Value = rotation.Value});
                PostUpdateCommands.AddComponent(currency, new Currency{Value = 1*currencyData.EnemyLevel});
                PostUpdateCommands.AddComponent(currency, new GridEntity{typeEnum = GridEntity.TypeEnum.Currency, AggressionRadius = 100});
        	}
        	PostUpdateCommands.RemoveComponent<DropCurrencyOnDeath>(entity);
        });
    }
}