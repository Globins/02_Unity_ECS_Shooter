using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class QuitButtonHandler : MonoBehaviour
{
	public void QuitGame()
	{
            Application.Quit();
	}
}
