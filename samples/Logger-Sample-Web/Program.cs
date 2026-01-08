using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

internal class Program
{
	private static void Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.CreateBootstrapLogger();

		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.Services.AddSerilog((services, lc) => lc
		 .ReadFrom.Configuration(builder.Configuration)
	   ).AddHttpContextAccessor();


		WebApplication app = builder.Build();

		app.UseHttpsRedirection();

		string[] summaries =
		[
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		];

		app.MapGet("/weatherforecast", ([FromServices] ILogger<WeatherForecast> logger) =>
		{

			logger.LogInformation("Weather forcast was called");

			WeatherForecast[] forecast = [.. Enumerable.Range(1, 5).Select(index =>
				new WeatherForecast
				(
					DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
					Random.Shared.Next(-20, 55),
					summaries[Random.Shared.Next(summaries.Length)]
				))];
			return forecast;
		});

		app.Run();
	}
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
