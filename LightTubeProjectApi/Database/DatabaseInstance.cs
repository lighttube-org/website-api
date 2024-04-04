using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LightTubeProjectApi;

public class DatabaseInstance
{
	[Key] public string Host { get; set; }
	[JsonIgnore] public string AuthorEmail { get; set; }
	public string Country { get; set; }
	public string Scheme { get; set; }
	public bool IsCloudflare { get; set; }
	public bool ApiEnabled { get; set; }
	public string ProxyEnabled { get; set; }
	public bool AccountsEnabled { get; set; }

	[JsonIgnore] public bool Approved { get; set; }
}