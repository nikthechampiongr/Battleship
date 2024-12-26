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

    private const int CarrierCapacity = 1;
    private const int DestroyerCapacity = 2;
    private const int BattleshipCapacity = 3;
    private const int SubmarineCapacity = 4;

    private int _remainingCarriers = CarrierCapacity;
    private int _remainingDestroyers = DestroyerCapacity;
    private int _remainingBattleships = BattleshipCapacity;
    private int _remainingSubmarines = SubmarineCapacity;

    private Orientation _orientation = Orientation.Horizontal;

    private void PlaceCarrier(object? sender, RoutedEventArgs e)
    {
        if (PlaceShip(ShipType.Carrier))
            _remainingCarriers -= 1;

        if (_remainingCarriers <= 0 && CarrierButton != null)
        {
            CarrierButton.IsEnabled = false;
            GoToNextStageIfPlacementComplete();
        }
    }

    private void PlaceDestroyer(object? sender, RoutedEventArgs e)
    {
        if(PlaceShip(ShipType.Destroyer) )
            _remainingDestroyers -= 1;

        if (_remainingDestroyers <= 0 && DestroyerButton != null)
        {
            DestroyerButton.IsEnabled = false;
            GoToNextStageIfPlacementComplete();
        }
    }

    private void PlaceBattleShip(object? sender, RoutedEventArgs e)
    {
        if (PlaceShip(ShipType.Battleship))
            _remainingBattleships -= 1;

        if (_remainingBattleships <= 0 && BattleshipButton != null)
        {
            BattleshipButton.IsEnabled = false;
            GoToNextStageIfPlacementComplete();
        }
    }

    private void PlaceSubmarine(object? sender, RoutedEventArgs e)
    {
        if(PlaceShip(ShipType.Submarine))
            _remainingSubmarines -= 1;

        if (_remainingSubmarines <= 0 && SubmarineButton != null)
        {
            GoToNextStageIfPlacementComplete();
            SubmarineButton.IsEnabled = false;
        }
    }

    private void GoToNextStageIfPlacementComplete()
    {
        if (_remainingCarriers <= 0 && _remainingDestroyers <= 0 && _remainingBattleships <= 0 &&
            _remainingSubmarines <= 0 && SetupPanel != null)
            SetupPanel.IsVisible = false;
    }

    private bool PlaceShip(ShipType ty)
    {
        if (PlayerGrid == null)
            return false;

        var ship = new Ship(ty);

        return PlayerGrid.TryPlace(ship, _orientation);
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

    private void Reset(object? sender, RoutedEventArgs routedEventArgs)
    {
        PlayerGrid?.Reset();
        _remainingCarriers = CarrierCapacity;
        _remainingDestroyers = DestroyerCapacity;
        _remainingBattleships = BattleshipCapacity;
        _remainingSubmarines = SubmarineCapacity;
        _orientation = Orientation.Horizontal;
        if (SetupPanel == null || CarrierButton == null || DestroyerButton == null || BattleshipButton == null || SubmarineButton == null)
            return;
        SetupPanel.IsVisible = true;
        CarrierButton.IsEnabled = true;
        DestroyerButton.IsEnabled = true;
        BattleshipButton.IsEnabled = true;
        SubmarineButton.IsEnabled = true;
    }

    private void ChangeOrientation(object? sender, RoutedEventArgs e)
    {
        if (OrientationButton == null)
            return;

        if (_orientation == Orientation.Horizontal)
        {
            _orientation = Orientation.Vertical;
            OrientationButton.Content = "Προσανατολισμός: κατακόρυφος";
        }
        else
        {
            _orientation = Orientation.Horizontal;
            OrientationButton.Content = "Προσανατολισμός: οριζόντιος";
        }
    }
}