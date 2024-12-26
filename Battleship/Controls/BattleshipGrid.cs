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

    private Cell? _selected;

    public int? Selected {get {
        if (_selected?.Name == null)
            return null;

        return int.Parse(_selected.Name);
    }}

    public void Reset()
    {
        _selected = null;

        for (int idx = 0; idx < Rows * Columns; idx++)
        {
            if (Children[idx] is Cell cell && cell.Content is Rectangle rect)
            {
                cell.Background = _defaultColor;
                cell.Reset();
                rect.Fill = _defaultColor;
            }
        }
    }

    public BattleshipGrid()
    {
        for (int row = 0; row < Rows; row++)
        {
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int column = 0; column < Columns; column++)
            {
                var cell = new Cell
                {
                    Width = 50,
                    Height = 50,
                    Background = _defaultColor,
                    Margin = new Thickness(2),
                    Name = $"{row * 10 + column}",
                    Content = new Rectangle()
                    {
                        Fill = _defaultColor,
                        Height = 20,
                        Width = 20,
                    }
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
    }

    private void Select(Cell cell)
    {
        if (cell.IsOpenentHit)
            return;

        Deselect();

        _selected = cell;
        cell.Background = _selectedColor;
    }

    private void Deselect()
    {
        if (_selected != null)
        {
            // Hell expression: If the cell was not hit before we just want the default color bar. If it was hit we want to indicate whether the hit was successful or not
            _selected.Background = _selected.IsOpenentHit ? _selected.IsOpenenetHitSuccessful ? _hitColor : _emptyHitColor : _defaultColor;
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

        if (Children[idx] is not Cell cell || cell.IsHit || cell.Content is not Rectangle rect)
            return false;

        cell.IsHit = true;

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

        cell.IsOpenentHit = true;
        cell.IsOpenenetHitSuccessful = hitSuccessful;

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