using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LogService
{
    private readonly string _logDirectory;
    public LogService(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message) => Write(message, "INFO");
    public void Warning(string message) => Write(message, "WARNING");
    public void Error(string message) => Write(message, "ERROR");    

    private void Write(string message, string level)
    {
        string logFilePath = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
}
