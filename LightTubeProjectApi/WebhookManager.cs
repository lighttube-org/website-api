using Serilog;

namespace LightTubeProjectApi;

public class WebhookManager
{
	private string url = Environment.GetEnvironmentVariable("LIGHTTUBEAPI_DISCORD_WEBHOOK")!;
	private HttpClient client = new();

	private async Task SendWebhook(string title, string message, int tries = 0)
	{
		if (tries >= 5) return;
		try
		{
			HttpRequestMessage req = new(HttpMethod.Post, url + "?wait=true");
			req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["content"] = $"> **{title}**\n> {string.Join("\n> ", message.Split("\n"))}"
			});

			HttpResponseMessage httpResponseMessage = await client.SendAsync(req);
			Log.Information("Webhook sent: " + httpResponseMessage.StatusCode);
		}
		catch (Exception)
		{
			await Task.Delay(1000);
			await SendWebhook(title, message, tries + 1);
		}
	}

	public async Task SendInstanceCreatedMessage(string host)
	{
		await SendWebhook("New Instance", $"Instance **{host}** is waiting for your approval!");
	}

	public async Task SendInstanceApprovedMessage(string host)
	{
		await SendWebhook("Instance Approved", $"Instance **{host}** has been approved");
	}

	public async Task SendInstanceRemovedMessage(string host)
	{
		await SendWebhook("Instance Removed", $"Instance **{host}** has been removed from the list");
	}

	public async Task SendErrorMessage(string path, Exception e)
	{
		await SendWebhook($":no_entry: Exception in `{path}`",
			$"**{e.GetType().FullName}:** {e.Message}\n{string.Join('\n', (e.StackTrace ?? "").Split("\n").Where(x => x.Contains("LightTube")))}");
	}
}