# App Layer — `DroidIDE.App`

The App layer is the **UI layer** of DroidIDE, built with .NET MAUI targeting Android. It contains all views, ViewModels, application services, value converters, and the DI container configuration. This layer implements the MVVM pattern with data binding throughout.

---

## Directory Structure

```
DroidIDE.App/
├── App.xaml / App.xaml.cs                    # Application entry, converter registration, DI, theme & exception wiring
├── MauiProgram.cs                            # DI container — registers all services + ViewModels
├── ViewModels/
│   ├── BaseViewModel.cs                      # Base class: INPC, SetProperty, RelayCommand, AsyncRelayCommand
│   ├── ExplorerViewModel.cs                  # File tree loading, selection, file-open events
│   ├── EditorViewModel.cs                    # Tab management, open/close/save
│   ├── TerminalViewModel.cs                  # Terminal sessions, command I/O, real-time output
│   ├── SearchViewModel.cs                    # Find-in-files search, result grouping
│   ├── GitViewModel.cs                       # Git operations, file status, staging, commit
│   └── SettingsViewModel.cs                  # App preferences, editor config, theme selection
├── Views/
│   ├── Shell/
│   │   ├── MainShell.xaml / .xaml.cs         # Root page: grid layout distributing sub-ViewModels
│   │   └── MainShellViewModel.cs             # Panel visibility, status text, orchestration
│   ├── Layout/
│   │   ├── ActivityBarView.xaml / .xaml.cs    # Sidebar icon buttons (Explorer/Search/Git/Debug)
│   │   ├── ExplorerView.xaml / .xaml.cs       # File tree CollectionView + empty state + loading
│   │   ├── EditorTabsView.xaml / .xaml.cs     # Horizontal scrolling tab strip
│   │   ├── EditorHostView.xaml / .xaml.cs     # Code editor area (monospace Editor control)
│   │   ├── BottomPanelView.xaml / .xaml.cs    # Terminal output + command input
│   │   ├── StatusBarView.xaml / .xaml.cs      # Branch, status, cursor line, language
│   │   ├── SearchPanelView.xaml / .xaml.cs    # Find-in-files panel with filters + results
│   │   ├── GitPanelView.xaml / .xaml.cs       # Git changes, staging, commit, branch info
│   │   ├── CommandPaletteView.xaml / .xaml.cs # Quick-action overlay (Ctrl+Shift+P)
│   │   ├── SettingsView.xaml / .xaml.cs       # Preferences UI (appearance, editor, auto-save)
│   │   └── WelcomeView.xaml / .xaml.cs        # Onboarding: quick actions, shortcuts, "don't show again"
│   ├── Editor/
│   │   └── MonacoEditorView.xaml / .xaml.cs   # Monaco WebView placeholder
│   └── Terminal/
│       └── TerminalView.xaml / .xaml.cs       # Standalone terminal placeholder
├── Services/
│   ├── EditorService.cs                      # IEditorService: file open/save, language detection
│   ├── SettingsService.cs                    # JSON-based preferences persistence
│   ├── SearchService.cs                      # File search across project with regex/literal
│   ├── CommandPaletteService.cs              # Command registry and fuzzy filtering
│   ├── KeyboardShortcutService.cs            # Keyboard shortcut binding and dispatch
│   ├── FileWatcherService.cs                 # File system change notifications
│   ├── AppStateService.cs                    # App state save/restore (tabs, panels, recent)
│   ├── ThemeService.cs                       # Runtime Dark↔Light switching via ResourceDictionary
│   ├── AppLogger.cs                          # File-based logging with rotation and severity levels
│   └── GlobalExceptionHandler.cs             # Unhandled exception catch + user-friendly dialogs
├── Converters/
│   └── ValueConverters.cs                    # BoolToIcon, TabColor, TabTextColor, ModifiedDot, IsNull/IsNotNull, InverseBool
└── Resources/
    ├── Images/                               # SVG icons (explorer, search, git, debug)
    ├── Styles/                               # Colors.xaml, Styles.xaml
    └── Themes/                               # DarkTheme.xaml/cs, LightTheme.xaml/cs
```

---

## ViewModel Architecture

```
MainShellViewModel (root)
├── ExplorerViewModel   — bound to ExplorerView
├── EditorViewModel     — bound to EditorTabsView + EditorHostView
├── TerminalViewModel   — bound to BottomPanelView
├── SearchViewModel     — bound to SearchPanelView
├── GitViewModel        — bound to GitPanelView
└── SettingsViewModel   — bound to SettingsView
```

`MainShellViewModel` owns all sub-ViewModels and wires the **Explorer → Editor** file-open pipeline:
- When a file is selected in Explorer, `FileOpenRequested` fires
- `MainShellViewModel` catches it and calls `EditorViewModel.OpenTab(tab)`

---

## DI Registration (`MauiProgram.cs`)

| Registration | Interface → Implementation |
|-------------|---------------------------|
| Infrastructure | `IFileSystemService` → `FileSystemService` |
| Infrastructure | `IProcessManager` → `ProcessManager` |
| Terminal | `ITerminalService` → `ProcessSessionManager` |
| App | `IEditorService` → `EditorService` |
| App | `SettingsService` (singleton) |
| App | `SearchService` (singleton) |
| App | `CommandPaletteService` (singleton) |
| App | `KeyboardShortcutService` (singleton) |
| App | `FileWatcherService` (singleton) |
| App | `AppStateService` (singleton) |
| App | `ThemeService` (singleton) |
| App | `AppLogger` (singleton) |
| App | `GlobalExceptionHandler` (singleton) |
| ViewModels | `MainShellViewModel`, `ExplorerViewModel`, `EditorViewModel`, `TerminalViewModel`, `SearchViewModel`, `GitViewModel`, `SettingsViewModel` |
| Views | `MainShell` |

All registrations are **singletons** (IDE has one long-lived session).

---

## Value Converters

| Converter | Usage |
|-----------|-------|
| `BoolToIconConverter` | `IsDirectory` → 📁/📄 icon |
| `BoolToTabColorConverter` | `IsActive` → `#1E1E1E` / `#2D2D30` |
| `BoolToTabTextColorConverter` | `IsActive` → white / gray text |
| `BoolToModifiedDotConverter` | `IsModified` → " ●" / "" |
| `IsNotNullConverter` | Object → bool (not null) |
| `IsNullConverter` | Object → bool (is null) |
| `InverseBoolConverter` | Inverts boolean values for inverse visibility bindings |

---

## Application Services

| Service | Description |
|---------|-------------|
| `EditorService` | File open/save, language detection by extension |
| `SettingsService` | JSON-based preferences persistence (`LocalApplicationData`) |
| `SearchService` | Recursive file search with regex/literal, case toggle, file filters |
| `CommandPaletteService` | Command registration, fuzzy name filtering, shortcut display |
| `KeyboardShortcutService` | Keyboard shortcut key bindings and command dispatch |
| `FileWatcherService` | Watches project directories for external file changes |
| `AppStateService` | Save/restore open tabs, panel sizes, recent projects |
| `ThemeService` | Runtime Dark↔Light switching via merged ResourceDictionary swap |
| `AppLogger` | File-based logger with 5MB rotation, severity levels, thread-safe writes |
| `GlobalExceptionHandler` | Catches `AppDomain`/`TaskScheduler` unhandled exceptions, shows error dialogs |

---

## Rules

1. **No direct OS access** — Use `IFileSystemService` and `IProcessManager`, never `System.IO` or `System.Diagnostics.Process`.
2. **No dotnet CLI calls** — Use `IDotnetCli` from the Runtime layer.
3. **No Roslyn logic** — That belongs in the Editor layer.
4. **All views are ContentView** — Except `MainShell` which is the single `ContentPage`. Child views must never be `ContentPage`.
5. **BindingContext flows from MainShell** — Child views receive their ViewModel from parent bindings, not from DI directly.
6. **Use Border, not Frame** — All card-style containers use `Border` with `StrokeShape` for forward compatibility.