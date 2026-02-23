# App Layer — `DroidIDE.App`

The App layer is the **UI layer** of DroidIDE, built with .NET MAUI targeting Android. It contains all views, ViewModels, application services, value converters, and the DI container configuration. This layer implements the MVVM pattern with data binding throughout.

---

## Directory Structure

```
DroidIDE.App/
├── App.xaml / App.xaml.cs                    # Application entry, converter registration, DI page resolution
├── MauiProgram.cs                            # DI container — registers all services + ViewModels
├── ViewModels/
│   ├── BaseViewModel.cs                      # Base class: INPC, SetProperty, RelayCommand, AsyncRelayCommand
│   ├── ExplorerViewModel.cs                  # File tree loading, selection, file-open events
│   ├── EditorViewModel.cs                    # Tab management, open/close/save
│   └── TerminalViewModel.cs                  # Terminal sessions, command I/O, real-time output
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
│   │   └── StatusBarView.xaml / .xaml.cs      # Branch, status, cursor line, language
│   ├── Editor/
│   │   └── MonacoEditorView.xaml / .xaml.cs   # Monaco WebView placeholder (Phase 3)
│   └── Terminal/
│       └── TerminalView.xaml / .xaml.cs       # Standalone terminal placeholder
├── Services/
│   └── EditorService.cs                      # IEditorService impl: file open/save, language detection
├── Converters/
│   └── ValueConverters.cs                    # BoolToIcon, TabColor, TabTextColor, ModifiedDot, IsNull/IsNotNull
└── Resources/
    ├── Images/                               # SVG icons (explorer, search, git, debug)
    ├── Styles/                               # Colors.xaml, Styles.xaml
    └── Themes/                               # DarkTheme.xaml, LightTheme.xaml
```

---

## ViewModel Architecture

```
MainShellViewModel (root)
├── ExplorerViewModel   — bound to ExplorerView
├── EditorViewModel     — bound to EditorTabsView + EditorHostView
└── TerminalViewModel   — bound to BottomPanelView
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
| ViewModels | `MainShellViewModel`, `ExplorerViewModel`, `EditorViewModel`, `TerminalViewModel` |
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

---

## Rules

1. **No direct OS access** — Use `IFileSystemService` and `IProcessManager`, never `System.IO` or `System.Diagnostics.Process`.
2. **No dotnet CLI calls** — Use `IDotnetCli` (coming in Phase 2).
3. **No Roslyn logic** — That belongs in the Editor layer.
4. **All views are ContentView** — Except `MainShell` which is the single `ContentPage`. Child views must never be `ContentPage`.
5. **BindingContext flows from MainShell** — Child views receive their ViewModel from parent bindings, not from DI directly.