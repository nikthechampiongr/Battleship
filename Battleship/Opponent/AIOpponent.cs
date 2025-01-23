using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Battleship.Controls;
using Battleship.Model;

namespace Battleship.Opponent;

// ReSharper disable once InconsistentNaming
public sealed class AIOpponent
{
    private readonly Random _rng;
    private readonly BattleshipGrid _playspace = new();

    private GameState _gameState;

    // Normally the AI picks cells on the board at random. When this array is not empty however it will pick from these ones.
    private readonly List<int> _priorityCells = [];

    private int? _prevHit;

    private int _remainingShips;

    public AIOpponent()
    {

        #if DEBUG
        _rng = new Random(0);
        #else
        _rng = new Random();
        #endif

        Setup();
    }

    public void GetHitResult(bool success, bool shipSunk, bool surrender)
    {
        if (_gameState != GameState.AwaitingHitResult)
            throw new InvalidOperationException("Invalid game state reached: Got hit results while not waiting for hit results.");

        if (_prevHit == null)
            throw new UnreachableException("We are somehow expecting a hit result while not having fired before");

        _playspace.Selected = _prevHit;

        _gameState = GameState.AwaitingHit;

        // Game over. Reset.
        if (surrender)
        {
            Setup();
            return;
        }

        if (success)
        {
            Debug.Assert(_playspace.HitOther(true, _prevHit.Value));

            // We hit something. Focus on the adjacent cells and hope we kill a ship
            if (!shipSunk)
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

            return;
        }

        _playspace.HitOther(false, _prevHit.Value);
    }

    public (bool hit, ShipType? hitShip, bool surrender) GetHit(int position)
    {
        if (_gameState != GameState.AwaitingHit)
            throw new InvalidOperationException("Invalid game state reached: Got hit while not waiting for hit.");

        var hit = false;
        var surrender = false;
        ShipType? ship = null;
        _gameState = GameState.AwaitingToHitPlayer;

        if (!_playspace.Hit(position, out var hitShip)) return (hit, ship, surrender);

        hit = true;

        if (hitShip.Health > 0) return (hit, ship, surrender);

        _remainingShips -= 1;
        ship = hitShip.ShipType;

        if (_remainingShips > 0) return (hit, ship, surrender);

        surrender = true;
        Setup();
        return (hit, ship, surrender);
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

    public int HitPlayer()
    {
        if (_gameState != GameState.AwaitingToHitPlayer)
            throw new InvalidOperationException("Invalid game state reached: Got to hit player while not waiting to hit player.");

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
        _prevHit = idx;
        _gameState = GameState.AwaitingHitResult;
        return _prevHit.Value;
    }

    private void Setup()
    {
        _playspace.Reset();

        _remainingShips = BattleshipWindow.CarrierCapacity + BattleshipWindow.DestroyerCapacity + BattleshipWindow.DestroyerCapacity + BattleshipWindow.SubmarineCapacity;

        for (int i = 0; i < BattleshipWindow.CarrierCapacity; i++)
        {
            SpawnRandom(ShipType.Carrier);
        }
        
        for (int i = 0; i < BattleshipWindow.DestroyerCapacity; i++)
        {
            SpawnRandom(ShipType.Destroyer);
        }
        
        for (int i = 0; i < BattleshipWindow.BattleshipCapacity; i++)
        {
            SpawnRandom(ShipType.Battleship);
        }

        for (int i = 0; i < BattleshipWindow.SubmarineCapacity; i++)
        {
            SpawnRandom(ShipType.Submarine);
        }
        
        _gameState = GameState.AwaitingHit;
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
}
