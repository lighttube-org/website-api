using Microsoft.AspNetCore.Mvc;

namespace LightTubeProjectApi.Controllers;

[AdminActionFilter]
[Route("admin")]
public class AdminController(DatabaseContext database, MailManager mailManager, WebhookManager webhookManager) : ControllerBase
{
	[Route("instances/pending")]
	public DatabaseInstance[] GetPendingInstances() => database.Instances.Where(x => !x.Approved).ToArray();

	[Route("instances/approve")]
	public async Task<IActionResult> ApproveInstance(string host)
	{
		DatabaseInstance? instance = await database.Instances.FindAsync(host);
		if (instance is null) return NotFound();

		instance.Approved = true;
		database.Instances.Update(instance);
		if (await database.Emails.FindAsync(instance.AuthorEmail) == null)
			database.Emails.Add(new DatabaseEmail
			{
				Email = instance.AuthorEmail,
				Flags = DatabaseEmail.EmailFlags.LIGHTTUBE_UPDATES
			});
		await database.SaveChangesAsync();

		string message = "Approved instance!";
		try
		{
			await mailManager.SendMail(instance.AuthorEmail, "LightTube Instance Approval",
				$"Your LightTube instance ({instance.Host}) has been approved and will now be displayed in the instances list with the others! You will also receive e-mails for all LightTube updates.\n\nIf you don't want to receive e-mails, please click the link below\n{mailManager.GetListUnsubscribe(instance.AuthorEmail)}");
		}
		catch (Exception e)
		{
			message =
				$"Instance approved, but failed to send an e-mail to `{instance.AuthorEmail}`.\n{e.GetType().FullName} {e.Message}";
		}
		await webhookManager.SendInstanceApprovedMessage(instance.Host);

		return Ok(message);
	}

	[Route("instances/remove")]
	public async Task<IActionResult> RemoveInstance(string host, string reason)
	{
		DatabaseInstance? instance = await database.Instances.FindAsync(host);
		if (instance is null) return NotFound();

		database.Instances.Remove(instance);
		await database.SaveChangesAsync();

		string message = "Removed instance!";
		try
		{
			await mailManager.SendMail(instance.AuthorEmail, "LightTube Instance Removal",
				$"Your LightTube instance ({instance.Host}) has been removed from the instance list for the following reason:\n\n> {reason}\n\nIf you think that this is a mistake, please contact us at lighttube@kuylar.dev");
		}
		catch (Exception e)
		{
			message =
				$"Instance removed, but failed to send an e-mail to `{instance.AuthorEmail}`.\n{e.GetType().FullName} {e.Message}";
		}
		await webhookManager.SendInstanceRemovedMessage(instance.Host);

		return Ok(message);
	}

	[Route("apps/pending")]
	public DatabaseApp[] GetPendingApps() => database.Apps.Where(x => !x.Approved).ToArray();

	[Route("apps/approve")]
	public async Task<IActionResult> ApproveApp(string host)
	{
		DatabaseApp? app = await database.Apps.FindAsync(host);
		if (app is null) return NotFound();

		app.Approved = true;
		database.Apps.Update(app);
		await database.SaveChangesAsync();

		string message = "Approved app!";
		try
		{
			await mailManager.SendMail(app.AuthorEmail, "LightTube App Approval",
				$"Your app ({app.Name}) has been approved and will now be displayed in the instances list with the others!");
		}
		catch (Exception e)
		{
			message =
				$"App approved, but failed to send an e-mail to `{app.AuthorEmail}`.\n{e.GetType().FullName} {e.Message}";
		}

		return Ok(message);
	}

	[Route("apps/remove")]
	public async Task<IActionResult> RemoveApp(string host, string reason)
	{
		DatabaseApp? app = await database.Apps.FindAsync(host);
		if (app is null) return NotFound();

		database.Apps.Remove(app);
		await database.SaveChangesAsync();

		string message = "Removed app!";
		try
		{
			await mailManager.SendMail(app.AuthorEmail, "LightTube App Removal",
				$"Your LightTube app ({app.Name}) has been removed from the apps list for the following reason:\n\n> {reason}\n\nIf you think that this is a mistake, please contact us at lighttube@kuylar.dev");
		}
		catch (Exception e)
		{
			message =
				$"App removed, but failed to send an e-mail to `{app.AuthorEmail}`.\n{e.GetType().FullName} {e.Message}";
		}

		return Ok(message);
	}

	[Route("announce")]
	public async Task<IActionResult> Announce([FromForm] string? title = null, [FromForm] string? body = null)
	{
		if (title == null)
			return BadRequest("title cant be null");
		if (body == null)
			return BadRequest("body cant be null");
		
		Dictionary<string, bool> receiveStatuses = new();
		try
		{
			await webhookManager.PublishWebhook(
				$"<@&1080558298425729135> **{title}**\n\n{body}");
			receiveStatuses.Add("webhook@discord.com", true);
		}
		catch (Exception)
		{
			receiveStatuses.Add("webhook@discord.com", false);
		}

		DatabaseEmail[] emails = database.Emails
			.Where(x => x.Flags.HasFlag(DatabaseEmail.EmailFlags.LIGHTTUBE_UPDATES)).ToArray();
		Console.WriteLine($"Emailing {emails.Length} people...");
		Task[] enumerable = emails.Select(async x =>
		{
			try
			{
				await mailManager.SendMail(x.Email, title, body);
				Console.WriteLine($"Sent announcement email to {x.Email}...");
				receiveStatuses.Add(x.Email, true);
			}
			catch (Exception e)
			{
				receiveStatuses.Add(x.Email, false);
				Console.WriteLine($"Failed to send announcement email to {x.Email}...");
				Console.WriteLine(e);
			}
		}).ToArray();
		Task.WaitAll(enumerable);
		
		return Ok(receiveStatuses);
	}
}