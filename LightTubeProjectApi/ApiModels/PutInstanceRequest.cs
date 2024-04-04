namespace LightTubeProjectApi.ApiModels;

public class PutInstanceRequest
{
	public string Url { get; set; }
	public string AuthorEmail { get; set; }
	public string Country { get; set; }
	public bool IsCloudflare { get; set; }
}