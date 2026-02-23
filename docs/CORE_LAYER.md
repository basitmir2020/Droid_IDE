# Core Layer — `DroidIDE.Core`

The Core layer is the **domain foundation** of DroidIDE. It contains all shared models, interfaces, enums, and constants that other layers depend on. Core has **zero external dependencies** — it references no other project, no MAUI, and no NuGet packages.

---

## Directory Structure

```
DroidIDE.Core/
├── Models/
│   ├── FileItem.cs           # File/folder tree node for Explorer
│   ├── EditorTab.cs          # Open editor tab state
│   ├── TerminalSession.cs    # Terminal session state
│   ├── ProjectInfo.cs        # .csproj project metadata
│   ├── SolutionInfo.cs       # .sln solution metadata
│   ├── BuildResult.cs        # Build output result
│   └── DiagnosticItem.cs     # Code error/warning item
├── Interfaces/
│   ├── IFileSystemService.cs # File/directory operations
│   ├── IProcessManager.cs    # OS process execution
│   ├── ITerminalService.cs   # Terminal session management
│   ├── IDotnetCli.cs         # dotnet CLI commands
│   ├── IProjectParser.cs     # .csproj parsing
│   ├── ISolutionParser.cs    # .sln parsing
│   ├── IGitService.cs        # Git operations
│   └── IEditorService.cs     # High-level editor operations
├── Enums/
│   ├── FileType.cs           # File type classification
│   ├── BuildStatus.cs        # Build lifecycle states
│   ├── DiagnosticSeverity.cs # Error/Warning/Info/Hint
│   └── GitFileStatus.cs      # Git file change status
└── Constants/
    ├── AppConstants.cs        # App config defaults
    └── PathConstants.cs       # Directory structure paths
```

---

## Models

| Model | Purpose | Key Properties |
|-------|---------|----------------|
| `FileItem` | Explorer tree node | `Name`, `FullPath`, `IsDirectory`, `Children`, `Extension`, `Icon` |
| `EditorTab` | Open file tab | `FilePath`, `DisplayName`, `Content`, `IsModified`, `IsActive`, `Language`, `CursorLine` |
| `TerminalSession` | Shell session | `Id`, `IsRunning`, `OutputLines`, `WorkingDirectory` |
| `ProjectInfo` | .csproj metadata | `Name`, `FilePath`, `TargetFramework`, `OutputType`, `PackageReferences` |
| `SolutionInfo` | .sln metadata | `Name`, `FilePath`, `Projects`, `StartupProject` |
| `BuildResult` | Build output | `Success`, `Diagnostics`, `ErrorCount`, `WarningCount`, `Duration` |
| `DiagnosticItem` | Code diagnostic | `FilePath`, `Line`, `Column`, `Message`, `Severity`, `Code` |

---

## Interfaces

| Interface | Implemented By | Layer |
|-----------|---------------|-------|
| `IFileSystemService` | `FileSystemService` | Infrastructure |
| `IProcessManager` | `ProcessManager` | Infrastructure |
| `ITerminalService` | `ProcessSessionManager` | Terminal |
| `IEditorService` | `EditorService` | App |
| `IDotnetCli` | *(Phase 2)* | Runtime |
| `IProjectParser` | *(Phase 2)* | ProjectSystem |
| `ISolutionParser` | *(Phase 2)* | ProjectSystem |
| `IGitService` | *(Phase 4)* | Git |

---

## Rules

1. **No MAUI references** — Core is a plain `net10.0` library.
2. **No OS-specific logic** — File paths, processes, and environment config belong in Infrastructure.
3. **No external NuGet packages** — Keep Core dependency-free.
4. **Interfaces define contracts** — Implementations live in other layers.
5. **Models are POCOs** — Simple data classes with minimal behavior (computed properties are okay).