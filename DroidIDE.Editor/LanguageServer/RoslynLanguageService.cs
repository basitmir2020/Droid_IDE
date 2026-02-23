using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace DroidIDE.Editor.LanguageServer;

/// <summary>
/// Hosts a Roslyn <see cref="AdhocWorkspace"/> for C# code analysis.
/// Provides IntelliSense completions and hover/quick-info for open documents.
/// </summary>
/// <remarks>
/// Creates an in-memory project with core BCL references and manages documents
/// as they are opened, modified, and closed in the editor.
/// Implements <see cref="IDisposable"/> to release the underlying workspace.
/// </remarks>
public class RoslynLanguageService : IDisposable
{
    private readonly AdhocWorkspace _workspace;
    private readonly ProjectId _projectId;
    private readonly Dictionary<string, DocumentId> _documents = new();

    /// <summary>
    /// Core BCL metadata references loaded from the running assembly for basic compilation support.
    /// </summary>
    private static readonly MetadataReference[] DefaultReferences =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
    ];

    /// <summary>
    /// Initializes a new instance of <see cref="RoslynLanguageService"/>, creating an in-memory
    /// workspace and project with C# latest language version and default BCL references.
    /// </summary>
    public RoslynLanguageService()
    {
        var host = MefHostServices.DefaultHost;
        _workspace = new AdhocWorkspace(host);

        _projectId = ProjectId.CreateNewId("DroidIDE_EditProject");
        var projectInfo = ProjectInfo.Create(
            _projectId,
            VersionStamp.Default,
            "EditProject",
            "EditProject",
            LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
            metadataReferences: DefaultReferences
        );

        _workspace.AddProject(projectInfo);
    }

    /// <summary>
    /// Adds a new document or updates an existing document's content in the Roslyn workspace.
    /// </summary>
    /// <param name="filePath">The absolute file path used as the document identifier.</param>
    /// <param name="content">The current source code content of the file.</param>
    public void UpdateDocument(string filePath, string content)
    {
        var sourceText = SourceText.From(content);

        if (_documents.TryGetValue(filePath, out var existingDocId))
        {
            // Update existing document
            var solution = _workspace.CurrentSolution.WithDocumentText(existingDocId, sourceText);
            _workspace.TryApplyChanges(solution);
        }
        else
        {
            // Add new document
            var docId = DocumentId.CreateNewId(_projectId, filePath);
            var docInfo = DocumentInfo.Create(
                docId,
                Path.GetFileName(filePath),
                loader: TextLoader.From(TextAndVersion.Create(sourceText, VersionStamp.Create())),
                filePath: filePath
            );

            var solution = _workspace.CurrentSolution.AddDocument(docInfo);
            _workspace.TryApplyChanges(solution);
            _documents[filePath] = docId;
        }
    }

    /// <summary>
    /// Removes a document from the Roslyn workspace, ceasing analysis for that file.
    /// </summary>
    /// <param name="filePath">The file path of the document to remove.</param>
    public void RemoveDocument(string filePath)
    {
        if (_documents.TryGetValue(filePath, out var docId))
        {
            var solution = _workspace.CurrentSolution.RemoveDocument(docId);
            _workspace.TryApplyChanges(solution);
            _documents.Remove(filePath);
        }
    }

    /// <summary>
    /// Gets IntelliSense completion items at a given character position in a document.
    /// </summary>
    /// <param name="filePath">The file path of the document to query.</param>
    /// <param name="position">The 0-based character offset in the document.</param>
    /// <returns>A list of up to 50 <see cref="CompletionItemInfo"/> entries, limited for performance.</returns>
    public async Task<List<CompletionItemInfo>> GetCompletionsAsync(string filePath, int position)
    {
        var results = new List<CompletionItemInfo>();

        if (!_documents.TryGetValue(filePath, out var docId))
            return results;

        var document = _workspace.CurrentSolution.GetDocument(docId);
        if (document is null) return results;

        var completionService = CompletionService.GetService(document);
        if (completionService is null) return results;

        var completions = await completionService.GetCompletionsAsync(document, position);
        if (completions is null) return results;

        foreach (var item in completions.ItemsList.Take(50)) // Limit for performance
        {
            results.Add(new CompletionItemInfo
            {
                Label = item.DisplayText,
                Kind = MapCompletionKind(item.Tags),
                InsertText = item.DisplayText,
                FilterText = item.FilterText
            });
        }

        return results;
    }

    /// <summary>
    /// Gets hover/quick-info for the symbol at the given character position.
    /// </summary>
    /// <param name="filePath">The file path of the document to query.</param>
    /// <param name="position">The 0-based character offset in the document.</param>
    /// <returns>A display string showing the symbol kind and full name, or <c>null</c> if no symbol is found.</returns>
    public async Task<string?> GetHoverInfoAsync(string filePath, int position)
    {
        if (!_documents.TryGetValue(filePath, out var docId))
            return null;

        var document = _workspace.CurrentSolution.GetDocument(docId);
        if (document is null) return null;

        var semanticModel = await document.GetSemanticModelAsync();
        if (semanticModel is null) return null;

        var syntaxRoot = await document.GetSyntaxRootAsync();
        if (syntaxRoot is null) return null;

        var token = syntaxRoot.FindToken(position);
        var symbol = semanticModel.GetSymbolInfo(token.Parent!).Symbol
                  ?? semanticModel.GetDeclaredSymbol(token.Parent!);

        if (symbol is null) return null;

        return $"{symbol.Kind}: {symbol.ToDisplayString()}";
    }

    /// <summary>
    /// Maps Roslyn completion item tags to Monaco <c>CompletionItemKind</c> numeric values.
    /// </summary>
    /// <param name="tags">The immutable array of Roslyn WellKnownTags for the completion item.</param>
    /// <returns>A Monaco CompletionItemKind integer value.</returns>
    private static int MapCompletionKind(ImmutableArray<string> tags)
    {
        if (tags.Contains("Method")) return 0;      // Method
        if (tags.Contains("Property")) return 9;     // Property
        if (tags.Contains("Field")) return 4;        // Field
        if (tags.Contains("Class")) return 5;        // Class
        if (tags.Contains("Interface")) return 7;    // Interface
        if (tags.Contains("Enum")) return 15;        // Enum
        if (tags.Contains("Keyword")) return 13;     // Keyword
        if (tags.Contains("Namespace")) return 8;    // Module
        if (tags.Contains("Variable") || tags.Contains("Local")) return 5; // Variable
        return 0; // Default: Method
    }

    /// <summary>
    /// Disposes the underlying <see cref="AdhocWorkspace"/> and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _workspace.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a completion item returned by the Roslyn completion service,
/// mapped to a format compatible with the Monaco editor.
/// </summary>
public class CompletionItemInfo
{
    /// <summary>Gets or sets the display text shown in the completion list.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the Monaco CompletionItemKind value (0=Method, 5=Class, etc.).</summary>
    public int Kind { get; set; }

    /// <summary>Gets or sets the text inserted when the completion is accepted.</summary>
    public string InsertText { get; set; } = string.Empty;

    /// <summary>Gets or sets the filter text used for fuzzy matching in the completion widget.</summary>
    public string? FilterText { get; set; }

    /// <summary>Gets or sets additional detail text shown beside the completion label.</summary>
    public string? Detail { get; set; }
}
