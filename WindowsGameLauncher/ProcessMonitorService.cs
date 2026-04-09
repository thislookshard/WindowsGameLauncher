using System.Diagnostics;
using WindowsGameLauncher.Models;

namespace WindowsGameLauncher.Services;

public class ProcessMonitorService
{
    private readonly LogService _log;

    public ProcessMonitorService(LogService log)
    {
        _log = log;
    }

    public async Task<LaunchResult> MonitorAsync(
        Process process,
        GameLaunchProfile profile,
        LaunchSession session,
        CancellationToken cancellationToken = default)
    {
        var result = new LaunchResult
        {
            StartedSuccessfully = true,
            Session = session
        };

        if (profile.MaxHangTimeSeconds > 0)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(profile.MaxHangTimeSeconds));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                result.TimedOutAsHung = true;
                result.Message = "Process exceeded configured hang timeout.";
                TryKill(process);
                FinalizeSession(session);
                _log.Warning($"PID={process.Id} timed out and was terminated.");
                return result;
            }
        }
        else
        {
            await process.WaitForExitAsync(cancellationToken);
        }

        result.ExitCode = process.ExitCode;
        result.ExitedNormally = process.ExitCode == 0;
        result.Message = result.ExitedNormally
            ? "Process exited normally."
            : $"Process exited with code {process.ExitCode}.";

        FinalizeSession(session);

        _log.Info($"PID={process.Id} exited with code {process.ExitCode}");

        return result;
    }

    public ProcessSnapshot CaptureSnapshot(Process process)
    {
        return new ProcessSnapshot
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            ProcessId = process.Id,
            ProcessName = process.ProcessName,
            HasExited = process.HasExited,
            WorkingSetBytes = process.HasExited ? 0 : process.WorkingSet64,
            TotalProcessorTime = process.HasExited ? TimeSpan.Zero : process.TotalProcessorTime
        };
    }

    private void FinalizeSession(LaunchSession session)
    {
        session.EndTimeUtc = DateTimeOffset.UtcNow;
    }

    private void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to kill process PID={process.Id}: {ex.Message}");
        }
    }
}