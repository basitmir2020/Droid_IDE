# DroidIDE Documentation

DroidIDE is a **tablet-first Android IDE** built with **.NET 10** and **.NET MAUI**, designed to provide a VS Code–like experience with Rider-level .NET functionality — running entirely locally on Android.

---

## Project Goals

- **VS Code–like UI** — activity bar, explorer sidebar, tabbed editor, terminal, status bar
- **Rider-level .NET functionality** — IntelliSense, diagnostics, refactoring via Roslyn
- **Fully local execution** — .NET SDK runs directly on Android via Linux compatibility layer
- **Console, ASP.NET Core, and microservice support** — build, run, and debug any .NET project
- **Android tablet optimized** — responsive layout with resizable panels and touch-friendly controls

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| UI Framework | .NET MAUI (Android) |
| Target Framework | `net10.0-android` |
| Language | C# 13 |
| Code Editor | Monaco Editor (via WebView) |
| Language Services | Roslyn (Microsoft.CodeAnalysis) |
| Git | LibGit2Sharp |
| Architecture | Clean Architecture (MVVM) |

---

## Solution Structure

```
DroidIDE/
├── DroidIDE.App/              # UI Layer — MAUI views, ViewModels, services, DI
├── DroidIDE.Core/             # Domain Layer — models, interfaces, enums, constants
├── DroidIDE.Infrastructure/   # OS Layer — file system, process, Linux environment
├── DroidIDE.Editor/           # Editor Layer — Monaco + Roslyn language services
├── DroidIDE.Terminal/         # Terminal Layer — shell sessions, output parsing
├── DroidIDE.Runtime/          # Runtime Layer — dotnet CLI wrapper, build/run services
├── DroidIDE.ProjectSystem/    # Project Layer — .sln/.csproj parsing, NuGet management
├── DroidIDE.Git/              # Git Layer — LibGit2Sharp integration
└── docs/                      # This documentation
```

---

## Documentation Index

| Document | Description |
|----------|------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture, dependency diagram, design principles |
| [LAYERS.md](LAYERS.md) | Overview of all 8 projects and their responsibilities |
| [CORE_LAYER.md](CORE_LAYER.md) | Domain models, interfaces, enums, constants |
| [APP_LAYER.md](APP_LAYER.md) | UI views, ViewModels, DI, converters, services |
| [INFRASTRUCTURE_LAYER.md](INFRASTRUCTURE_LAYER.md) | File system, process, Linux, security services |
| [TERMINAL_LAYER.md](TERMINAL_LAYER.md) | Shell session management and output parsing |
| [EDITOR_LAYER.md](EDITOR_LAYER.md) | Monaco + Roslyn integration |
| [RUNTIME_LAYER.md](RUNTIME_LAYER.md) | dotnet CLI wrapper, build/run services |
| [PROJECT_SYSTEM_LAYER.md](PROJECT_SYSTEM_LAYER.md) | .sln/.csproj parsing, NuGet management |
| [GIT_LAYER.md](GIT_LAYER.md) | Git operations via LibGit2Sharp |
| [DEVELOPMENT_RULES.md](DEVELOPMENT_RULES.md) | Coding standards and architectural rules |
| [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) | 5-phase implementation roadmap |
| [FUTURE_ROADMAP.md](FUTURE_ROADMAP.md) | Feature roadmap across all phases |
| [MISSING_FEATURES.md](MISSING_FEATURES.md) | Remaining work tracker |

---

## Current Status

**All Phases Implemented ✅ | Build: 0 Errors, 0 Warnings**

| Phase | Focus | Status |
|-------|-------|--------|
| Phase 1 | Foundation & UI Shell | ✅ Complete |
| Phase 2 | Project System & Build/Run | ✅ Complete |
| Phase 3 | Editor Intelligence (Roslyn) | ✅ Complete |
| Phase 4 | ASP.NET, Git & Multi-Project | ✅ Complete |
| Phase 5 | Polish, Performance & Extensibility | ✅ Complete |

### Implemented Features

- Core domain models, interfaces, enums, and constants
- Infrastructure services (file system, process, credentials, Linux environment)
- Terminal session management with ANSI parsing and MSBuild output parsing
- Project system — solution/project parsing, NuGet management
- Runtime — dotnet CLI wrapper, build/run services
- Editor — Monaco bridge, Roslyn language services, diagnostics, refactoring
- Git — clone, commit, push, pull, branch management, repository tracking
- All ViewModels with MVVM data binding (7 ViewModels)
- Complete UI with 12 views and data binding
- Cross-cutting: global exception handler, file-based logging, theme toggle, settings, onboarding
- Application services: search, command palette, keyboard shortcuts, file watcher, app state persistence
- Full DI registration in `MauiProgram.cs`