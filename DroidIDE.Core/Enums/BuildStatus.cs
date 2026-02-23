namespace DroidIDE.Core.Enums;

/// <summary>
/// Represents the lifecycle state of a <c>dotnet build</c> operation.
/// </summary>
public enum BuildStatus
{
    /// <summary>No build is currently in progress.</summary>
    Idle,

    /// <summary>NuGet package restore is running before the build.</summary>
    Restoring,

    /// <summary>The MSBuild compilation is actively running.</summary>
    Building,

    /// <summary>The build completed with zero errors.</summary>
    Success,

    /// <summary>The build completed with one or more errors.</summary>
    Failed
}
