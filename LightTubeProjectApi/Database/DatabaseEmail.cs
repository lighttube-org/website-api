using System.ComponentModel.DataAnnotations;

namespace LightTubeProjectApi;

public class DatabaseEmail
{
	[Key]
	public string Email { get; set; }
	public string Name { get; set; }

	public EmailFlags Flags { get; set; }

	public enum EmailFlags
	{
		LIGHTTUBE_UPDATES = 0
	}
}