using PongGameServer;
using PongGameServer.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("log.txt")
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
