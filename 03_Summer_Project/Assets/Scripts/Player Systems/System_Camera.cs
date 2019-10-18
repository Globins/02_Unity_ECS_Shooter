/*
*   Function: System_Camera.cs
*   Author: Gordon Lobins Jr.
*   Description: Changes Camera position to follow the entity that is currently receiving input and not dead. It updates it's translation
*   according to the entity's.
*
*   Input: Player Position
*   Output: Camera Position
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class System_Camera : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    private bool firstFrame = true;
    private float3 offset;

    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.Exclude<Dead>());
    }
    
    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        
        Entities.With(currentInputReceiverQuery).ForEach((Entity entity, ref Translation position) =>
            {
                float3 inputReceiverPos = position.Value;
                if (firstFrame)
                {
                    offset = new float3(mainCamera.transform.position) - inputReceiverPos;
                    firstFrame = false;
                }
                var targetCamPos = inputReceiverPos + offset;
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCamPos, Bootstrap.Settings.CamSmoothing * Time.deltaTime);
            });
    }
}
