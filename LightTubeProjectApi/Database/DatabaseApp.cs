using System.Text.Json.Serialization;

namespace LightTubeProjectApi;

public class DatabaseApp
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Author { get; set; }
	public string? AuthorUrl { get; set; }
	[JsonIgnore] public string AuthorEmail { get; set; }
	public string Summary { get; set; }
	public string Screenshots { get; set; }
	public string? SourceUrl { get; set; }
	public string? IssuesUrl { get; set; }
	
	[JsonIgnore] public bool Approved { get; set; }
}