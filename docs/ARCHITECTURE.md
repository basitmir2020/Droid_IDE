# System Architecture

DroidIDE follows a **modular clean architecture** with strict separation of concerns. Each project has a single responsibility and dependencies flow inward toward the Core domain layer.

---

## Dependency Diagram

```
┌─────────────────────────────────────────────────┐
│                DroidIDE.App                     │
│           (UI Layer — .NET MAUI)                │
│  Views ← ViewModels ← Services ← DI Container  │
└──────────┬────────┬────────┬────────┬───────────┘
           │        │        │        │
     ┌─────▼──┐ ┌───▼───┐ ┌─▼────┐ ┌─▼──────────┐
     │Terminal │ │Editor │ │ Git  │ │ProjectSystem│
     │ Layer  │ │ Layer │ │Layer │ │   Layer     │
     └───┬────┘ └───┬───┘ └──┬───┘ └─────┬──────┘
         │          │        │            │
    ┌────▼──────────▼────────▼────────────▼──────┐
    │          DroidIDE.Infrastructure            │
    │    (File System, Process, Linux, Security)  │
    └─────────────────┬──────────────────────────┘
                      │
              ┌───────▼───────┐
              │ DroidIDE.Core │
              │   (Domain)    │
              │ Models, Enums │
              │  Interfaces   │
              │  Constants    │
              └───────────────┘
```

---

## Layer Rules

| Rule | Description |
|------|-------------|
| **Core is independent** | Core depends on nothing. All other layers depend on Core. |
| **UI never touches OS** | App layer accesses file system, processes, and CLI only through injected interfaces. |
| **Dependencies flow inward** | Feature layers depend on Infrastructure and Core, never on App. |
| **No circular references** | Each project reference is strictly one-directional. |
| **Interfaces live in Core** | All service contracts (`IFileSystemService`, `IProcessManager`, etc.) are defined in Core and implemented in other layers. |

---

## Design Patterns

| Pattern | Usage |
|---------|-------|
| **MVVM** | All UI views have matching ViewModels with data binding via `INotifyPropertyChanged`. |
| **Dependency Injection** | All services registered in `MauiProgram.cs` and injected via constructors. |
| **Repository Pattern** | `IFileSystemService` abstracts file I/O from the domain. |
| **Command Pattern** | `RelayCommand` / `AsyncRelayCommand` for UI actions. |
| **Observer Pattern** | `ITerminalService.OutputReceived` event for real-time terminal streaming. |
| **Strategy Pattern** | `EditorService.DetectLanguage()` maps file extensions to Monaco language IDs. |
| **Singleton Pattern** | Infrastructure services, ViewModels, and cross-cutting services are singleton-scoped. |
| **Factory Pattern** | `ThemeService` factories `LightTheme`/`DarkTheme` `ResourceDictionary` instances at runtime. |

---

## Key Design Decisions

1. **MAUI ContentView, not ContentPage** — All child views are `ContentView` subclasses to enable nesting within the single `MainShell` ContentPage layout.
2. **Singleton services** — Infrastructure services and ViewModels are singletons since the IDE has a single long-lived session.
3. **File-based credential storage** — Infrastructure targets `net10.0` (not platform-specific), so MAUI SecureStorage is unavailable. Credentials use JSON file storage in the sandboxed app directory instead.
4. **Source-generated Regex** — Terminal parsers use `[GeneratedRegex]` for compile-time optimized pattern matching of ANSI codes and MSBuild output.
5. **Sub-ViewModel distribution** — `MainShellViewModel` owns sub-ViewModels (`Explorer`, `Editor`, `Terminal`) and `MainShell.xaml` distributes them to child views via `BindingContext` bindings.
6. **Runtime theme switching** — `ThemeService` swaps merged `ResourceDictionary` instances at runtime, enabling live Dark↔Light toggling without restarting the app.
7. **Global exception safety** — `GlobalExceptionHandler` catches `AppDomain` and `TaskScheduler` unhandled exceptions, logs them via `AppLogger`, and shows user-friendly dialogs.
8. **File-based logging with rotation** — `AppLogger` provides structured file-based logging with 5MB rotation, severity levels, and thread-safe buffered writes.
9. **Border over Frame** — All UI cards use `Border` with `StrokeShape` instead of the deprecated `Frame` for forward compatibility with .NET 10+.