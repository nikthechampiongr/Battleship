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
/// A meta message is any message which is not an attempted hit against a cell.
/// When we are doing multiplayer over the network, a byte that's received will be me a meta message if the most significant
/// bit is 0.
/// </summary>
/// <param name="MessageType">The type of meta message received.</param>
public sealed record MetaMessage(MessageType MessageType) : OpponentMessage;

public sealed record ShipHitMessage(ShipType? SunkShip) : OpponentMessage;

public sealed record SurrenderMessage(ShipType SunkMessage) : OpponentMessage;

/// <summary>
/// A message that signals a hit either against one of our opponent's cells or a cell of our own.
/// When we are doing multiplayer over the network, a byte that's received will be a meta message if the most significant
/// bit is 1.
/// </summary>
/// <param name="Position">The cell idx being hit.</param>
public sealed record HitMessage(int Position) : OpponentMessage;

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