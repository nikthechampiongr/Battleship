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

    private async void ConnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (AddressField?.Text is null or "")
            return;

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        var endpoint = new DnsEndPoint(AddressField.Text, 3775);

        socket.Connect(endpoint);

        var opponent = new NetOpponent(socket);

        await Start(opponent, false);
        socket.Shutdown(SocketShutdown.Both);
    }

    private async void HostButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3775));

        socket.Listen();

        var conn = socket.Accept();

        var opponent = new NetOpponent(conn);

        await Start(opponent, true);
        conn.Shutdown(SocketShutdown.Both);
    }

    private async Task Start(IOpponent opponent, bool startFirst)
    {

        var battleship = new BattleshipWindow(opponent, startFirst)
        {
            Width = 850,
            Height = 700,
            Background = Brushes.Black,
        };

        await battleship.ShowDialog(this);
        SetStats();
    }

    private void OfflineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var opponent = new AIOpponent();

        Start(opponent, true);
    }
}