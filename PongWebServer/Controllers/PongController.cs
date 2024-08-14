﻿using Microsoft.AspNetCore.Mvc;
using PongGameServer;
using PongGameServer.Services;

[ApiController]
[Route("[controller]")]
public class PongController : ControllerBase
{
    private readonly GameServer _gameServer;
    private readonly LLMCommentService _llmCommentService;

    public PongController(GameServer gameServer, LLMCommentService llmCommentService)
    {
        _gameServer = gameServer;
        _llmCommentService = llmCommentService;
    }

    [HttpPost("start")]
    public IActionResult StartServer()
    {
        _gameServer.StartServer();
        return Ok("Server started");
    }

    [HttpPost("stop")]
    public IActionResult StopServer()
    {
        _gameServer.StopServer();
        return Ok("Server stopped");
    }

    [HttpGet("games")]
    public IActionResult GetGames()
    {
        var response = GetGameStats();
        return Ok(response);
    }

    [HttpPost("addgame")]
    public IActionResult AddGame()
    {
        int gameUpdateDelayInMSec = 1;
        int winningScore = 11;
        _gameServer.AddNewGame(gameUpdateDelayInMSec, winningScore);
        return Ok("New game added");
    }

    [HttpDelete("games/{id}")]
    public IActionResult RemoveGame(int id)
    {
        _gameServer.StopGame(id);
        return Ok($"Game {id} removed");
    }

    [HttpDelete("stopall")]
    public IActionResult StopAllGames()
    {
        _gameServer.StopGames();
        return Ok("All games stopped");
    }

    // Refactored method to get the game stats
    private object GetGameStats()
    {
        var games = _gameServer.GetGames();
        var serverUpTime = _gameServer.UpTime;

        var response = new
        {
            serverUpTime = serverUpTime.ToString(@"dd\.hh\:mm\:ss"),
            games = games.Select(g => new
            {
                id = g.GetHashCode(),
                score = g.Score,
                duration = g.Duration.ToString(@"hh\:mm\:ss")
            })
        };

        return response;
    }

    // New endpoint to get the latest LLM comment
    [HttpGet("llmcomment")]
    public async Task<IActionResult> GetLLMComment()
    {
        var gameStats = GetGameStats();
        var comment = await _llmCommentService.GenerateCommentAsync(gameStats);
        return Ok(new { comment });
    }
}