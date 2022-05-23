using WatchDog.Echo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
//builder.Services.AddWatchDogEchoServices();
builder.Services.AddWatchDogEchoServices(opt => 
{ 
    opt.EchoIntervalInMinutes = 0; opt.HostURLs = "https://localhost:5001/, https://localhost:7068/, http://localhost:41560"; opt.WebhookURLs = "https://hooks.slack.com/services/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";
    opt.EmailAddresses = "something@gmail.com, nothing@outlook.com";
    opt.MailConfig = new WatchDog.Echo.src.Models.MailSettings
    {
        MailFrom = "test",
        MailHost = "test",
        MailPort = 455,
        MailPubKey = "test",   
        MailSecKey = "test", 
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}