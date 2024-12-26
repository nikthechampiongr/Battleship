using System.Threading.Tasks;

namespace Battleship.Opponent;

/// <summary>
/// This represents an opponent who can fight against you in game.
/// It is an interface so we are able to support both fighting against AI and against other humans over the internet.
/// For this purpose, we abstract what an opponent can do behind messages.
/// </summary>
public interface IOpponent
{
    Task SendMessage(OpponentMessage msg);

    Task<OpponentMessage> GetMessage();
}