using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Battleship.Db;
using Battleship.Opponent;

namespace Battleship;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SetStats();
    }

    private async void SetStats()
    {
        if (WinText == null || LossText == null || TotalText == null || TurnsPlayedText == null || TimePlayedText == null)
            return;

        var stats = await DbManager.GetStats();

        WinText.Text = $"Νίκες: {stats.Wins.ToString()}";
        LossText.Text = $"Ήττες: {stats.Losses.ToString()}" ;
        TotalText.Text = $"Παιχνίδια: {stats.TotalGames.ToString()}";
        TurnsPlayedText.Text = $"Προσπάθειες: {stats.TurnsPlayed.ToString()}";
        TimePlayedText.Text =  $"Ώρες: {stats.TimePlayed}";
    }

    private async void Start(object? sender, RoutedEventArgs routedEventArgs)
    {

        var battleship = new BattleshipWindow()
        {
            Width = 850,
            Height = 700,
            Background = Brushes.Black,
        };

        await battleship.ShowDialog(this);
        SetStats();
    }
}