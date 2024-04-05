using System.Text.Json.Serialization;

namespace LightTubeProjectApi.Controllers;

public class LibreCaptchaResponse
{
	[JsonPropertyName("result")]
	public string Result { get; set; }
}