using Avalonia.Controls;
using Avalonia.Interactivity;
using Battleship.Controls;
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
}