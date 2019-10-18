/*
*   Function: System_Player_Rotate.cs
*   Author: Gordon Lobins Jr.
*   Description: Rotates the player toward mouse location the world. An invisible plane with a raycast layer called "Floor" will follow
*   the camera.
*
*   Input: Mouse location.
*   Output: Player Rotation.
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class System_Player_Rotate : ComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.Exclude<Dead>());
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
            
        float3 mousePos = Input.mousePosition;
        var camRayLen = Bootstrap.Settings.CamRayLen;
        var floor = LayerMask.GetMask("Walkable");

        Entities.With(query).ForEach((Entity entity, ref Translation position, ref Rotation rotation) =>
        {
            var camRay = mainCamera.ScreenPointToRay(mousePos);
            RaycastHit floorHit;
            if(Physics.Raycast(camRay, out floorHit, camRayLen, floor))
            {
                float3 playerToMouse = new float3(floorHit.point)-position.Value;
                playerToMouse.y = 0;
                //Currently a magic number, the number represents the z difference between the plane and the camera to make up for the camera angle
                playerToMouse.z -= 9.150631f;
                rotation.Value = Quaternion.LookRotation(playerToMouse);
            }
        });
    }
}