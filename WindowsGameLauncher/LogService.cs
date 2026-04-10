using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowsGameLauncher.Services;

public class LogService
{
    private readonly string _logDirectory;

    public LogService(string logDirectory)
    {
        if (string.IsNullOrWhiteSpace(logDirectory))
            throw new ArgumentException("Log directory is required.", nameof(logDirectory));

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

        Console.WriteLine(logEntry);
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
}