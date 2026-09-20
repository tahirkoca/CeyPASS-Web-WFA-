using System.IO;
using System.Windows;
using AutoUpdaterDotNET;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.WPF;

public partial class App : System.Windows.Application
{
    private const string AppDisplayName = "CeyPASS PDKS";

    public static IServiceProvider Services { get; private set; } = null!;

    private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        TryStartAutoUpdater();
        CeypassDxLocalization.Apply();
        CeypassTheme.ApplySaved();

        try
        {
            Services = ServiceRegistration.Build();
            var login = Services.GetRequiredService<Views.LoginWindow>();
            login.Show();
        }
        catch (Exception ex)
        {
            UiDialog.Error(
                $"Uygulama başlatılamadı:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "CeyPASS");
            Shutdown(-1);
        }
    }

    private static void TryStartAutoUpdater()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("CEYPASS_SKIP_AUTOUPDATE"), "1", StringComparison.OrdinalIgnoreCase))
            return;

        // WFA Program.cs ile aynı kanal / ayarlar
        try
        {
            AutoUpdater.AppTitle = AppDisplayName;
            AutoUpdater.InstallationPath = AppContext.BaseDirectory;
            AutoUpdater.Mandatory = false;
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.ReportErrors = false;
            AutoUpdater.Synchronous = false;
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.ApplicationExitEvent += () =>
            {
                try { Current.Shutdown(); } catch { /* ignore */ }
            };
            AutoUpdater.Start(@"http://192.168.0.23/CeyPASS-Updates/update.xml");
        }
        catch
        {
            // Güncelleme kontrolü başarısız olsa bile program açılsın
        }
    }

    private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "startup.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] {e.Exception}\r\n");
        }
        catch
        {
            // ignore
        }

        e.Handled = true;
        UiDialog.Error(e.Exception.Message, "CeyPASS");
    }
}