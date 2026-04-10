using System.Text.Json;
using WindowsGameLauncher.Models;

namespace WindowsGameLauncher.Services;

public class ProfileService
{
    public GameLaunchProfile Load(string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath))
            throw new InvalidDataException($"Invalid JSON path: '{jsonPath}'");

        string json = File.ReadAllText(jsonPath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        GameLaunchProfile? profile;
        try
        {
            profile = JsonSerializer.Deserialize<GameLaunchProfile>(json, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Failed to parse JSON: " + ex.Message, ex);
        }

        if (profile is null)
            throw new InvalidOperationException("Failed to parse JSON: deserialized object is null.");

        ValidateProfile(profile);
        return profile;
    }

    private static void ValidateProfile(GameLaunchProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.GameName))
            throw new InvalidDataException("GameName is required.");

        if (string.IsNullOrWhiteSpace(profile.ExePath))
            throw new InvalidDataException("ExePath is required.");

        if (!File.Exists(profile.ExePath))
            throw new FileNotFoundException("Executable not found.", profile.ExePath);

        if (string.IsNullOrWhiteSpace(profile.WorkingDirectory))
            profile.WorkingDirectory = Path.GetDirectoryName(profile.ExePath) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(profile.WorkingDirectory) || !Directory.Exists(profile.WorkingDirectory))
            throw new DirectoryNotFoundException(
                $"Working directory does not exist: '{profile.WorkingDirectory}'");
    }
}