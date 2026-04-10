using WindowsGameLauncher.Models;
using WindowsGameLauncher.Services;

namespace WindowsGameLauncher;
internal class Program
{
    async static Task<int> Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        string profilePath = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gameProfile.json");
        string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        var logger = new LogService(logDirectoryPath);
        var profileServce = new ProfileService();

        logger.Info("Windows Game Launcher started.");
        try
        {
            var profile = profileServce.Load(profilePath);
            var launcher = new LauncherService(logger);
            var watchdog = new WatchdogService(logger, launcher, new ProcessMonitorService(logger));
            await watchdog.RunWithRecoveryAsync(profile);
            logger.Info("Launcher run completed.");
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            return 1;
        }
    }
}

