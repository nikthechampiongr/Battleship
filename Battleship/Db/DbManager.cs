using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace Battleship.Db;

public static class DbManager
{
    private const string ConnString = "Data Source=battleship_db.sqlite";

    private static readonly SQLiteConnection Conn = new(ConnString);

    private static bool _initialized;

    public static async Task InitDb()
    {
        // I'd rather have an actual migration system but this works.
        const string initDbQuery = """
                                   CREATE TABLE IF NOT EXISTS Games (
                                               GameId TEXT PRIMARY KEY,
                                               PlayerWon BOOLEAN NOT NULL,
                                               Turns INTEGER NOT NULL,
                                               Duration TEXT NOT NULL
                                               );
                                   """;

        await Conn.ExecuteAsync(initDbQuery);
        _initialized = true;
    }

    public static async Task InsertGame(bool playerWon, uint turns, TimeSpan duration)
    {
        if (!_initialized)
            throw new InvalidOperationException("DbManager not initialized");

        const string query = "INSERT INTO Games(GameId, PlayerWon, Turns, Duration) VALUES (@GameId, @PlayerWon, @Turns, @Duration)";

        var gameId = Guid.NewGuid();

        await Conn.ExecuteAsync(query, new {GameId = gameId, PlayerWon = playerWon, Turns = turns, Duration = duration});
    }

    public static async Task<Statistics> GetStats()
    {
        if (!_initialized)
            throw new InvalidOperationException("DbManager not initialized");

        const string query = "SELECT PlayerWon,Turns,Duration FROM Games";

        var stats = await Conn.QueryAsync<Game>(query);
        var wins = 0u;
        var losses = 0u;
        var turns = 0u;
        var timePlayed = TimeSpan.Zero;

        foreach (var game in stats)
        {
            if (game.PlayerWon)
            {
                wins += 1;
            }
            else
            {
                losses += 1;
            }

            turns += game.Turns;

            timePlayed += TimeSpan.Parse(game.Duration);
        }
        return new Statistics(wins, losses, timePlayed, turns);
    }

    private class Game
    {
        public bool PlayerWon;
        public uint Turns;
        //This is stupid but for some reason this can't be mapped properly from a string from Dapper's Query function.
        public string Duration;
    }
}

public class Statistics(uint wins, uint losses, TimeSpan timePlayed, uint turnsPlayed)
{
    public readonly uint Wins = wins;
    public readonly uint Losses = losses;
    public uint TotalGames => Losses + Wins;
    public readonly TimeSpan TimePlayed = timePlayed;
    public readonly uint TurnsPlayed = turnsPlayed;
}