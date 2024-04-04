using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Text;

namespace LightTubeProjectApi;

public class MailManager
{
	private SmtpClient client = new(Environment.GetEnvironmentVariable("LIGHTTUBEAPI_SMTP_HOST"),
		int.Parse(Environment.GetEnvironmentVariable("LIGHTTUBEAPI_SMTP_PORT") ?? "587"))
	{
		Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("LIGHTTUBEAPI_SMTP_USERNAME"),
			Environment.GetEnvironmentVariable("LIGHTTUBEAPI_SMTP_PASSWORD")),
		DeliveryMethod = SmtpDeliveryMethod.Network,
		EnableSsl = true
	};

	public async Task SendMail(string to, string subject, string body)
	{
		MailMessage message = new();
		message.From = new MailAddress(Environment.GetEnvironmentVariable("LIGHTTUBEAPI_SMTP_USERNAME")!,
			"LightTube Notifications");
		message.To.Add(new MailAddress(to));
		message.Subject = subject;
		message.Body = body;
		// TODO: use JWTs here
		message.Headers.Add("List-Unsubscribe", GetListUnsubscribe(to));

		await client.SendMailAsync(message);
	}

	public string GetListUnsubscribe(string email) =>
		Environment.GetEnvironmentVariable("LIGHTTUBEAPI_UNSUBSCRIBE_URL") + "?email=" +
		Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
}