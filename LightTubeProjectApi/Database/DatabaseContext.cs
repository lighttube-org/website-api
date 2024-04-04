using Microsoft.EntityFrameworkCore;

namespace LightTubeProjectApi;

public class DatabaseContext : DbContext
{
	public DbSet<DatabaseInstance> Instances { get; set; }
	public DbSet<DatabaseEmail> Emails { get; set; }
	public DbSet<DatabaseApp> Apps { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.UseNpgsql(
			$"Host={Environment.GetEnvironmentVariable("LIGHTTUBEAPI_DATABASE_HOST") ?? "localhost"};Database=ltprojectapi;Username=ltprojectapi;Password=ltprojectapi");
}