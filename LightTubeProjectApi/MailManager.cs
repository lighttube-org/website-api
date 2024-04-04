using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Text;
using System.Web;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

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

	private string jwtKey = Environment.GetEnvironmentVariable("LIGHTTUBEAPI_UNSUBSCRIBE_URL")!;
	private static readonly IJwtAlgorithm Algorithm = new HMACSHA256Algorithm();
	private static readonly IBase64UrlEncoder UrlEncoder = new JwtBase64UrlEncoder();
	private static readonly IJsonSerializer Serializer = new SystemTextSerializer();
	private static readonly IDateTimeProvider Provider = new UtcDateTimeProvider();
	private static readonly IJwtValidator Validator = new JwtValidator(Serializer, Provider);
	private static readonly IJwtEncoder Encoder = new JwtEncoder(Algorithm, Serializer, UrlEncoder);
	private static readonly IJwtDecoder Decoder = new JwtDecoder(Serializer, Validator, UrlEncoder, Algorithm);

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

	public string GetListUnsubscribe(string email)
	{
		
		Dictionary<string, object> payload = new()
		{
			{ "iat", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
			{ "sub", email }
		};
		return Environment.GetEnvironmentVariable("LIGHTTUBEAPI_UNSUBSCRIBE_URL") + "?email=" +
		       HttpUtility.UrlEncode(Encoder.Encode(payload, jwtKey));
	}

	public string? DecodeListUnsubscribeEmail(string encoded)
	{
		try
		{
			string json = Decoder.Decode(encoded, jwtKey)!;
			Dictionary<string, object> deserialize =
				(Dictionary<string, object>)Serializer.Deserialize(typeof(Dictionary<string, object>), json);
			return (string)deserialize["sub"];
		}
		catch (Exception)
		{
			return null;
		}
	}
}