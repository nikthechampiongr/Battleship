using System;
using Battleship.Model;

namespace Battleship.Opponent;

/// <summary>
/// Represents a message changing the state of the game. This is both sent to the opponent and received from them.
///
/// When player multiplayer, each message is 1 byte long and split as follows: 0 000 0000.
/// If the most significant bit is 0 then we are dealing with a "Meta message". The <see cref="MessageType"/> is
/// determined by the first 4 least significant bits. The 3 bits after that are used to determine if, and what ship was
/// sunk in the case of a Surrender or Hit message.
/// If the most significant bit is 1, then all the remaining bits are used to determine what cell is gonna be hit.
/// </summary>
public abstract record OpponentMessage;

/// <summary>
/// A message that is sent to indicate that a ship was hit in response to an attack.
/// </summary>
/// <param name="SunkShip">If the hit resulted in a ship sinking then this will be not null and null otherwise.</param>
public sealed record ShipHitMessage(ShipType? SunkShip) : OpponentMessage;

/// <summary>
/// A message that is sent to indicate that the opposing player has surrendered after losing all their ships.
/// This will always contain the final ship that was sunk.
/// </summary>
/// <param name="SunkMessage"></param>
public sealed record SurrenderMessage(ShipType SunkMessage) : OpponentMessage;

/// <summary>
/// A message that signals a hit either against one of our opponent's cells or a cell of our own.
/// When we are doing multiplayer over the network, a byte that's received will be a meta message if the most significant
/// bit is 1.
/// </summary>
/// <param name="Position">The cell idx being hit.</param>
public sealed record HitMessage(int Position) : OpponentMessage;

/// <summary>
/// A message that indicates that the previous attack missed.
/// </summary>
public sealed record HitMissedMessage() : OpponentMessage;

/// <summary>
/// A message to indicate the opposing player has setup their ships and is ready to play.
/// </summary>
public sealed record SetupCompleteMessage() : OpponentMessage;

[Flags]
public enum MessageType : byte
{
    // Ship placement is complete.
    SetupComplete = 1 << 0,
    // All ships sunk.
    Surrender = 1 << 1,
    // Hit was unsuccessful
    HitMiss = 1 << 2,
    // Hit was successful.
    HitSuccess = 1 << 3,
    // We are hiting a ship
    Hit = 1 << 7
}