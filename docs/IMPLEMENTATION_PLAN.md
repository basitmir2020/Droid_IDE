# Implementation Plan

This document outlines the step-by-step implementation plan for all missing features in DroidIDE. The plan is organized into 5 phases, aligned with the project roadmap. Each phase builds upon the previous one.

---

## Phase 1 — Foundation & UI Shell

**Goal:** Get a working IDE shell with file browsing, editing, and terminal.

### 1.1 Core Layer — Domain Models & Contracts

> Build the shared contracts that all other layers depend on.

- **Models/**
  - `FileItem` — represents a file/folder in the explorer tree (Name, Path, IsDirectory, Children, Extension)
  - `EditorTab` — represents an open tab (FilePath, DisplayName, IsModified, Content)
  - `TerminalSession` — represents a terminal session (Id, IsRunning, OutputLines)
  - `ProjectInfo` — represents a loaded project (Name, Path, TargetFramework, References)
  - `SolutionInfo` — represents a loaded solution (Name, Path, Projects)
  - `BuildResult` — represents a build output (Success, Errors, Warnings, Output)
  - `DiagnosticItem` — represents a code error/warning (FilePath, Line, Column, Message, Severity)

- **Interfaces/**
  - `IFileSystemService` — ReadFile, WriteFile, ListDirectory, CreateFile, DeleteFile, FileExists, Watch
  - `IProcessManager` — StartProcess, KillProcess, GetOutput, SendInput
  - `ITerminalService` — CreateSession, SendCommand, GetOutput, CloseSession
  - `IDotnetCli` — Build, Run, New, Restore, Test, Clean
  - `IProjectParser` — ParseCsproj, GetReferences, GetTargetFramework
  - `ISolutionParser` — ParseSln, GetProjects
  - `IGitService` — Clone, Commit, Push, Pull, GetStatus, GetBranches, Checkout
  - `IEditorService` — OpenFile, SaveFile, GetDiagnostics, FormatCode

- **Enums/**
  - `FileType` — CSharp, Xml, Json, Markdown, Unknown
  - `BuildStatus` — Idle, Building, Success, Failed
  - `DiagnosticSeverity` — Error, Warning, Info, Hint
  - `GitFileStatus` — Modified, Added, Deleted, Untracked, Renamed

- **Constants/**
  - `AppConstants` — DefaultTheme, MaxTabs, TerminalBufferSize
  - `PathConstants` — WorkspacePath, SdkPath, TempPath

### 1.2 Infrastructure Layer — File System & Process Management

> Implement the OS-level services that other layers need.

- **FileSystem/**
  - `FileSystemService : IFileSystemService` — full implementation using System.IO
  - File watching using `FileSystemWatcher` for live reload

- **Process/**
  - `ProcessManager : IProcessManager` — wrapper around `System.Diagnostics.Process`
  - Support for stdout/stderr streaming
  - Process lifecycle management (start, kill, wait)

- **Linux/**
  - `LinuxEnvironmentManager` — detect/configure Linux environment on Android (Termux/proot)
  - `EnvironmentSetup` — PATH, HOME, SDK path configuration

- **Security/**
  - `CredentialStore` — secure storage for Git credentials using Android Keystore
  - `PermissionManager` — handle Android storage/network permissions

### 1.3 App Layer — Dependency Injection & ViewModels

> Wire everything together and add real logic to the UI.

- **MauiProgram.cs**
  - Register all services (IFileSystemService, IProcessManager, etc.)
  - Register all ViewModels
  - Configure logging with Serilog or Microsoft.Extensions.Logging

- **ViewModels/**
  - `MainShellViewModel` — app state, active panel tracking, layout commands
  - `ExplorerViewModel` — folder tree loading, file selection, context menu actions
  - `EditorViewModel` — open/save files, tab management, content binding
  - `TerminalViewModel` — session lifecycle, command input/output binding
  - `BottomPanelViewModel` — tab switching (Terminal/Output/Problems), panel content

### 1.4 App Layer — Functional UI Views

> Replace all placeholder views with real, data-bound UI.

- **ExplorerView** — bind to ExplorerViewModel, show recursive folder tree with expand/collapse
- **EditorTabsView** — bind to EditorViewModel.OpenTabs, support close/switch/reorder
- **EditorHostView** — embed a WebView for Monaco editor with JS interop
- **BottomPanelView** — bind to TerminalViewModel, show live terminal output with input Entry
- **ActivityBarView** — wire up buttons to toggle sidebar panels (Explorer, Search, Git, Debug)
- **Add StatusBarView** — show current file, line/column, build status, git branch

### 1.5 Terminal Layer — Shell Sessions

> Get a working terminal inside the app.

- **Shell/**
  - `ShellSession` — manages a single bash/sh session on Android using Process
  - Support for PTY-like interaction via stdin/stdout streams

- **ProcessSession/**
  - `ProcessSession` — wraps a running process with output buffering
  - `ProcessSessionManager` — track multiple concurrent process sessions

- **OutputParser/**
  - `AnsiParser` — strip or convert ANSI escape codes for display
  - `OutputLineParser` — parse output lines into structured data (for error detection)

---

## Phase 2 — Project System & Build/Run

**Goal:** Open .NET projects, build, and run console apps.

### 2.1 Project System Layer

- **SolutionParser/**
  - `SolutionParser : ISolutionParser` — parse .sln files to extract project list
  - Support for .slnx (XML-based solution format)

- **ProjectParser/**
  - `ProjectParser : IProjectParser` — parse .csproj files using XML
  - Extract TargetFramework, PackageReferences, ProjectReferences, OutputType

- **NuGetManager/**
  - `NuGetManager` — run dotnet restore, list installed packages
  - Show package versions and update availability

### 2.2 Runtime Layer — dotnet CLI

- **DotnetManager/**
  - `DotnetCliService : IDotnetCli` — wrapper for all dotnet CLI commands
  - SDK version detection and validation

- **Build/**
  - `BuildService` — execute dotnet build, parse MSBuild output
  - Real-time build output streaming to the Problems/Output panels
  - Error/warning extraction from build output

- **Run/**
  - `RunService` — execute dotnet run for console apps
  - Process lifecycle management (start/stop/restart)
  - Output streaming to terminal panel

### 2.3 App Layer — Project Integration

- Update **ExplorerView** to load real project file trees from .sln/.csproj
- Add **Build/Run buttons** in the toolbar or status bar
- Wire **Problems panel** in BottomPanelView to show build errors/warnings
- Wire **Output panel** to show build output stream

---

## Phase 3 — Editor Intelligence (Roslyn)

**Goal:** Add IntelliSense, diagnostics, and code formatting.

### 3.1 Editor Layer — Monaco Integration

- **Monaco/**
  - `MonacoEditorBridge` — JS interop bridge between MAUI WebView and Monaco
  - Load Monaco editor via embedded HTML/JS in WebView
  - Support for SetContent, GetContent, SetLanguage, SetTheme
  - Forward keyboard events and cursor position changes

### 3.2 Editor Layer — Roslyn Language Server

- **LanguageServer/**
  - `RoslynLanguageService` — host Roslyn workspace in-process
  - Provide completions (IntelliSense) via `CompletionService`
  - Provide signature help via `SignatureHelpService`
  - Provide hover information via `QuickInfoService`

- **Diagnostics/**
  - `DiagnosticService` — real-time error/warning analysis using Roslyn analyzers
  - Push diagnostics to Monaco via JS interop (red/yellow squiggles)
  - Sync diagnostics to the Problems panel

### 3.3 Editor Layer — Refactoring

- **Refactoring/**
  - `RefactoringService` — Roslyn-powered code actions
  - Support: Rename, Extract Method, Add Using, Generate Constructor
  - Surface refactoring suggestions in Monaco's light bulb menu

---

## Phase 4 — ASP.NET, Git & Multi-Project

**Goal:** Support web projects, Git workflows, and large solutions.

### 4.1 Runtime Layer — ASP.NET & Services

- **Run/**
  - Add ASP.NET Core project support (detect web apps by OutputType/Sdk)
  - Port tracking — detect which port Kestrel binds to

- **ServiceManager/**
  - `ServiceManager` — track multiple running services (microservices)
  - Start/stop individual services independently
  - Service health dashboard

- **App Layer**
  - Add embedded WebView for web preview (browse localhost)
  - Port forwarding UI for accessing web apps from external devices

### 4.2 Git Layer

- **GitService/**
  - `GitService : IGitService` — implement using LibGit2Sharp
  - Clone, Init, Commit, Push, Pull, Fetch operations

- **BranchManager/**
  - `BranchManager` — list, create, delete, checkout branches
  - Merge and conflict detection

- **RepositoryManager/**
  - `RepositoryManager` — open/close repos, track working directory state
  - File status tracking (modified, staged, untracked)

- **App Layer**
  - Add **GitPanelView** — show changed files, staging area, commit message input
  - Wire ActivityBar git button to toggle GitPanelView
  - Show current branch in StatusBar

### 4.3 Multi-Project Solutions

- Update SolutionParser to handle large solutions with many projects
- Add dependency graph visualization
- Support building individual projects within a solution
- Project-level context menus in Explorer (Build, Clean, Set as Startup)

---

## Phase 5 — Polish, Performance & Extensibility

**Goal:** Production-ready quality, performance, and plugin support.

### 5.1 Cross-Cutting Concerns

- **Error Handling**
  - Global exception handler in MauiProgram.cs
  - User-friendly error dialogs with "Report Bug" option
  - Crash logging to local file

- **Logging**
  - Structured logging with Serilog or NLog
  - Log viewer in Output panel
  - Log file rotation

- **App State Persistence**
  - Save/restore open tabs, cursor positions, panel sizes
  - Recent projects list
  - User preferences stored via Preferences API or JSON file

- **Theming**
  - Theme toggle (Dark/Light) with dynamic resource switching
  - Populate LightTheme.xaml with full color definitions
  - Custom theme support (user-defined color schemes)

### 5.2 UX Enhancements

- **Command Palette** — Ctrl+Shift+P style quick-action search
- **Keyboard Shortcuts** — configurable keybindings for all major actions
- **Search/Find-in-Files** — grep-like search across project files with result navigation
- **Settings UI** — preferences page for editor, terminal, theme, SDK path configuration
- **Onboarding** — first-run wizard to configure SDK path and create first project
- **Breadcrumb Navigation** — file path breadcrumbs above the editor

### 5.3 Performance Optimization

- Virtualized file tree in ExplorerView for large projects
- Lazy loading of file contents (only load when tab is active)
- Background thread for Roslyn analysis (avoid UI freezes)
- Memory-efficient terminal output buffering (ring buffer)

### 5.4 Plugin System

- Define plugin interface/contract (IPlugin, IPluginContext)
- Plugin manifest format (JSON-based)
- Plugin lifecycle management (load, activate, deactivate)
- Extension points: editor commands, sidebar panels, terminal commands

---

## Execution Order Summary

| Phase | Focus | Estimated Effort |
|-------|-------|-----------------|
| Phase 1 | Foundation, File System, Terminal, Basic UI | 3–4 weeks |
| Phase 2 | Project System, Build/Run Console Apps | 2–3 weeks |
| Phase 3 | Monaco Editor, Roslyn IntelliSense | 3–4 weeks |
| Phase 4 | ASP.NET, Git, Multi-Project Solutions | 3–4 weeks |
| Phase 5 | Polish, Performance, Plugin System | 2–3 weeks |

---

## Dependencies Between Phases

```
Phase 1 (Foundation)
  ├── Phase 2 (Project System + Build)
  │     └── Phase 4 (ASP.NET + Multi-Project)
  ├── Phase 3 (Editor Intelligence)
  └── Phase 5 (Polish + Plugins)
```

Phase 1 must be completed first. Phases 2 and 3 can be worked on in parallel. Phase 4 depends on Phase 2. Phase 5 can begin incrementally alongside any phase.
