# Editor Layer — `DroidIDE.Editor`

> **Status:** 🔲 Planned for Phase 3

The Editor layer will handle code intelligence — Monaco editor integration, Roslyn language services, diagnostics, and refactoring.

---

## Planned Structure

```
DroidIDE.Editor/
├── Monaco/
│   └── MonacoEditorBridge.cs      # JS interop bridge (MAUI WebView ↔ Monaco)
├── LanguageServer/
│   └── RoslynLanguageService.cs   # Roslyn workspace host for IntelliSense
├── Diagnostics/
│   └── DiagnosticService.cs       # Real-time error/warning analysis
└── Refactoring/
    └── RefactoringService.cs      # Roslyn code actions (rename, extract, etc.)
```

---

## Planned Features

### Monaco Integration
- Load Monaco editor via embedded HTML/JS in a MAUI `WebView`
- JS interop bridge for: `SetContent`, `GetContent`, `SetLanguage`, `SetTheme`
- Forward cursor position and keyboard events to native code
- Apply diagnostics as squiggly underlines via `monaco.editor.setModelMarkers`

### Roslyn Language Server
- Host a Roslyn `AdhocWorkspace` or `MSBuildWorkspace` in-process
- Provide completions (IntelliSense) via `CompletionService`
- Provide signature help via `SignatureHelpService`
- Provide hover info via `QuickInfoService`

### Diagnostics
- Real-time error/warning analysis using Roslyn analyzers
- Push diagnostics to Monaco via JS interop (red/yellow squiggles)
- Sync diagnostics to the Problems panel in BottomPanelView

### Refactoring
- Roslyn-powered code actions: Rename, Extract Method, Add Using, Generate Constructor
- Surface refactoring suggestions in Monaco's light bulb menu

---

## Dependencies (Planned)

- `DroidIDE.Core`
- `Microsoft.CodeAnalysis.CSharp` (Roslyn)
- `Microsoft.CodeAnalysis.CSharp.Features`

---

## Current State

`MonacoEditorView.xaml` exists as an empty `ContentView` placeholder. The `EditorHostView` currently uses a native MAUI `Editor` control for basic text editing. In Phase 3, this will be replaced with a `WebView` hosting the Monaco editor.