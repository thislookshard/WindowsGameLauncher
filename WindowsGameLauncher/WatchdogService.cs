using System.Diagnostics;
using System.IO.Pipelines;
using WindowsGameLauncher.Models;

namespace WindowsGameLauncher.Services;

public class WatchdogService
{
    private readonly LogService _log;
    private readonly LauncherService _launcher;
    private readonly ProcessMonitorService _monitor;

    public WatchdogService(
        LogService log,
        LauncherService launcher,
        ProcessMonitorService monitor)
    {
        _log = log;
        _launcher = launcher;
        _monitor = monitor;
    }

    public async Task RunWithRecoveryAsync(
        GameLaunchProfile profile,
        CancellationToken cancellationToken = default)
    {
        int attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            _log.Info(
                $"Launching '{profile.GameName}' (attempt {attempt}/{Math.Max(1, profile.MaxRestarts + 1)})..."
            );

            Process process;
            LaunchSession session;

            try
            {
                process = _launcher.Launch(profile, attempt, out session);
            }
            catch (Exception ex)
            {
                _log.Error($"Launch attempt failed: {ex.Message}");
                if (attempt > profile.MaxRestarts)
                {
                    _log.Error($"Max restart attempts reached for '{profile.GameName}'. Aborting.");
                    return;
                }

                _log.Warning(
                    $"Will retry launching '{profile.GameName}' in {profile.RestartDelaySeconds}s. " +
                    $"Attempt {attempt} of {profile.MaxRestarts}."
                );

                await Task.Delay(TimeSpan.FromSeconds(profile.RestartDelaySeconds), cancellationToken);
                continue;
            }


            LaunchResult result = await _monitor.MonitorAsync(process, profile, session, cancellationToken);

            bool shouldRestart =
                profile.ShouldRestartOnCrash &&
                attempt <= profile.MaxRestarts &&
                (!result.ExitedNormally || result.TimedOutAsHung);

            if (!shouldRestart)
            {
                _log.Info($"Run complete for '{profile.GameName}'. No further restart attempts.");
                break;
            }

            _log.Warning(
                $"Restarting '{profile.GameName}' in {profile.RestartDelaySeconds}s. " +
                $"Attempt {attempt} of {profile.MaxRestarts}.");

            await Task.Delay(TimeSpan.FromSeconds(profile.RestartDelaySeconds), cancellationToken);
        }
    }

    private void LogResult(GameLaunchProfile profile, LaunchResult result)
    {
        string sessionId = result.Session != null ? $"SessionID={result.Session.SessionId}" : "No session";
        string exitCodeText = result.ExitCode.HasValue ? $"ExitCode={result.ExitCode.Value}" : "ExitCode=N/A";  
        if (result.TimedOutAsHung)
        {
            _log.Warning($"'{profile.GameName}' timed out and was treated as hung.\n" +
                         $"{sessionId}, {exitCodeText}");
        }
        else if (!result.ExitedNormally)
        {
            _log.Warning($"'{profile.GameName}' exited abnormally with code {exitCodeText}.");
        }
        else
        {
            _log.Info($"'{profile.GameName}' exited normally with code {exitCodeText}.");
        }
    }
}