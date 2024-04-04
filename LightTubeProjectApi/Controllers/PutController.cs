using System.Text.Json;
using LightTubeProjectApi.ApiModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LightTubeProjectApi.Controllers;

[Route("/put")]
public class PutController(DatabaseContext database, MailManager mailManager) : ControllerBase
{
	private HttpClient client = new();

	[Route("instance")]
	[HttpPut]
	public async Task PutInstance()
	{
		PutInstanceRequest? body = await Request.ReadFromJsonAsync<PutInstanceRequest>();
		if (body is null)
		{
			Response.StatusCode = 400;
			await Response.StartAsync();
			return;
		}

		Uri uri;
		try
		{
			uri = new Uri(body.Url.TrimEnd('/') + "/api/info");
		}
		catch (Exception e)
		{
			Response.StatusCode = 400;
			await Response.StartAsync();
			await Response.WriteAsync("Malformed instance URL");
			return;
		}

		LightTubeInstanceInfo lightTubeInstanceInfo;
		try
		{
			lightTubeInstanceInfo = await client.GetFromJsonAsync<LightTubeInstanceInfo>(uri)!;
		}
		catch (Exception e)
		{
			Response.StatusCode = 400;
			await Response.StartAsync();
			await Response.WriteAsync("Failed to update instance info");
			return;
		}

		DatabaseInstance? entity = new();
		entity.Host = uri.Host;
		entity.AuthorEmail = body.AuthorEmail;
		entity.Country = body.Country;
		entity.Scheme = uri.Scheme;
		entity.IsCloudflare = body.IsCloudflare;
		entity.ApiEnabled = lightTubeInstanceInfo!.AllowsApi;
		entity.ProxyEnabled = lightTubeInstanceInfo.AllowsThirdPartyProxyUsage ? "all" : "local";
		entity.AccountsEnabled = lightTubeInstanceInfo.AllowsNewUsers;
		entity.Approved = false;
		database.Instances.Add(entity);
		await database.SaveChangesAsync();
		
		Response.StatusCode = 200;
		await Response.StartAsync();
	}
}