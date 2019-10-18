using Unity.Entities;

public class System_Remove : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.WithAll<Deleted>().ForEach((Entity entity) => 
		{
			PostUpdateCommands.DestroyEntity(entity);
		});
	}
}