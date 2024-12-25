using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Battleship;

// Mostly a convenience class that allows me to easily create a grid with extra logic and less setup in axaml.
public class BattleshipGrid : Grid
{
    private const int Rows = 10;
    private const int Columns = 10;

    private readonly IBrush _defaultColor = Brushes.Gray;
    private readonly IBrush _selectedColor = Brushes.Yellow;

    private Button? _selected;

    public BattleshipGrid()
    {
        for (int row = 0; row < Rows; row++)
        {
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int column = 0; column < Columns; column++)
            {
                var cell = new Button()
                {
                    Width = 50,
                    Height = 50,
                    Background = _defaultColor,
                    Margin = new Thickness(2)
                };

                cell.Click += (_, __) =>
                {
                    if (_selected != null)
                    {
                        _selected.Background = _defaultColor;
                    }
                    cell.Background = _selectedColor;
                    _selected = cell;
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

}