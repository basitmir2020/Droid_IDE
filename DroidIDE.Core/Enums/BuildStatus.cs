namespace DroidIDE.Core.Enums;

/// <summary>
/// Represents the current state of a dotnet build operation.
/// </summary>
public enum BuildStatus
{
    Idle,
    Restoring,
    Building,
    Success,
    Failed
}
