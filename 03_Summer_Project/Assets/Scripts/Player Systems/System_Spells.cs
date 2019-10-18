/*
*   Function: System_Player_Spell.cs
*   Author: Gordon Lobins Jr.
*   Description: When the player presses the appropriate key for any spell, it will make a call to the spell manager for the spell ID that 
*   was requested.
*
*   Input: Player Input.
*   Output: Spell call to spellmanager.
*
*/
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

public class System_Player_Spells : ComponentSystem
{
    private EntityQuery query;
    private EntityManager entityManager = World.Active.EntityManager;
    private SpellManager spellManager;
    private float timerOne, timerTwo, timerThree, timerFour = 0;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Player>(),
            ComponentType.ReadOnly<ReceiveInput>(),
            ComponentType.Exclude<Dead>());
        spellManager = new SpellManager();
    }
    protected override void OnUpdate()
    {
        timerOne += Time.deltaTime;
        timerTwo += Time.deltaTime;
        timerThree += Time.deltaTime;
        timerFour += Time.deltaTime;
        Entities.With(query).ForEach((Entity player, ref Translation translation, ref Rotation rotation, ref ReceiveInput input) =>
        {
            if(Input.GetKeyDown(KeyCode.Alpha1) && timerOne >= Bootstrap.Settings.SpellIDCoolDowns[input.SpellID_1])
            {
                timerOne = 0;
                spellManager.CallSpell(input.SpellID_1, player, 1, translation.Value, rotation);
            }
            if(Input.GetKeyDown(KeyCode.Alpha2) && timerTwo >= Bootstrap.Settings.SpellIDCoolDowns[input.SpellID_2])
            {
                timerTwo = 0;
                spellManager.CallSpell(input.SpellID_2, player, 1, translation.Value, rotation);
            }
            if(Input.GetKeyDown(KeyCode.Alpha3) && timerThree >= Bootstrap.Settings.SpellIDCoolDowns[input.SpellID_3])
            {
                timerThree = 0;
                spellManager.CallSpell(input.SpellID_3, player, 1, translation.Value, rotation);
            }
            if(Input.GetKeyDown(KeyCode.Alpha4) && timerFour >= Bootstrap.Settings.SpellIDCoolDowns[input.SpellID_4])
            {
                timerFour = 0;
                spellManager.CallSpell(input.SpellID_4, player, 1, translation.Value, rotation);
            }
        });
    }
}
