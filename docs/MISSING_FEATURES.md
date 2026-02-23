# Missing Features

This document tracks features that are **not yet implemented**. Items marked ~~strikethrough~~ were previously missing but have since been completed.

> **Last updated:** Phase 2 complete

---

## ~~Core Layer~~ ✅
~~All models, interfaces, enums, and constants are implemented.~~

## ~~Infrastructure Layer~~ ✅
~~FileSystemService, ProcessManager, LinuxEnvironmentManager, and CredentialStore are implemented.~~

## ~~Terminal Layer~~ ✅
~~ProcessSessionManager, AnsiParser, and OutputLineParser are implemented.~~

## ~~App Layer — Foundation~~ ✅
~~DI registration, ViewModels, EditorService, value converters, and data-bound views are implemented.~~

## ~~Project System Layer~~ ✅
~~SolutionParser (.sln/.slnx), ProjectParser (.csproj), and NuGetManager are implemented.~~

## ~~Runtime Layer~~ ✅
~~DotnetCliService (IDotnetCli), BuildService, RunService, and ServiceManager are implemented.~~

---

## Editor Layer (Phase 3)
- No Monaco WebView integration (Monaco folder is empty)
- No Roslyn language server integration (LanguageServer folder is empty)
- No diagnostics/error reporting (Diagnostics folder is empty)
- No refactoring support (Refactoring folder is empty)
- EditorHostView uses native MAUI Editor control instead of Monaco WebView

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
