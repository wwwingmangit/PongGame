using PongGameServer;
using PongGameServer.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSingleton(Log.Logger);

// Create GameServer
builder.Services.AddSingleton<GameServer>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    bool writeToConsole = false;
    return new GameServer(logger, writeToConsole);
});

// Register LLMCommentService with the logger
builder.Services.AddSingleton<LLMCommentService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    return new LLMCommentService(logger);
});

// This will start the server automatically
builder.Services.AddHostedService<GameServerHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline
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
