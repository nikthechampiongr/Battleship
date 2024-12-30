using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Battleship.Model;
using Battleship.Opponent;

namespace Battleship;

public partial class MainWindow : Window
{
    private IOpponent _opponent;

    public MainWindow()
    {
        InitializeComponent();
        _opponent = new AIOpponent();
    }

    public const int CarrierCapacity = 1;
    public const int DestroyerCapacity = 2;
    public const int BattleshipCapacity = 3;
    public const int SubmarineCapacity = 4;

    private int _remainingCarriers = CarrierCapacity;
    private int _remainingDestroyers = DestroyerCapacity;
    private int _remainingBattleships = BattleshipCapacity;
    private int _remainingSubmarines = SubmarineCapacity;

    private int _remainingShips = CarrierCapacity + DestroyerCapacity + BattleshipCapacity + SubmarineCapacity;

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

        if (_remainingSubmarines > 0 || SubmarineButton == null) return;
        GoToNextStageIfPlacementComplete();
        SubmarineButton.IsEnabled = false;
    }

    private void GoToNextStageIfPlacementComplete()
    {
        if (_remainingCarriers > 0 || _remainingDestroyers > 0 || _remainingBattleships > 0 ||
            _remainingSubmarines > 0 || SetupPanel == null)
            return;

        SetupPanel.IsVisible = false;

        BeginGame();
    }

    private async void BeginGame()
    {
        if (HitButton == null)
            return;

        await Send(new SetupCompleteMessage());
        var response = await Receive();

        // TODO: Reset on invalid response
        Debug.Assert(response is { success: true, msg: SetupCompleteMessage });

        HitButton.IsVisible = true;
    }

    private bool PlaceShip(ShipType ty)
    {
        if (PlayerGrid == null)
            return false;

        var ship = new Ship(ty);

        return PlayerGrid.TryPlace(ship, _orientation);
    }

    private async void Hit(object? sender, RoutedEventArgs e)
    {
        var selected = PlayerGrid?.Selected;

        if (selected == null || HitButton == null)
            return;

        HitButton.IsEnabled = false;

        await Send(new HitMessage(selected.Value));

        var response = await Receive();


        // TODO: Reset on invalid response
        Debug.Assert(response.success);

        switch (response.msg)
        {
            case ShipHitMessage hitMessage:
                // TODO: Announce ship sinking
                PlayerGrid?.HitOther(true, selected.Value);
                break;
            case SurrenderMessage surrender:
                // TODO: Announce ship sinking and victory
                // TODO: Make it not just reset the game but show stats
                Reset();
                return;
            case HitMissedMessage _:
                PlayerGrid?.HitOther(false, selected.Value);
                break;
            default:
                Debug.Fail($"Got unexpected message while awaiting a hit: {response.msg?.GetType()}");
                break;
        }

        response = await Receive();

        Debug.Assert(response is { success: true, msg: HitMessage });

        // TODO: Reset on invalid response
        if (response.msg is not HitMessage hit)
            return;

        // Null coercion. It should not be possible for this to be null since we have a selected cell.
        if (PlayerGrid!.Hit(hit.Position, out var damaged))

        {
            if (damaged.Health <= 0)
            {
                _remainingShips -= 1;
                if (_remainingShips <= 0)
                {
                    // TODO: Make it not just reset the game but show stats
                    await Send(new SurrenderMessage(damaged.ShipType));
                    Reset();
                    return;
                }

                await Send(new ShipHitMessage(damaged.ShipType));
            }
            else
            {
                await Send(new ShipHitMessage(null));
            }
        }
        else
        {
            await Send(new HitMissedMessage());
        }

        HitButton.IsEnabled = true;
    }

    // This right now seems useless, but we are preparing to deal with exceptions regarding the network when we implement multiplayer.
    private async Task<bool> Send(OpponentMessage msg)
    {
        await _opponent.SendMessage(msg);

        return true;
    }

    // This right now seems useless, but we are preparing to deal with exceptions regarding the network when we implement multiplayer.
    private async Task<(bool success, OpponentMessage? msg)> Receive()
    {
        var res = await _opponent.GetMessage();
        return (true, res);
    }

    private void Reset()
    {
        PlayerGrid?.Reset();
        _remainingCarriers = CarrierCapacity;
        _remainingDestroyers = DestroyerCapacity;
        _remainingBattleships = BattleshipCapacity;
        _remainingSubmarines = SubmarineCapacity;
        _orientation = Orientation.Horizontal;
        if (SetupPanel == null || CarrierButton == null || DestroyerButton == null || BattleshipButton == null || SubmarineButton == null || HitButton == null)
            return;
        SetupPanel.IsVisible = true;
        CarrierButton.IsEnabled = true;
        DestroyerButton.IsEnabled = true;
        BattleshipButton.IsEnabled = true;
        SubmarineButton.IsEnabled = true;
        HitButton.IsVisible = false;
        HitButton.IsEnabled = true;
        _remainingShips = CarrierCapacity + DestroyerCapacity + BattleshipCapacity + SubmarineCapacity;
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