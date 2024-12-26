using Avalonia.Controls;
using Battleship.Model;

namespace Battleship.Controls;

// A control representing a battleship Cell. Contains extra info.
public class Cell : Button
{
    public Ship? Ship;
    public bool IsHit;
    public bool IsOpenentHit;
    public bool IsOpenenetHitSuccessful;


    public void Reset()
    {
        Ship = null;
        IsHit = false;
        IsOpenentHit = false;
        IsOpenenetHitSuccessful = false;
    }
}