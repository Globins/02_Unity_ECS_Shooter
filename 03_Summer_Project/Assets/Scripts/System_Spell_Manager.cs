using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class SpellManager
{
	//Add default cases for other spells
	public void CallSpell(int spellID, Entity owner, int level, float3 position, Rotation rotation)
	{
		switch(spellID)
		{
			case 1:
				Debug.Log("Honk1");
				break;
			case 2:
				Debug.Log("Honk2");
				break;
			case 3:
				Debug.Log("Honk3");
				break;
			case 4:
				Debug.Log("Honk4");
				break;
			case 5:
				Debug.Log("Honk5");
				break;
			case 6:
				Debug.Log("Honk6");
				break;
			case 7:
				Debug.Log("Honk7");
				break;
			default:
				break;
		}
	}
}
