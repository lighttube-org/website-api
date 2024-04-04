using Microsoft.AspNetCore.Mvc;

namespace LightTubeProjectApi.Controllers;

public class GetController(DatabaseContext database) : ControllerBase
{
	[Route("/instances")]
	public IQueryable<DatabaseInstance> GetPendingInstances() => database.Instances.Where(x => x.Approved);
}