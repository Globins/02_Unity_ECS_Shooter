// using UnityEngine;
// using System.Collections;
// using Unity.Physics;
// using Unity.Entities;
// using Unity.Mathematics;

// public unsafe class Pathfinding_Grid : MonoBehaviour 
// {
// 	public Vector2 gridWorldSize;
// 	public float nodeRadius;
// 	public BlobAssetReference<Unity.Physics.Collider> sphereCollider;
// 	public int timer = 0;
// 	Node[,] grid;

// 	float nodeDiameter;
// 	int gridSizeX, gridSizeY;

// 	void Awake() 
// 	{
// 		var filter = new CollisionFilter()
//         {
//             BelongsTo = 1u << 5,
//             CollidesWith = 1u << 5,
//             GroupIndex = 5
//         };
// 		sphereCollider = Unity.Physics.SphereCollider.Create(float3.zero, nodeRadius/2, filter);
// 		nodeDiameter = nodeRadius*2;
// 		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
// 		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		
// 	}
// 	void FixedUpdate()
// 	{
// 		//Updates after all the entities are initialized
// 		if(timer <= 5)
// 		{
// 			timer++;
// 		}
// 		if(timer == 5)
// 		{
// 			CreateGrid();
// 		}
		
// 	}
// 	void CreateGrid() 
// 	{
// 		grid = new Node[gridSizeX,gridSizeY];
// 		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

// 		for (int x = 0; x < gridSizeX; x ++)
// 		{
// 			for (int y = 0; y < gridSizeY; y ++)
// 			{
// 				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
// 				bool walkable = !(SphereCast(worldPoint, worldPoint, nodeRadius));
// 				grid[x,y] = new Node(walkable,worldPoint);
// 			}
// 		}
// 	}

// 	public Node NodeFromWorldPoint(Vector3 worldPosition) 
// 	{
// 		float percentX = (worldPosition.x - transform.position.x) / gridWorldSize.x + 0.5f - (nodeRadius / gridWorldSize.x);
// 		float percentY = (worldPosition.z - transform.position.z) / gridWorldSize.y + 0.5f - (nodeRadius / gridWorldSize.y);
// 		percentX = Mathf.Clamp01(percentX);
// 		percentY = Mathf.Clamp01(percentY);

// 		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
// 		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
// 		return grid[x,y];
// 	}

// 	void OnDrawGizmos() {
// 		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));

	
// 		if (grid != null) {
// 			foreach (Node n in grid) {
// 				Gizmos.color = (n.walkable)?Color.white:Color.red;
// 				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
// 			}
// 		}
// 	}


//   public unsafe bool SphereCast(float3 RayFrom, float3 RayTo, float radius)
//     {
//         var physicsWorldSystem = Unity.Entities.World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
//         var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

//         Unity.Physics.Collider* test = (Unity.Physics.Collider*)sphereCollider.GetUnsafePtr();
//         ColliderCastInput input = new ColliderCastInput()
//         {
//             Start  = RayFrom,
//             Orientation = quaternion.identity,
//             End = RayTo - RayFrom,
//             Collider = test,
//         };

//         ColliderCastHit hit = new ColliderCastHit();
//         bool haveHit = collisionWorld.CastCollider(input, out hit);
//         if (haveHit)
//         {
//         	return true;
//         }
//         return false;
//     }
// }

// public class Node {
	
// 	public bool walkable;
// 	public Vector3 worldPosition;
	
// 	public Node(bool _walkable, Vector3 _worldPos) {
// 		walkable = _walkable;
// 		worldPosition = _worldPos;
// 	}
// }