using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace Battleship.Db;

public static class DbManager
{
    private const string ConnString = "Data Source=battleship_db.sqlite";

    private static readonly SQLiteConnection Conn = new(ConnString);

    public static async Task InitDb()
    {
        // I'd rather have an actual migration system but this works.
        const string initDbQuery = """
                                   CREATE TABLE IF NOT EXISTS Games (
                                               GameId TEXT PRIMARY KEY,
                                               PlayerWon INTEGER NOT NULL,
                                               Turns INTEGER NOT NULL,
                                               Duration TEXT NOT NULL
                                               );
                                   """;

        await Conn.ExecuteAsync(initDbQuery);
    }

    public static async Task InsertGame(bool playerWon, uint turns, TimeSpan duration)
    {
        const string query = "INSERT INTO Games(GameId, PlayerWon, Turns, Duration) VALUES (@GameId, @PlayerWon, @Turns, @Duration)";

        var gameId = Guid.NewGuid();

        await Conn.ExecuteAsync(query, new {GameId = gameId, PlayerWon = playerWon, Turns = turns, Duration = duration});
    }

}