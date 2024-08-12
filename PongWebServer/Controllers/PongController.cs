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

}