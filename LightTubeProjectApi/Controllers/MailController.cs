using Microsoft.AspNetCore.Mvc;

namespace LightTubeProjectApi.Controllers;

public class MailController(DatabaseContext database, MailManager mailManager) : ControllerBase
{
	[Route("/mail-unsubscribe")]
	public async Task<IActionResult> UnsubscribeFromMailingList(string email)
	{
		string? decoded = mailManager.DecodeListUnsubscribeEmail(email);

		string message;
		if (decoded == null)
		{
			message = "Invalid URL.";
		}
		else
		{
			DatabaseEmail? dbEmail = await database.Emails.FindAsync(decoded);
			if (dbEmail == null)
			{
				message = "Invalid URL.";
			}
			else
			{
				database.Emails.Remove(dbEmail);
				await database.SaveChangesAsync();
				message = "Got it! We will no longer send e-mails to you.";
			}
		}

		return Ok(message);
	}
}