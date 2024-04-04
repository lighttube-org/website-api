using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace LightTubeProjectApi;

public class AdminActionFilterAttribute : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues headers))
			context.Result = new UnauthorizedResult();

		if (headers.Count == 0)
			context.Result = new UnauthorizedResult();

		string[] header = (headers[0] ?? "").Split(" ");
		if (header.Length != 2)
			context.Result = new UnauthorizedResult();

		if (!header[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
			context.Result = new UnauthorizedResult();

		string[] parts = Encoding.UTF8.GetString(Convert.FromBase64String(header[1])).Split(":");
		string username = parts[0];
		string password = string.Join(":", parts[1..]); // RFC2617 section 2, page 6, passwords may contain ':'

		if (username != Environment.GetEnvironmentVariable("LIGHTTUBEAPI_ADMIN_USER") ||
		    password != Environment.GetEnvironmentVariable("LIGHTTUBEAPI_ADMIN_PASS"))
			context.Result = new UnauthorizedResult();
	}
}