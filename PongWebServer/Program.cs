using PongGameServer;
using Serilog;
using PongGameServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("log.txt")
    .CreateLogger();

builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

// Creat GameServer
builder.Services.AddSingleton<GameServer>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    bool writeToConsole = false;
    return new GameServer(logger, writeToConsole);
});

// This will start the server automatically
builder.Services.AddHostedService<GameServerHostedService>();

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

app.UseStaticFiles();

app.Run();
