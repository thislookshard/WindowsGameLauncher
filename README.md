# Windows Game Launcher

A robust Windows console application for launching games with advanced monitoring, crash recovery, and watchdog functionality. Built with C# and .NET 10.0 with additional PowerShell helper scripts for launch orchestration, cleanup, and diagnostics.

## Features

- **Game Launch Management**: Launch Windows games with customizable profiles
- **Process Monitoring**: Real-time monitoring of game processes with hang detection
- **Automatic Recovery**: Configurable restart policies for crashed or hung games
- **Comprehensive Logging**: Detailed logging of launch attempts, process events, and diagnostics
- **Environment Variable Support**: Custom environment variables per game profile
- **PowerShell Integration**: Helper scripts for launch, cleanup, and diagnostics collection


## Architecture

The application follows a service-oriented architecture:

- **Program.cs**: Main entry point and orchestration
- **LauncherService**: Handles game process creation and startup
- **WatchdogService**: Monitors processes and manages restart logic
- **ProcessMonitorService**: Tracks process state and detects hangs
- **LogService**: Centralized logging functionality
- **ProfileService**: Loads and validates game profiles

## Prerequisites

- **Operating System**: Windows 10 or later
- **.NET Runtime**: .NET 10.0 Runtime
- **Permissions**: Administrator privileges may be required for some games

## Installation

1. Clone or download the repository
2. Build the project using Visual Studio or the .NET CLI:
   ```bash
   dotnet build
   ```

## Usage

### Basic Usage

Run the launcher with a game profile:

```bash
WindowsGameLauncher.exe [profile_path]
```

If no profile path is provided, it defaults to `gameProfile.json` in the same directory as the executable.


### Core flow

1. Load and validate a launch profile
2. Start the target executable with the configured working directory, arguments, and environment variables
3. Monitor the process until it exits or exceeds the configured hang timeout
4. Capture periodic process snapshots while the process is running
5. Log normal exits, abnormal exits, and timeouts
6. Optionally restart the process if configured to do so

### Game Profile Configuration

Create a `gameProfile.json` file with your game configuration:

```json
{
  "gameName": "Your Game Name",
  "exePath": "C:/Path/To/Game.exe",
  "workingDirectory": "C:/Path/To/Game",
  "arguments": "",
  "maxHangTimeSeconds": 300,
  "maxRestarts": 3,
  "restartDelaySeconds": 5,
  "shouldRestartOnCrash": true,
  "environmentVariables": {
    "EXAMPLE_VAR": "value"
  }
}
```

### Profile Properties

- `gameName`: Display name for the game
- `exePath`: Full path to the game executable
- `workingDirectory`: Working directory for the game process
- `arguments`: Command-line arguments to pass to the game
- `maxHangTimeSeconds`: Maximum time (in seconds) before considering the process hung
- `maxRestarts`: Maximum number of restart attempts
- `restartDelaySeconds`: Delay between restart attempts
- `shouldRestartOnCrash`: Whether to restart on crashes or abnormal exits
- `environmentVariables`: Dictionary of environment variables to set

### PowerShell Scripts

The project includes PowerShell scripts in the `scripts/` directory:

#### Launch Script (`launch_game.ps1`)

```powershell
.\scripts\launch_game.ps1 -ProfilePath "path/to/profile.json" -ProjectRoot "."
```

#### Cleanup Script (`cleanup_run.ps1`)

Automatically archives old log files and cleans up temporary files.

#### Diagnostics Script (`collect_diagnostics.ps1`)

Collects system information, running processes, and event logs for troubleshooting.

## Building

### Using Visual Studio

1. Open `WindowsGameLauncher.sln`
2. Select Debug or Release configuration
3. Build → Build Solution

### Using .NET CLI

```bash
# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Publish as self-contained executable
dotnet publish --configuration Release --self-contained true --runtime win-x64
```

## Logging

Logs are written to the `logs/` directory with the format `log_YYYYMMDD.txt`. The application logs:

- Launch attempts and results
- Process events (start, exit, hangs)
- Game output and error streams
- Recovery actions and restart attempts

## Troubleshooting

### Common Issues

1. **"Executable not found"**: Ensure the `exePath` in your profile points to a valid executable
2. **Permission denied**: Some games require administrator privileges
3. **Process hangs not detected**: Adjust `maxHangTimeSeconds` based on your game's behavior

### Diagnostics

Run the diagnostics script to collect system information:

```powershell
.\scripts\collect_diagnostics.ps1
```

This creates a diagnostics file in `logs/diagnostics/` with:
- Running processes
- System information
- Recent event log entries
- Environment variables

## License

This project is licensed under the MIT License - see the LICENSE file for details.