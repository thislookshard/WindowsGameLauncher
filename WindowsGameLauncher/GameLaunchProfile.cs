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
    public int MaxHangTimeSeconds { get; set; } = 300;
    public int MaxRestarts { get; set; } = 3;
    public int RestartDelaySeconds { get; set; } = 5;
    public bool ShouldRestartOnCrash { get; set; } = true;
    public Dictionary<string,object> launchArgs { get; set; } = new();
    public Dictionary<string,object> restartSettings { get; set; } = new();
    public Dictionary<string,object> environmentVariables { get; set; } = new();
}
