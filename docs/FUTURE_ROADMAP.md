# Future Roadmap

Development is organized into 5 phases. Each phase builds on the previous one.

---

## Phase 1 — Foundation & UI Shell ✅

> **Status: Complete**

- Core domain models, interfaces, enums, constants
- Infrastructure services (file system, process, Linux, security)
- Terminal session management + ANSI/MSBuild parsing
- All ViewModels with full MVVM data binding
- Functional UI: Explorer, Editor tabs, Editor host, Terminal panel, Activity bar, Status bar
- DI registration and app startup pipeline

---

## Phase 2 — Project System & Build/Run 🔲

> **Status: Next up**

- **ProjectSystem:** Parse `.sln` and `.csproj` files into `SolutionInfo`/`ProjectInfo` models
- **Runtime:** `DotnetCliService` wrapping all dotnet CLI commands
- **Build:** Execute `dotnet build` with structured output, error/warning extraction
- **Run:** Execute `dotnet run` for console apps with process lifecycle
- **NuGet:** Package restore, list, add/remove
- **App Integration:** Project tree in Explorer, Build/Run toolbar buttons, Problems panel

---

## Phase 3 — Editor Intelligence (Roslyn) 🔲

- **Monaco:** WebView-hosted Monaco editor with JS interop bridge
- **Roslyn IntelliSense:** Completions, signature help, hover info
- **Diagnostics:** Real-time error squiggles synced to Problems panel
- **Refactoring:** Rename, Extract Method, Add Using, Generate Constructor

---

## Phase 4 — ASP.NET, Git & Multi-Project 🔲

- **ASP.NET Core:** Web app support with port detection and browser preview
- **Microservices:** Multi-service manager with independent start/stop
- **Git:** Full LibGit2Sharp integration (clone, commit, push, pull, branches)
- **Git UI:** Changed files panel, staging area, commit message, branch indicator
- **Multi-project:** Large solution support, dependency graph, per-project build

---

## Phase 5 — Polish, Performance & Extensibility 🔲

- **Error Handling:** Global exception handler, crash logging, user-friendly dialogs
- **Logging:** Structured logging with Serilog + Output panel log viewer
- **State Persistence:** Save/restore tabs, panel sizes, cursor positions, recent projects
- **Theming:** Dark/Light toggle with dynamic resource switching
- **Command Palette:** Quick-action search (Ctrl+Shift+P style)
- **Keyboard Shortcuts:** Configurable keybindings
- **Search:** Find-in-files with result navigation
- **Settings UI:** Preferences for editor, terminal, theme, SDK path
- **Onboarding:** First-run wizard for SDK setup and project creation
- **Plugin System:** IPlugin interface, manifest format, lifecycle management

---

## Phase Dependencies

```
Phase 1 (Foundation) ✅
  ├── Phase 2 (Project System + Build)
  │     └── Phase 4 (ASP.NET + Multi-Project)
  ├── Phase 3 (Editor Intelligence)
  └── Phase 5 (Polish + Plugins)
```

Phase 1 is complete. Phases 2 and 3 can proceed in parallel. Phase 4 depends on Phase 2. Phase 5 can begin incrementally alongside any phase.