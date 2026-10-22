using GoatTweaks.Core.Catalog;
using GoatTweaks.Core.Assets;
using GoatTweaks.Core.Execution;
using GoatTweaks.Runner;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace GoatTweaks;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            if (e.Args.Length > 0 && e.Args[0] is "--runner" or "--action")
            {
                Environment.ExitCode = await RunnerEntry.RunAsync(e.Args);
                Shutdown();
                return;
            }

            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19045) || !Environment.Is64BitProcess)
                throw new PlatformNotSupportedException("Requer Windows 10 22H2 ou Windows 11 de 64 bits.");
            var version = typeof(App).Assembly.GetName().Version?.ToString() ?? "1.0.0";
            var assets = EmbeddedAssetStore.MaterializeThirdParty(Path.Combine(GoatPaths.LocalRoot, "assets", version));
            Environment.SetEnvironmentVariable("GOATTWEAKS_ASSET_ROOT", assets);
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception exception)
        {
            Environment.ExitCode = 1;
            MessageBox.Show($"Não foi possível abrir o GOAT Tweaks.\n\n{exception.Message}",
                "GOAT Tweaks", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
