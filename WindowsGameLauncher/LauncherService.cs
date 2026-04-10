using System.Diagnostics;
using WindowsGameLauncher.Models;

namespace WindowsGameLauncher.Services;

public class LauncherService
{
    private readonly LogService _logService;

    public LauncherService(LogService logService)
    {
        _logService = logService;
    }

    public Process Launch(GameLaunchProfile profile, int attempt, out LaunchSession session)
    {
        _logService.Info($"Launching game: {profile.GameName}");

        var startInfo = new ProcessStartInfo
        {
            FileName = profile.ExePath,
            WorkingDirectory = profile.WorkingDirectory,
            Arguments = profile.Arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var envVar in profile.EnvironmentVariables)
        {
            startInfo.Environment[envVar.Key] = envVar.Value;
        }

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
                _logService.Info($"[Game Output] {args.Data}");
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
                _logService.Error($"[Game Error] {args.Data}");
        };

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start '{profile.GameName}'.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        session = new LaunchSession
        {
            ProcessId = process.Id,
            ProfileName = profile.GameName,
            AttemptNumber = attempt,
            StartTimeUtc = DateTimeOffset.UtcNow
        };

        _logService.Info($"Game launched with PID: {process.Id}");

        return process;
    }
}