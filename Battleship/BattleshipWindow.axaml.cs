using System;
using System.Diagnostics;
using Avalonia.Controls; using Avalonia.Interactivity;
using Avalonia.Threading;
using Battleship.Db;
using Battleship.Model;
using Battleship.Opponent;

namespace Battleship;

public partial class BattleshipWindow : Window
{
    private AIOpponent _opponent;

    public const int CarrierCapacity = 1;
    public const int DestroyerCapacity = 2;
    public const int BattleshipCapacity = 3;
    public const int SubmarineCapacity = 4;

    private int _remainingCarriers = CarrierCapacity;
    private int _remainingDestroyers = DestroyerCapacity;
    private int _remainingBattleships = BattleshipCapacity;
    private int _remainingSubmarines = SubmarineCapacity;

    private int _remainingShips = CarrierCapacity + DestroyerCapacity + BattleshipCapacity + SubmarineCapacity;

    private DispatcherTimer _gameTimer;
    private TimeSpan _gametime;
    private uint _turns;

    private Orientation _orientation = Orientation.Horizontal;

    public BattleshipWindow()
    {
        InitializeComponent();

        _opponent = new AIOpponent();

        _gameTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, SetTime);
        _gameTimer.IsEnabled = false;
    }

    private void SetTime(object? sender, EventArgs e)
    {
        if (GameTime == null)
            return;

        var time = DateTime.Now.TimeOfDay - _gametime;
        GameTime.Text = $"{time.Minutes:00}:{time.Seconds:00}";
    }

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

    private void BeginGame()
    {
        if (HitButton == null)
            return;

        _gametime = DateTime.Now.TimeOfDay;
        _gameTimer.IsEnabled = true;
        _turns = 0;

        HitButton.IsVisible = true;
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

        if (selected == null || HitButton == null)
            return;

        HitButton.IsEnabled = false;
        _turns += 1;

        var results = _opponent.GetHit(selected.Value);

        PlayerGrid?.HitOther(results.hit, selected.Value);

        if (results.surrender)
        {
            Debug.Assert(results.hitShip != null);

            if (results.hitShip == null)
                return;

            SetAnnouncement($"Νίκη! Βυθίστηκε το {results.hitShip} του αντίπαλου", true);
            Reset(Outcome.Victory);
            return;
        }

        if (results.hitShip != null)
            SetAnnouncement($"Βυθίστηκε το {results.hitShip.Value.GetString()} του αντίπαλου!", false);

        ExpectHit();
    }

    private void ExpectHit()
    {
        if (HitButton == null || PlayerGrid == null)
            return;

        var results = _opponent.HitPlayer();

        if (PlayerGrid!.Hit(results, out var damaged))
        {
            if (damaged.Health <= 0)
            {
                _remainingShips -= 1;
                if (_remainingShips <= 0)
                {
                    _opponent.GetHitResult(true, true, true);
                    SetAnnouncement($"Ήττα! Βυθίστηκε το {damaged.ShipType.GetString()} μου!", true);
                    Reset(Outcome.Defeat);
                    return;
                }

                SetOpponentAnnouncement($"Βυθίστηκε το {damaged.ShipType.GetString()} μου!");
                _opponent.GetHitResult(true, true, false);
            }
            else
            {
                _opponent.GetHitResult(true, false, false);
            }
        }
        else
        {
            _opponent.GetHitResult(false, false, false);
        }

        HitButton.IsEnabled = true;
    }


    private void Reset(Outcome outcome)
    {
        PlayerGrid?.Reset();
        _remainingCarriers = CarrierCapacity;
        _remainingDestroyers = DestroyerCapacity;
        _remainingBattleships = BattleshipCapacity;
        _remainingSubmarines = SubmarineCapacity;
        _orientation = Orientation.Horizontal;
        if (SetupPanel == null || CarrierButton == null || DestroyerButton == null || BattleshipButton == null || SubmarineButton == null || HitButton == null || GameTime == null)
            return;
        SetupPanel.IsVisible = true;
        CarrierButton.IsEnabled = true;
        DestroyerButton.IsEnabled = true;
        BattleshipButton.IsEnabled = true;
        SubmarineButton.IsEnabled = true;
        HitButton.IsVisible = false;
        HitButton.IsEnabled = true;
        GameTime.IsVisible = false;
        _remainingShips = CarrierCapacity + DestroyerCapacity + BattleshipCapacity + SubmarineCapacity;
        if (outcome == Outcome.Draw)
            return;

        DbManager.InsertGame(outcome == Outcome.Victory, _turns, DateTime.Now.TimeOfDay - _gametime);
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

    private void SetAnnouncement(string announcement, bool final)
    {
        if (AnnouncementText == null || OpponentAnnouncementText == null)
            return;

        if (final)
            OpponentAnnouncementText.IsVisible = false;

        AnnouncementText.IsVisible = true;
        AnnouncementText.Text = announcement;
    }

    private void SetOpponentAnnouncement(string announcement)
    {
        if (OpponentAnnouncementText == null)
            return;

        OpponentAnnouncementText.IsVisible = true;
        OpponentAnnouncementText.Text = announcement;
    }

    private enum Outcome
    {
        Victory,
        Draw,
        Defeat
    }
}