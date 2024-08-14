using PongGameServer;
using Serilog;
using PongLLM; // Ensure this namespace is included for LLMCommentService

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

// Create GameServer
builder.Services.AddSingleton<GameServer>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    bool writeToConsole = false;
    return new GameServer(logger, writeToConsole);
});

// Register LLMCommentService with the logger
builder.Services.AddSingleton<PongGameServer.Services.LLMCommentService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    return new PongGameServer.Services.LLMCommentService(logger);
});

// This will start the server automatically
builder.Services.AddHostedService<PongGameServer.Services.GameServerHostedService>();

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
