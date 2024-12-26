namespace Battleship.Model;

public enum GameState
{
    Setup,
    AwaitingHitResult,
    AwaitingHit,
    Hitting,
    Finished
}