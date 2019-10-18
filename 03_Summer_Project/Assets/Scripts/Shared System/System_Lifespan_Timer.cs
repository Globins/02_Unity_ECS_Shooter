/*
*   Function: System_Lifespan_Timer.cs
*   Author: Gordon Lobins Jr.
*   Description: When the entity's lifespan timer reaches its limit, the system will destroy the entity.
*
*   Input: Lifespan Timer
*   Output: Entity Deletion
*
*/
using Unity.Entities;
using UnityEngine;

public class System_Lifespan_Timer : ComponentSystem
{
    protected override void OnUpdate()
    {
		Entities.WithNone<CollisionData>().ForEach((Entity entity, ref TimeToLive time) =>
		{
            time.Timer += Time.deltaTime;
            if(time.Timer >= time.Lifespan)
            {
                PostUpdateCommands.AddComponent(entity, new Deleted());
            }
		});	
    }
}