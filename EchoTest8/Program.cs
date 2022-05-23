using WatchDog.Echo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddWatchDogEchoServices(opt =>
{
    opt.EchoIntervalInMinutes = 0; opt.HostURLs = "https://localhost:7188"; opt.WebhookURLs = "https://hooks.slack.com/services/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";
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
