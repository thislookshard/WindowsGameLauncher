namespace WindowsGameLauncher.Models;

public class LaunchResult
{
    public bool StartedSuccessfully { get; set; }
    public bool ExitedNormally { get; set; }
    public bool TimedOutAsHung { get; set; }
    public int? ExitCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public LaunchSession Session { get; set; } = new LaunchSession();
}