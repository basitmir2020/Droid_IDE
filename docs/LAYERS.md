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

- MVVM ViewModels (`MainShellViewModel`, `ExplorerViewModel`, `EditorViewModel`, `TerminalViewModel`)
- Visual layout (`MainShell`, `ActivityBarView`, `ExplorerView`, `EditorTabsView`, `EditorHostView`, `BottomPanelView`, `StatusBarView`)
- Application services (`EditorService`)
- Value converters for data binding
- DI registration in `MauiProgram.cs`
- **Dependencies:** DroidIDE.Core, DroidIDE.Infrastructure, DroidIDE.Terminal
- **Status:** ✅ Implemented

---

## DroidIDE.Editor
**Code editor with Monaco WebView and Roslyn language services.**

- Monaco editor integration via WebView + JS interop
- Roslyn-powered IntelliSense, diagnostics, and refactoring
- **Dependencies:** DroidIDE.Core
- **Status:** 🔲 Planned (Phase 3)

---

## DroidIDE.Runtime
**dotnet CLI wrapper for build, run, and project management commands.**

- `DotnetCliService` — wrapper for `dotnet build`, `dotnet run`, `dotnet new`, etc.
- Build output streaming and error parsing
- Process lifecycle management for running apps
- **Dependencies:** DroidIDE.Core, DroidIDE.Infrastructure
- **Status:** 🔲 Planned (Phase 2)

---

## DroidIDE.ProjectSystem
**Solution and project file parsing.**

- `SolutionParser` — parse `.sln` and `.slnx` files
- `ProjectParser` — parse `.csproj` files for metadata and references
- `NuGetManager` — package management
- **Dependencies:** DroidIDE.Core
- **Status:** 🔲 Planned (Phase 2)

---

## DroidIDE.Git
**Git version control via LibGit2Sharp.**

- `GitService` — clone, commit, push, pull, branch operations
- `BranchManager` — branch listing and management
- `RepositoryManager` — repo state and file status tracking
- **Dependencies:** DroidIDE.Core, LibGit2Sharp
- **Status:** 🔲 Planned (Phase 4)