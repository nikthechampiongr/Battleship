using Avalonia.Controls;
using Avalonia.Interactivity;
using Battleship.Model;

namespace Battleship;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void PlaceCarrier(object? sender, RoutedEventArgs e)
    {
        var ship = new Ship(ShipType.Carrier);

        PlayerGrid?.TryPlace(ship, Orientation.Horizontal);
    }

    private void Hit(object? sender, RoutedEventArgs e)
    {
        var selected = PlayerGrid?.Selected;

        if (selected == null)
            return;

        PlayerGrid?.HitOther(false, selected.Value);
    }

    private void SimulateEnemyHit(object? sender, RoutedEventArgs e)
    {
        PlayerGrid?.Hit(out _);
    }

    private void SimulateSuccessfulHit(object? sender, RoutedEventArgs e)
    {
        var selected = PlayerGrid?.Selected;

        if (selected == null)
            return;

        PlayerGrid?.HitOther(true, selected.Value);
    }
}