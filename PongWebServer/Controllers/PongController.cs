using Microsoft.AspNetCore.Mvc;
using PongGameServer;

[ApiController]
[Route("[controller]")]
public class PongController : ControllerBase
{
    private readonly GameServer _gameServer;

    public PongController(GameServer gameServer)
    {
        _gameServer = gameServer;
    }

    [HttpGet("games")]
    public IActionResult GetGames()
    {
        var games = _gameServer.GetGames();
        return Ok(games.Select(g => new { Id = g.GetHashCode(), Score = g.Score }));
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

    [HttpPost("stopall")]
    public IActionResult StopAllGames()
    {
        _gameServer.StopGames();
        return Ok("All games stopped");
    }

    [HttpPost("stop")]
    public IActionResult StopServer()
    {
        _gameServer.StopServer();
        return Ok("Server stopped");
    }
}