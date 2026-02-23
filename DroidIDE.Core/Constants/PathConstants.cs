namespace DroidIDE.Core.Constants;

/// <summary>
/// Path constants for the application's directory structure on the Android device.
/// Paths reference the app's sandboxed data directory under <c>/data/data/{package}</c>.
/// </summary>
public static class PathConstants
{
    /// <summary>Base directory for user project workspaces on the device.</summary>
    public const string WorkspaceRoot = "/data/data/com.companyname.droidide.app/files/workspaces";

    /// <summary>Installation directory for the .NET SDK binaries on the device.</summary>
    public const string DotnetSdkPath = "/data/data/com.companyname.droidide.app/files/dotnet";

    /// <summary>Temporary directory for build artifacts, scratch files, and cache data.</summary>
    public const string TempPath = "/data/data/com.companyname.droidide.app/cache/tmp";

    /// <summary>Directory for persisted application settings, state, and configuration files.</summary>
    public const string AppDataPath = "/data/data/com.companyname.droidide.app/files/appdata";

    /// <summary>Directory where application log files are stored by <c>AppLogger</c>.</summary>
    public const string LogPath = "/data/data/com.companyname.droidide.app/files/logs";
}
