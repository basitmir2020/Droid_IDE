namespace DroidIDE.Core.Enums;

/// <summary>
/// Represents the version control status of a file within a Git repository working directory.
/// </summary>
public enum GitFileStatus
{
    /// <summary>The file has not been modified since the last commit.</summary>
    Unmodified,

    /// <summary>The file has been modified in the working directory.</summary>
    Modified,

    /// <summary>The file has been newly added and staged for commit.</summary>
    Added,

    /// <summary>The file has been deleted from the working directory.</summary>
    Deleted,

    /// <summary>The file has been renamed (detected via content similarity).</summary>
    Renamed,

    /// <summary>The file exists in the working directory but is not tracked by Git.</summary>
    Untracked,

    /// <summary>The file matches a .gitignore pattern and is ignored by Git.</summary>
    Ignored,

    /// <summary>The file has merge conflicts that must be resolved.</summary>
    Conflicted
}
