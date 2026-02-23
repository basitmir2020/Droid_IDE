namespace DroidIDE.Core.Enums;

/// <summary>
/// Represents the status of a file in a Git repository.
/// </summary>
public enum GitFileStatus
{
    Unmodified,
    Modified,
    Added,
    Deleted,
    Renamed,
    Untracked,
    Ignored,
    Conflicted
}
