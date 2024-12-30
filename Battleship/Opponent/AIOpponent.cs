using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Battleship.Controls;
using Battleship.Model;

namespace Battleship.Opponent;

// ReSharper disable once InconsistentNaming
public sealed class AIOpponent : IOpponent
{
    private readonly Random _rng;
    private readonly BattleshipGrid _playspace = new();

    private GameState _gameState = GameState.Setup;

    // Normally the AI picks cells on the board at random. When this array is not empty however it will pick from these ones.
    private readonly List<int> _priorityCells = [];

    private readonly List<OpponentMessage> _messages = [];

    private int? _prevHit;

    private int _remainingShips;

    public AIOpponent()
    {

        #if DEBUG
        _rng = new Random(0);
        #else
        _rng = new Random();
        #endif
    }

    public Task SendMessage(OpponentMessage msg)
    {
        switch (_gameState)
        {
            case GameState.Setup:
                if (msg is not SetupCompleteMessage)
                    throw new InvalidOperationException("A message other than SetupComplete was received when the game is not running");
                Setup();
                break;
            case GameState.AwaitingHitResult:
                if (_prevHit == null)
                    throw new UnreachableException("We are somehow expected hit results while not having fired before");

                _playspace.Selected = _prevHit;
                switch (msg)
                {
                    // Game over. Reset.
                    case SurrenderMessage:
                        _gameState = GameState.Setup;
                        break;
                    case ShipHitMessage hit:
                        _playspace.HitOther(true, _prevHit.Value);
                        // We hit something. Focus on the adjacent cells and hope we kill a ship
                        if (hit.SunkShip == null)
                        {
                            AssignPriorityCells(_prevHit.Value);
                        }
                        else
                        {
                            // We sunk a ship. Reset the priority cells. Now this is a naive algorithm since theoretically
                            // we could infer that other ships are in the area with other data, but I do not feel like coding
                            // something like that right now.
                            _priorityCells.Clear();
                        }

                        _gameState = GameState.AwaitingHit;
                        break;
                    case HitMissedMessage:
                        _playspace.HitOther(false, _prevHit.Value);
                        _gameState = GameState.AwaitingHit;
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected message while awaiting hit result: {msg.GetType()}.");
                }
                break;
            case GameState.AwaitingHit:

                if (msg is not HitMessage hitAttempt)
                {
                    throw new InvalidOperationException($"Unexpected message while awaiting hit: {msg.GetType()}. We are expected a hit from the player.");
                }

                if (!_playspace.Hit(hitAttempt.Position, out var hitShip))
                {
                    _messages.Add(new HitMissedMessage());
                } else if (hitShip.Health <= 0)
                {
                    _remainingShips -= 1;
                    // We lost. Reset
                    if (_remainingShips <= 0)
                    {
                        _messages.Add(new SurrenderMessage(hitShip.ShipType));
                        _gameState = GameState.Setup;
                        break;
                    }
                    _messages.Add(new ShipHitMessage(hitShip.ShipType));
                }
                else
                {
                    _messages.Add(new ShipHitMessage(null));
                }

                Hit();
                break;
            default:
                throw new InvalidOperationException($"Received a message while in {_gameState} state. We should not receive a message at this time.");
        }

        return Task.CompletedTask;
    }

    private void AssignPriorityCells(int idx)
    {
        if (_prevHit == null)
        {
            throw new UnreachableException("We are somehow trying to assign priority cells without having previous hits to reference.");
        }
        // We do not care if we pick invalid cells. They will just get filtered out when we try to hit.
        // We are adding all adjacent cells to the list.
        _priorityCells.Add(idx + 1);
        _priorityCells.Add(idx - 1);
        _priorityCells.Add(idx + BattleshipGrid.Columns);
        _priorityCells.Add(idx - BattleshipGrid.Columns);
    }

    private void Hit()
    {
        int idx;
        while (true)
        {
            if (_priorityCells.Count != 0)
            {
                var selected = _rng.Next(0, _priorityCells.Count);
                idx = _priorityCells[selected];
                _priorityCells.RemoveAt(selected);
            }
            else
            {
                idx = _rng.Next(0, BattleshipGrid.TotalCells);
            }

            if (_playspace.CanHitOther(idx))
                break;
        }
        _messages.Add(new HitMessage(idx));
        _prevHit = idx;
        _gameState = GameState.AwaitingHitResult;
    }

    private void Setup()
    {
        _playspace.Reset();

        _remainingShips = MainWindow.CarrierCapacity + MainWindow.DestroyerCapacity + MainWindow.DestroyerCapacity + MainWindow.SubmarineCapacity;

        for (int i = 0; i < MainWindow.CarrierCapacity; i++)
        {
            SpawnRandom(ShipType.Carrier);
        }
        
        for (int i = 0; i < MainWindow.DestroyerCapacity; i++)
        {
            SpawnRandom(ShipType.Destroyer);
        }
        
        for (int i = 0; i < MainWindow.BattleshipCapacity; i++)
        {
            SpawnRandom(ShipType.Battleship);
        }

        for (int i = 0; i < MainWindow.SubmarineCapacity; i++)
        {
            SpawnRandom(ShipType.Submarine);
        }
        
        _gameState = GameState.AwaitingHit;
        _messages.Add(new SetupCompleteMessage());
    }

    private void SpawnRandom(ShipType shipType)
    {
        var ship = new Ship(shipType);
        while (true)
        {
            var orientation  = _rng.Next(0,1) == 0 ? Orientation.Horizontal : Orientation.Vertical;
            var spot = _rng.Next(0, BattleshipGrid.TotalCells);
            _playspace.Selected = spot;
            if (_playspace.TryPlace(ship, orientation))
                return;
        }
    }

    public Task<OpponentMessage> GetMessage()
    {
        if (_messages.Count == 0)
            throw new InvalidOperationException("Message was received when it was not expected.");

        var next = _messages[0];
        _messages.RemoveAt(0);
        return Task.FromResult(next);
    }
}
