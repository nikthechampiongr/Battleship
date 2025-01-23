using System;
using System.Net;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Battleship.Db;
using Battleship.Opponent;

namespace Battleship;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        await DbManager.InitDb();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                Width = 450,
                Height = 400
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}