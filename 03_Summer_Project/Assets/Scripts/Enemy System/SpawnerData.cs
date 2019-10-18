using Unity.Entities;

public struct SpawnerData : IComponentData
{
	public Entity Prefab;
	public int Count;
	public float Distance;
	public uint RandomSeed;
}
public struct RoundSpawnerData : IComponentData
{
	public Entity Prefab;
	public int Count;
	public float Distance;
	public uint RandomSeed;
}