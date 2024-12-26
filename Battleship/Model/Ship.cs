using System;
using System.Diagnostics;

namespace Battleship.Model;

public sealed class Ship
{
    public readonly ShipType ShipType;

    public int Width => (int) ShipType;

    public int Health;

    public Ship(ShipType shipType)
    {
        ShipType = shipType;

        Health = Width;
    }

    // Hit the ship! If health reaches 0 then the ship is considered destroyed and the function returns true.
    public bool Hit()
    {
        Debug.Assert(Health > 0);

        Health -= 1;

        return Health == 0;
    }
}

public enum ShipType : byte
{
    Carrier = 5,
    Destroyer = 4,
    Battleship = 3,
    Submarine = 2
}