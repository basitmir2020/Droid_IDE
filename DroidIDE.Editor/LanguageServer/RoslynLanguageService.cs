using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace DroidIDE.Editor.LanguageServer;

/// <summary>
/// Hosts a Roslyn AdhocWorkspace for C# code analysis.
/// Provides IntelliSense completions, hover info, and signature help.
/// </summary>
public class RoslynLanguageService : IDisposable
{
    private readonly AdhocWorkspace _workspace;
    private readonly ProjectId _projectId;
    private readonly Dictionary<string, DocumentId> _documents = new();

    // Core BCL references for basic compilation support
    private static readonly MetadataReference[] DefaultReferences =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
    ];

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
    /// Add or update a document in the workspace.
    /// </summary>
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
    /// Remove a document from the workspace.
    /// </summary>
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
    /// Get completion items at a given position in a document.
    /// </summary>
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
    /// Get hover/quick info at a given position in a document.
    /// </summary>
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
    /// Convert Roslyn completion tags to Monaco CompletionItemKind values.
    /// </summary>
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

    public void Dispose()
    {
        _workspace.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a completion item returned by Roslyn.
/// </summary>
public class CompletionItemInfo
{
    public string Label { get; set; } = string.Empty;
    public int Kind { get; set; }
    public string InsertText { get; set; } = string.Empty;
    public string? FilterText { get; set; }
    public string? Detail { get; set; }
}
