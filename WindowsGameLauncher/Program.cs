using WindowsGameLauncher.Models;
using WindowsGameLauncher.Services;

namespace WindowsGameLauncher;
internal class Program
{
    async static Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        string profilePath = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gameProfile.json");
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.log");

        var logger = new LogService(logPath);
        var profileServce = new ProfileService();

        logger.Info("Windows Game Launcher started.");
        try
        {
            var profile = profileServce.Load(profilePath);
            var launcher = new LauncherService(logger);
            var watchdog = new WatchdogService(logger, launcher, new ProcessMonitorService(logger));
           // await watchdog.RunWithRecoveryAsync(profile);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
        }
    }
}

