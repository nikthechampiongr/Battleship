using System;

namespace Battleship.Model;

public class Ship
{
    private ShipType _shipType;

    public int Width => (int) _shipType;

    public int Health;

    public Ship(ShipType shipType)
    {
        _shipType = shipType;

        Health = Width;
    }

    // Hit the ship! If health reaches 0 then the ship is considered destroyed and the function returns true.
    public bool Hit()
    {
        Health -= 1;

        if (Health <= 0)
            throw new InvalidOperationException("Hit a ship that is already dead");

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