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
    /// The collection of metadata references used by the background project.
    /// </summary>
    public IEnumerable<MetadataReference> References { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RoslynLanguageService"/>, creating an in-memory
    /// workspace and project with C# latest language version and default BCL references.
    /// </summary>
    public RoslynLanguageService()
    {
        var host = MefHostServices.DefaultHost;
        _workspace = new AdhocWorkspace(host);

        _projectId = ProjectId.CreateNewId("DroidIDE_EditProject");
        References = GetDefaultReferences();

        var projectInfo = ProjectInfo.Create(
            _projectId,
            VersionStamp.Default,
            "EditProject",
            "EditProject",
            LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings("System", "System.Collections.Generic", "System.Linq", "System.Threading.Tasks", "System.IO"),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
            metadataReferences: References
        );

        _workspace.AddProject(projectInfo);
    }

    private static List<MetadataReference> GetDefaultReferences()
    {
        var references = new List<MetadataReference>();
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(System.Linq.Enumerable).Assembly,
            typeof(System.ComponentModel.Component).Assembly,
            typeof(System.Net.Http.HttpClient).Assembly
        };

        foreach (var assembly in assemblies)
        {
            try
            {
                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                else
                {
                    // Fallback for Android/AOT where Location is empty
                    var resourceName = assembly.GetName().Name + ".dll";
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        references.Add(MetadataReference.CreateFromStream(stream));
                    }
                    else
                    {
                        // Last resort: unsafe but often works for BCL in MAUI
                        // Note: This is a placeholder for more advanced assembly resolution if needed
                        System.Diagnostics.Debug.WriteLine($"[Roslyn] Could not load reference for {assembly.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Roslyn] Reference error for {assembly.FullName}: {ex.Message}");
            }
        }

        return references;
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
    /// Finds the source definition location for a symbol at the given character position.
    /// </summary>
    /// <returns>A <see cref="DefinitionInfo"/> or <c>null</c> if no source definition is found.</returns>
    public async Task<DefinitionInfo?> GetDefinitionAsync(string filePath, int position)
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

        // Find source definition location
        var sourceLocation = symbol.Locations.FirstOrDefault(l => l.IsInSource);
        if (sourceLocation is null) return null;

        var span = sourceLocation.GetLineSpan();
        return new DefinitionInfo
        {
            FilePath = span.Path,
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1
        };
    }

    /// <summary>
    /// Finds all source usages (references) of the symbol at the given character position.
    /// </summary>
    public async Task<List<ReferenceInfo>> GetReferencesAsync(string filePath, int position)
    {
        var results = new List<ReferenceInfo>();

        if (!_documents.TryGetValue(filePath, out var docId))
            return results;

        var document = _workspace.CurrentSolution.GetDocument(docId);
        if (document is null) return results;

        var semanticModel = await document.GetSemanticModelAsync();
        if (semanticModel is null) return results;

        var syntaxRoot = await document.GetSyntaxRootAsync();
        if (syntaxRoot is null) return results;

        var token = syntaxRoot.FindToken(position);
        var symbol = semanticModel.GetSymbolInfo(token.Parent!).Symbol
                  ?? semanticModel.GetDeclaredSymbol(token.Parent!);

        if (symbol is null) return results;

        var references = await Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindReferencesAsync(symbol, _workspace.CurrentSolution);

        foreach (var referencedSymbol in references)
        {
            foreach (var reference in referencedSymbol.Locations)
            {
                var span = reference.Location.GetLineSpan();
                results.Add(new ReferenceInfo
                {
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1
                });
            }
        }

        // Also add the definition itself as a reference
        foreach (var location in symbol.Locations.Where(l => l.IsInSource))
        {
            var span = location.GetLineSpan();
            results.Add(new ReferenceInfo
            {
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1
            });
        }

        return results;
    }

    /// <summary>
    /// Gets compiler diagnostics for the specified document from the current workspace compilation.
    /// </summary>
    /// <param name="filePath">The file path of the document to analyze.</param>
    /// <param name="cancellationToken">Token to cancel the diagnostic retrieval.</param>
    /// <returns>A collection of Roslyn diagnostics.</returns>
    public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!_documents.TryGetValue(filePath, out var docId))
            return ImmutableArray<Diagnostic>.Empty;

        var document = _workspace.CurrentSolution.GetDocument(docId);
        if (document is null) return ImmutableArray<Diagnostic>.Empty;

        var compilation = await document.Project.GetCompilationAsync(cancellationToken);
        if (compilation is null) return ImmutableArray<Diagnostic>.Empty;

        // Note: This gets all diagnostics for the file, which is more efficient 
        // than rebuilding the compilation in another service.
        return compilation.GetDiagnostics(cancellationToken);
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

/// <summary>
/// Represents the source definition location of a symbol.
/// </summary>
public class DefinitionInfo
{
    public string FilePath { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int StartColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}

/// <summary>
/// Represents a single reference (usage) of a symbol in source code.
/// </summary>
public class ReferenceInfo
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
}
