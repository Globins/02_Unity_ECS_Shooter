/*
*   Function: System_Grid.cs
*   Author: Gordon Lobins Jr.
*   Description: Spatial partition of the game map. Separates entities to be mapped in a native hash map so they can be looked up when that partition is
*	called.
*
*   Input: Entity Positions
*   Output: Native Hash Map
*
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public struct GridData : IComponentData
{
	public Entity entity;
	public float3 position;
	public GridEntity GridEntity;
}

public struct GridEntity : IComponentData
{
	public float AggressionRadius;
	public TypeEnum typeEnum;
	public enum TypeEnum
	{
		Player,
		Enemy,
		Currency,
	}
}


public class System_Grid: ComponentSystem
{
	public static NativeMultiHashMap<int, GridData> GridMultiHashMap;
	public const int GRIDCELLVOLUME = 50;
	public const int GRIDZMULTIPLIER = 1000;
	
	public static int GetPositionHashMapKey(float3 position)
	{
		return (int)((math.floor(position.x/GRIDCELLVOLUME)) +
			(GRIDZMULTIPLIER * math.floor(position.z / GRIDCELLVOLUME)));
	}

	[BurstCompile]
	private struct SetGridDataHashMapJob : IJobForEachWithEntity<Translation, GridEntity>
	{
		public NativeMultiHashMap<int, GridData>.ParallelWriter GridMultiHashMap; 
		public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref GridEntity GridEntity)
		{
			int hashMapKey = GetPositionHashMapKey(translation.Value);
			GridMultiHashMap.Add(hashMapKey, new GridData{entity = entity, position = translation.Value, GridEntity = GridEntity});
		}
	}

	private static void DebugSystem(float3 position)
	{
		Vector3 originCorner = new Vector3(math.floor(position.x/GRIDCELLVOLUME)* GRIDCELLVOLUME, 0f, (GRIDCELLVOLUME * math.floor(position.z / GRIDCELLVOLUME)));
		Debug.DrawLine(originCorner, originCorner+ new Vector3(1,0,0)*GRIDCELLVOLUME);
		Debug.DrawLine(originCorner, originCorner+ new Vector3(0,0,1)*GRIDCELLVOLUME);
		Debug.DrawLine(originCorner+ new Vector3(0,0,1)*GRIDCELLVOLUME, originCorner+ new Vector3(1,0,1)*GRIDCELLVOLUME);
		Debug.DrawLine(originCorner+ new Vector3(1,0,0)*GRIDCELLVOLUME, originCorner+ new Vector3(1,0,1)*GRIDCELLVOLUME);
	}

	protected override void OnCreate()
	{
		GridMultiHashMap = new NativeMultiHashMap<int, GridData>(0, Allocator.Persistent);
		base.OnCreate();
	}
	protected override void OnDestroy()
	{
		GridMultiHashMap.Dispose();
		base.OnDestroy();
	}
	protected override void OnUpdate()
	{
		var camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit floorHit;
		var floor = LayerMask.GetMask("Floor");
		if (Physics.Raycast(camRay, out floorHit, floor))
		{
			DebugSystem(floorHit.point);
		}

		EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(GridEntity));
		GridMultiHashMap.Clear();
		if(entityQuery.CalculateEntityCount() > GridMultiHashMap.Capacity)
		{
			GridMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
		}
		SetGridDataHashMapJob setGridDataHashMapJob = new SetGridDataHashMapJob
		{
			GridMultiHashMap = GridMultiHashMap.AsParallelWriter(),
		};
		JobHandle jobHandle = JobForEachExtensions.Schedule(setGridDataHashMapJob, entityQuery);
		jobHandle.Complete();
		
	}
}
