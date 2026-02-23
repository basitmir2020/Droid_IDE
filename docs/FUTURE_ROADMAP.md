# Future Roadmap

Development was organized into 5 phases. All phases are now complete.

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

## Phase 2 — Project System & Build/Run ✅

> **Status: Complete**

- **ProjectSystem:** Parse `.sln` and `.csproj` files into `SolutionInfo`/`ProjectInfo` models
- **Runtime:** `DotnetCliService` wrapping all dotnet CLI commands
- **Build:** Execute `dotnet build` with structured output, error/warning extraction
- **Run:** Execute `dotnet run` for console apps with process lifecycle
- **NuGet:** Package restore, list, add/remove
- **App Integration:** Project tree in Explorer, Build/Run toolbar buttons, Problems panel

---

## Phase 3 — Editor Intelligence (Roslyn) ✅

> **Status: Complete**

- **Monaco:** WebView-hosted Monaco editor with JS interop bridge
- **Roslyn IntelliSense:** Completions, signature help, hover info
- **Diagnostics:** Real-time error squiggles synced to Problems panel
- **Refactoring:** Rename, Extract Method, Add Using, Generate Constructor

---

## Phase 4 — ASP.NET, Git & Multi-Project ✅

> **Status: Complete**

- **ASP.NET Core:** Web app support with port detection and browser preview
- **Microservices:** Multi-service manager with independent start/stop
- **Git:** Full LibGit2Sharp integration (clone, commit, push, pull, branches)
- **Git UI:** GitPanelView with changed files, staging area, commit message, branch indicator
- **GitViewModel:** Full MVVM data binding for all Git operations
- **Multi-project:** Large solution support, dependency graph, per-project build

---

## Phase 5 — Polish, Performance & Extensibility ✅

> **Status: Complete**

- **Error Handling:** GlobalExceptionHandler with AppDomain/TaskScheduler hooks, user-friendly dialogs
- **Logging:** AppLogger with file-based structured logging, 5MB rotation, severity levels
- **State Persistence:** AppStateService for save/restore tabs, panel sizes, recent projects
- **Theming:** ThemeService with runtime Dark↔Light toggle via ResourceDictionary swapping
- **Command Palette:** CommandPaletteView with fuzzy search and shortcut display
- **Keyboard Shortcuts:** KeyboardShortcutService for configurable keybindings
- **Search:** SearchPanelView with find-in-files, case toggle, file filters
- **Settings UI:** SettingsView with appearance, editor, and auto-save preferences
- **Onboarding:** WelcomeView with quick actions, keyboard shortcuts reference, "don't show again"
- **UI Modernization:** All `Frame` elements replaced with `Border` for .NET 10+ compatibility

---

## Phase Dependencies

```
Phase 1 (Foundation) ✅
  ├── Phase 2 (Project System + Build) ✅
  │     └── Phase 4 (ASP.NET + Multi-Project) ✅
  ├── Phase 3 (Editor Intelligence) ✅
  └── Phase 5 (Polish + Plugins) ✅
```

All phases are complete. The solution builds with **0 errors and 0 warnings**.