using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using ManagedBass;

namespace HanumanInstitute.MediaPlayer.Avalonia.Sample;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BassDevice.Instance.Configure(Configuration.FloatDSP, true);
        BassDevice.Instance.Configure(Configuration.SRCQuality, 4);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
