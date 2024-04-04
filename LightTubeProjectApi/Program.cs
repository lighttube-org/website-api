using Serilog;
using Serilog.Events;
using LightTubeProjectApi;
using Microsoft.EntityFrameworkCore;

LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
	.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
	.Filter.ByExcluding(x => x.MessageTemplate.Text.Contains("Executed DbCommand"))
	.Enrich.FromLogContext()
	.WriteTo.Console(LogEventLevel.Information);
Log.Logger = loggerConfiguration.CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSerilog();
builder.Services.AddSingleton<MailManager>();
builder.Services.AddDbContext<DatabaseContext>();

#pragma warning disable ASP0000
Log.Information("Applying database migrations");
DatabaseContext db = builder.Services.BuildServiceProvider().GetService<DatabaseContext>()!;
foreach (string migration in db.Database.GetAppliedMigrations()) Log.Debug("Applied migration: " + migration);
foreach (string migration in db.Database.GetPendingMigrations()) Log.Information("Pending migration: " + migration);
await db.Database.MigrateAsync();
Log.Information("Database migrated");
#pragma warning restore ASP0000

WebApplication app = builder.Build();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();