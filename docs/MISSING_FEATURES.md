# Missing Features

This document tracks features that are **not yet implemented**. Items marked ~~strikethrough~~ were previously missing but have since been completed.

> **Last updated:** Phase 1 complete

---

## ~~Core Layer~~ ✅
~~All models, interfaces, enums, and constants are implemented.~~

## ~~Infrastructure Layer~~ ✅
~~FileSystemService, ProcessManager, LinuxEnvironmentManager, and CredentialStore are implemented.~~

## ~~Terminal Layer~~ ✅
~~ProcessSessionManager, AnsiParser, and OutputLineParser are implemented.~~

## ~~App Layer — Foundation~~ ✅
~~DI registration, ViewModels, EditorService, value converters, and data-bound views are implemented.~~

---

## Editor Layer (Phase 3)
- No Monaco WebView integration (Monaco folder is empty)
- No Roslyn language server integration (LanguageServer folder is empty)
- No diagnostics/error reporting (Diagnostics folder is empty)
- No refactoring support (Refactoring folder is empty)
- EditorHostView uses native MAUI Editor control instead of Monaco WebView

## Runtime Layer (Phase 2)
- No dotnet CLI wrapper (DotnetManager folder is empty)
- No build command execution (Build folder is empty)
- No run/debug command execution (Run folder is empty)
- No microservice process tracking (ServiceManager folder is empty)

## Project System Layer (Phase 2)
- No .sln file parser (SolutionParser folder is empty)
- No .csproj file parser (ProjectParser folder is empty)
- No NuGet package manager (NuGetManager folder is empty)

## Git Layer (Phase 4)
- No GitService implementation (GitService folder is empty)
- No branch management (BranchManager folder is empty)
- No repository management (RepositoryManager folder is empty)

## App Layer — Advanced Features
- No command palette
- No settings/preferences UI
- No keyboard shortcut handling
- No search/find-in-files functionality
- No file watching for live reload
- No project/solution-aware Explorer (currently shows flat folder tree)

## Cross-Cutting Concerns
- No error handling or global exception handler
- No logging infrastructure beyond debug logger
- No app state persistence (open tabs, panel sizes, recent projects)
- No theming toggle (only dark theme is active, LightTheme.xaml exists but unused)
- No onboarding or first-run experience
