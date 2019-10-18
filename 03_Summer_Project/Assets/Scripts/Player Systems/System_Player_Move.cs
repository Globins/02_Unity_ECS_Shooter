/*
*   Function: System_Player_Move.cs
*   Author: Gordon Lobins Jr.
*   Description: Moves the player using the keyboard inputs. Speed is determined by the player prefab.
*
*   Input: Keyboard inputs.
*   Output: Player Velocity and Transform
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateBefore(typeof(BuildPhysicsWorld))]
public class System_Player_Move : ComponentSystem
{
    private EntityQuery currentInputReceiverQuery;
    private int jumpCounter = 0;
    protected override void OnCreate()
    {
        currentInputReceiverQuery = GetEntityQuery(
            ComponentType.ReadWrite<PhysicsVelocity>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.ReadOnly<MoveData>(),
            ComponentType.ReadOnly<JumpData>(),
            ComponentType.Exclude<Dead>(),
            ComponentType.Exclude<Stunned>());
    }

    protected override void OnUpdate()
    {
    	float time = Time.deltaTime;
        Entities.With(currentInputReceiverQuery).ForEach((Entity entity, ref PhysicsVelocity velocity, ref MoveData move, ref JumpData jump) =>
        {
        	if(jumpCounter == jump.JumpCount && (int)velocity.Linear.y < 0)
        		jumpCounter = 0;
        	float horizontal = Input.GetAxis("Horizontal") * move.Speed * time;
        	float vertical = Input.GetAxis("Vertical") * move.Speed * time;
        	float upward = Input.GetAxis("Jump") * jump.JumpPower * time;

        	velocity.Linear.z = vertical;
        	velocity.Linear.x = horizontal;

        	if(upward != 0 && jumpCounter < jump.JumpCount && (int)velocity.Linear.y == 0)
        	{
        		velocity.Linear.y = upward;
        		jumpCounter++;
        	}
            velocity.Angular = float3.zero;
        });
    }
}
