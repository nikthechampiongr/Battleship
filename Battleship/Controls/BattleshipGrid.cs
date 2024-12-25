using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Battleship.Model;

namespace Battleship.Controls;

// Mostly a convenience class that allows me to easily create a grid with extra logic and less setup in axaml.
public class BattleshipGrid : Grid
{
    private const int Rows = 10;
    private const int Columns = 10;

    private readonly IBrush _defaultColor = Brushes.Gray;
    private readonly IBrush _selectedColor = Brushes.Yellow;

    private readonly IBrush _healthyShipColor = Brushes.Blue;

    private readonly IBrush _hitColor = Brushes.Red;
    private readonly IBrush _emptyHitColor = Brushes.Black;

    private Button? _selected;

    public BattleshipGrid()
    {
        for (int row = 0; row < Rows; row++)
        {
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int column = 0; column < Columns; column++)
            {
                var cell = new Cell()
                {
                    Width = 50,
                    Height = 50,
                    Background = _defaultColor,
                    Margin = new Thickness(2),
                    Name = $"{row * 10 + column}"
                };
                cell.Content = new Rectangle()
                {
                    Fill = _defaultColor,
                    Height = 20,
                    Width = 20,
                };

                cell.Click += (_, __) =>
                {
                    Select(cell);
                };
                SetRow(cell, row);
                SetColumn(cell, column);
                Children.Add(cell);
            }
        }

        // Might as well make this only show grid lines on debug
        #if DEBUG
        ShowGridLines = true;
        #endif
    }

    private void Select(Cell cell)
    {
        if (_selected != null)
        {
            _selected.Background = _defaultColor;
        }
        _selected = cell;
        cell.Background = _selectedColor;
    }

    private void Deselect()
    {
        if (_selected != null)
        {
            _selected.Background = _defaultColor;
            _selected = null;
        }
    }

    public bool TryPlace(Ship ship, Orientation orientation)
    {
        if (_selected == null)
            return false;

        // Null coercion since if that is null something went horribly wrong during initialisation
        var idx = int.Parse(_selected.Name!);

        var width = ship.Width;

        var x = idx % Columns;
        var y = idx / Rows;

        var toOccupy = new Cell[width];

        // Early return when we cannot possibly have enough space
        if ((orientation == Orientation.Horizontal && x + width > Columns) ||
            (orientation == Orientation.Vertical && y + width > Rows))
            return false;

        for (int i = 0; i < width; i++)
        {
            Control candidate;

            if (orientation == Orientation.Horizontal)
            {
                candidate = Children[idx + i];
            }
            else
            {
                candidate = Children[idx + i * Columns];
            }

            if (candidate is Cell cell && cell.Ship == null)
            {
                toOccupy[i] = cell;
            }
            else
            {
                return false;
            }
        }

        Place(toOccupy, ship);
        Deselect();
        return true;
    }

    // Hit the selected Cell. If the hit is successful the damaged ship is returned in the hitShip parameter.
    public bool Hit([NotNullWhen(returnValue: true)] out Ship? hitShip)
    {
        hitShip = null;

        if (_selected == null)
            return false;

        // Null coercion since if that is null something went horribly wrong during initialisation
        var idx = int.Parse(_selected.Name!);

        // It should not be possible to select a hit Cell.
        if (Children[idx] is not Cell cell || cell.IsHit || cell.Content is not Rectangle rect)
            throw new UnreachableException();

        cell.IsHit = true;
        Deselect();

        rect.Fill = _emptyHitColor;

        if (cell.Ship == null)
            return false;

        rect.Fill = _hitColor;

        hitShip = cell.Ship;

        cell.Ship.Hit();

        return true;
    }


    // Mark a cell as hit on the opponent's side
    public void HitOther(bool hitSuccessful, int idx)
    {
        if (Children[idx] is not Cell cell)
            throw new UnreachableException();

        if (hitSuccessful)
        {
            cell.Background = _hitColor;
        }
        else
        {
            cell.Background = _emptyHitColor;
        }
    }

    private void Place(Cell[] toPlace, Ship ship)
    {
        foreach (var cell in toPlace)
        {
            cell.Ship = ship;

            if (cell.Content is Rectangle rectangle)
            {
                rectangle.Fill = _healthyShipColor;
            }
        }
    }
}