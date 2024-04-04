namespace LightTubeProjectApi;

public class LightTubeInstanceInfo
{
	public string Type { get; set; }
	public string Version { get; set; }
	public string Motd { get; set; }
	public bool AllowsApi { get; set; }
	public bool AllowsNewUsers { get; set; }
	public bool AllowsOauthApi { get; set; }
	public bool AllowsThirdPartyProxyUsage { get; set; }
}