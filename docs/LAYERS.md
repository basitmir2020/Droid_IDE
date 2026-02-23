# Layer Overview

DroidIDE is composed of **8 projects** organized in a clean architecture. Each layer has a specific responsibility and strict dependency rules.

---

## DroidIDE.Core
**Domain layer — the foundation everything depends on.**

- Domain models (`FileItem`, `EditorTab`, `TerminalSession`, `ProjectInfo`, etc.)
- Service interfaces (`IFileSystemService`, `IProcessManager`, `ITerminalService`, etc.)
- Enums (`FileType`, `BuildStatus`, `DiagnosticSeverity`, `GitFileStatus`)
- Constants (`AppConstants`, `PathConstants`)
- **Dependencies:** None. This is the innermost layer.
- **Status:** ✅ Implemented

---

## DroidIDE.Infrastructure
**OS-level services — file I/O, process execution, environment config.**

- `FileSystemService` — file/directory CRUD, recursive tree building
- `ProcessManager` — sync + streaming process execution via `System.Diagnostics.Process`
- `LinuxEnvironmentManager` — dotnet SDK path and env variable configuration
- `CredentialStore` — JSON-based secure credential persistence
- **Dependencies:** DroidIDE.Core
- **Status:** ✅ Implemented

---

## DroidIDE.Terminal
**Terminal session management and output parsing.**

- `ProcessSessionManager` — multi-session terminal with IProcessManager
- `AnsiParser` — ANSI escape code stripping
- `OutputLineParser` — MSBuild diagnostic extraction from build output
- **Dependencies:** DroidIDE.Core
- **Status:** ✅ Implemented

---

## DroidIDE.App
**UI layer — .NET MAUI views, ViewModels, services, and DI.**

- MVVM ViewModels (`MainShellViewModel`, `ExplorerViewModel`, `EditorViewModel`, `TerminalViewModel`, `SearchViewModel`, `GitViewModel`, `SettingsViewModel`)
- Visual layout (12 views including `MainShell`, `ExplorerView`, `EditorHostView`, `BottomPanelView`, `GitPanelView`, `SearchPanelView`, `CommandPaletteView`, `WelcomeView`, `SettingsView`)
- Application services (`EditorService`, `SettingsService`, `SearchService`, `CommandPaletteService`, `KeyboardShortcutService`, `FileWatcherService`, `AppStateService`, `ThemeService`, `AppLogger`, `GlobalExceptionHandler`)
- Value converters for data binding (`BoolToIcon`, `TabColor`, `IsNull`, `IsNotNull`, `InverseBool`)
- Theme system with `DarkTheme.xaml` and `LightTheme.xaml` resource dictionaries
- DI registration in `MauiProgram.cs`
- **Dependencies:** DroidIDE.Core, DroidIDE.Infrastructure, DroidIDE.Terminal
- **Status:** ✅ Implemented

---

## DroidIDE.Editor
**Code editor with Monaco WebView and Roslyn language services.**

- `MonacoEditorBridge` — JS interop between MAUI WebView and Monaco
- `RoslynLanguageService` — Roslyn-powered IntelliSense and analysis
- `DiagnosticService` — real-time error/warning detection
- `RefactoringService` — Roslyn code actions (rename, extract, etc.)
- **Dependencies:** DroidIDE.Core, Microsoft.CodeAnalysis
- **Status:** ✅ Implemented

---

## DroidIDE.Runtime
**dotnet CLI wrapper for build, run, and project management commands.**

- `DotnetCliService` — wrapper for `dotnet build`, `dotnet run`, `dotnet new`, etc.
- `BuildService` — build execution with structured output and error parsing
- `RunService` — process lifecycle management for running apps
- `ServiceManager` — multi-service tracking for microservices
- **Dependencies:** DroidIDE.Core, DroidIDE.Infrastructure
- **Status:** ✅ Implemented

---

## DroidIDE.ProjectSystem
**Solution and project file parsing.**

- `SolutionParser` — parse `.sln` and `.slnx` files
- `ProjectParser` — parse `.csproj` files for metadata and references
- `NuGetManager` — package management
- **Dependencies:** DroidIDE.Core
- **Status:** ✅ Implemented

---

## DroidIDE.Git
**Git version control via LibGit2Sharp.**

- `GitService` — clone, commit, push, pull, branch operations
- `BranchManager` — branch listing and management
- `RepositoryManager` — repo state and file status tracking
- **Dependencies:** DroidIDE.Core, LibGit2Sharp
- **Status:** ✅ Implemented