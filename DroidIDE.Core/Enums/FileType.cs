namespace DroidIDE.Core.Enums;

/// <summary>
/// Classifies a file type for syntax highlighting and icon selection in the explorer and editor.
/// </summary>
public enum FileType
{
    /// <summary>An unrecognized file type with no special handling.</summary>
    Unknown,

    /// <summary>A C# source file (.cs).</summary>
    CSharp,

    /// <summary>An XML file (.xml).</summary>
    Xml,

    /// <summary>A XAML markup file (.xaml).</summary>
    Xaml,

    /// <summary>A JSON data file (.json).</summary>
    Json,

    /// <summary>A Markdown documentation file (.md).</summary>
    Markdown,

    /// <summary>An HTML file (.html, .htm).</summary>
    Html,

    /// <summary>A CSS stylesheet file (.css).</summary>
    Css,

    /// <summary>A JavaScript source file (.js).</summary>
    JavaScript,

    /// <summary>A TypeScript source file (.ts).</summary>
    TypeScript,

    /// <summary>A plain text file (.txt).</summary>
    Text,

    /// <summary>A .NET solution file (.sln, .slnx).</summary>
    Solution,

    /// <summary>A .NET project file (.csproj).</summary>
    Project
}
