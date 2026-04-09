using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using WindowsGameLauncher.Models;

public class LauncherService
{
    private LogService _logService;
    public LauncherService(LogService logService)
    {
        _logService = logService;
    }

    public Process LaunchGame(GameLaunchProfile profile, int attempt, out LaunchSession session)
    {   
        session = null;
        Process process = null;
  
        _logService.Info($"Launching game: {profile.GameName}");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = profile.ExePath,
            WorkingDirectory = profile.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var envVar in profile.environmentVariables)
        {
            startInfo.Environment[envVar.Key] = envVar.Value.ToString();
        }

        process = new Process
        {
            StartInfo = startInfo
        };

        process.OutputDataReceived += (sender, args) => 
        {
            if (!string.IsNullOrEmpty(args.Data))
                _logService.Info($"[Game Output] {args.Data}");
        };
        process.ErrorDataReceived += (sender, args) => 
        {
            if (!string.IsNullOrEmpty(args.Data))
                _logService.Error($"[Game Error] {args.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _logService.Info($"Game launched with PID: {process.Id}");
        
        session = new LaunchSession
        {
            ProcessId = process.Id,
            ProfileName = profile.GameName,
            AttemptNumber = attempt
        };

        // result.Session = session;
        // result.StartedSuccessfully = true;
        // result.TimedOutAsHung = false;
        // result.Message = "Success";


        return process;
    }

}