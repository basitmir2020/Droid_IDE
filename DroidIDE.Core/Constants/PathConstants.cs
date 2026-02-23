namespace DroidIDE.Core.Constants;

/// <summary>
/// Path constants for the application's directory structure on Android/Linux.
/// </summary>
public static class PathConstants
{
    /// <summary>Base directory for user workspaces.</summary>
    public const string WorkspaceRoot = "/data/data/com.companyname.droidide.app/files/workspaces";

    /// <summary>Path where the .NET SDK is installed on the device.</summary>
    public const string DotnetSdkPath = "/data/data/com.companyname.droidide.app/files/dotnet";

    /// <summary>Temp directory for build artifacts and scratch files.</summary>
    public const string TempPath = "/data/data/com.companyname.droidide.app/cache/tmp";

    /// <summary>Path for application settings and state.</summary>
    public const string AppDataPath = "/data/data/com.companyname.droidide.app/files/appdata";

    /// <summary>Path for log files.</summary>
    public const string LogPath = "/data/data/com.companyname.droidide.app/files/logs";
}
