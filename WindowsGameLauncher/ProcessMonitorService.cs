using System.Diagnostics;
using System.Runtime.InteropServices;
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

        var snapshotTask = MonitorSnapshotsAsync(process, session, cancellationToken);

        try
        {
            await MonitorProcessAsync(process, profile, result, cancellationToken);

            result.ExitCode = process.ExitCode;
            result.ExitedNormally = process.ExitCode == 0;
            result.Message = result.ExitedNormally
                ? "Process exited normally."
                : $"Process exited with code {process.ExitCode}.";

            FinalizeSession(session);

            _log.Info($"PID={process.Id} exited with code {process.ExitCode}");
            return result;
        }
        finally
        {
            try
            {
                await snapshotTask;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }
    }

    private async Task MonitorProcessAsync(Process process, GameLaunchProfile profile, LaunchResult result, CancellationToken cancellationToken)
    {
        var startIdleTime = DateTimeOffset.UtcNow;

        while (!cancellationToken.IsCancellationRequested && !process.HasExited)
        {
            if (process.HasExited)
            {
                result.ExitedNormally = process.ExitCode == 0;
                result.ExitCode = process.ExitCode;
                break;
            }
            
            bool isHung = process.Responding == false;
            if (!isHung)
            {
                startIdleTime = DateTimeOffset.UtcNow;
            }
            else
            {
                var idleDuration = DateTimeOffset.UtcNow - startIdleTime;
                if (idleDuration.TotalSeconds >= 30)
                {
                    _log.Warning($"PID={process.Id} has been hung for {idleDuration.TotalSeconds}s.");
                }

                if (idleDuration.TotalSeconds > profile.MaxHangTimeSeconds)
                {
                    _log.Error($"PID={process.Id} has been hung for {idleDuration.TotalSeconds}s, exceeding hang threshold. Terminating.");
                    TryKill(process);
                    result.TimedOutAsHung = true;
                    result.ExitedNormally = false;
                    result.ExitCode = process.ExitCode;
                    result.Message = "Process exceeded configured hang timeout.";
                    break;
                }
            }


            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private async Task MonitorSnapshotsAsync(
        Process process,
        LaunchSession session,
        CancellationToken cancellationToken)
    {
        string snapshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "snapshots");
        Directory.CreateDirectory(snapshotDir);

        string snapshotFile = Path.Combine(snapshotDir, $"snapshots_{session.SessionId}.log");
        while (!cancellationToken.IsCancellationRequested)
        {
            if (process.HasExited)
                break;

            try
            {
                var snapshot = CaptureSnapshot(process);

                string line =
                    $"{snapshot.TimestampUtc:u} " +
                    $"PID={snapshot.ProcessId} " +
                    $"Name={snapshot.ProcessName} " +
                    $"Exited={snapshot.HasExited} " +
                    $"WorkingSet={snapshot.WorkingSetBytes} " +
                    $"CPU={snapshot.TotalProcessorTime}";

                File.AppendAllText(snapshotFile, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to capture process snapshot: {ex.Message}");
            }

            var delayTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var exitTask = process.WaitForExitAsync(cancellationToken);

            await Task.WhenAny(delayTask, exitTask);
        }
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