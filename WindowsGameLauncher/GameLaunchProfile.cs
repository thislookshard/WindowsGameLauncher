using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace WindowsGameLauncher.Models;


public class GameLaunchProfile
{
    public string GameName { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public int MaxHangTimeSeconds { get; set; } = 300;
    public int MaxRestarts { get; set; } = 3;
    public int RestartDelaySeconds { get; set; } = 5;
    public bool ShouldRestartOnCrash { get; set; } = true;

    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
}