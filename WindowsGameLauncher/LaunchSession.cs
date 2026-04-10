namespace WindowsGameLauncher.Models;
public class LaunchSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string ProfileName {get; set; } = string.Empty;
    public DateTimeOffset StartTimeUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndTimeUtc { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public int ProcessId { get; set; }
    public bool Restarted { get; set; } = false;
}