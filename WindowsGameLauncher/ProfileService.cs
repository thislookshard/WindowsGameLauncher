using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using WindowsGameLauncher.Models;

namespace WindowsGameLauncher.Services;
public class ProfileService
{
    public GameLaunchProfile Load(string jsonPath)
    {
        
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath)) 
            throw new InvalidDataException("Invalid JSON path");


        string json = File.ReadAllText(jsonPath);
        System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            IncludeFields = true
        };
        
        GameLaunchProfile? profile = null;
        try
        {
            profile = System.Text.Json.JsonSerializer.Deserialize<GameLaunchProfile>(json, options);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidDataException("Failed to parse JSON: " + ex.Message);
        }
        

        if (profile is null)
            throw new InvalidOperationException("Failed to parse JSON: Deserialized object is null");

        ValidateProfile(profile);
        return profile;
    }

    private void ValidateProfile(GameLaunchProfile profile)
    {
        if (string.IsNullOrEmpty(profile.GameName))
            throw new InvalidDataException("Game name is required");

        if (string.IsNullOrEmpty(profile.ExePath)) 
            throw new InvalidDataException("Executable path is required");

        if (!File.Exists(profile.ExePath))
            throw new FileNotFoundException("Executable not found at path: " + profile.ExePath);

        if (string.IsNullOrEmpty(profile.WorkingDirectory))
        {
            profile.WorkingDirectory = Path.GetDirectoryName(profile.ExePath);
        }

    }
}


