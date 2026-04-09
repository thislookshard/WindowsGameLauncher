namespace WindowsGameLauncher.Models;

public class ProcessSnapshot
{
    public DateTimeOffset TimestampUtc { get; set; }
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = "";
    public bool HasExited { get; set; }
    public long WorkingSetBytes { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
}