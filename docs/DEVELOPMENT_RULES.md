# Development Rules

These rules ensure consistency, maintainability, and architectural integrity across all DroidIDE development.

---

## Architecture Rules

1. **Never mix UI and runtime logic.** Views and ViewModels belong in the App layer. Process execution, CLI commands, and file I/O belong in Infrastructure, Terminal, or Runtime layers.

2. **All OS operations go through Infrastructure.** File system access (`System.IO`) and process execution (`System.Diagnostics.Process`) must only appear in `DroidIDE.Infrastructure`. Other layers consume these via `IFileSystemService` and `IProcessManager`.

3. **All dotnet CLI calls go through the Runtime layer.** No layer except Runtime should invoke `dotnet build`, `dotnet run`, etc. directly. Use `IDotnetCli` via DI.

4. **Editor must use Roslyn, not custom parsing.** IntelliSense, diagnostics, and refactoring must be powered by `Microsoft.CodeAnalysis`, not ad-hoc regex or string parsing.

5. **Every feature must respect modular boundaries.** Each project has a single responsibility. Cross-cutting features should be composed through dependency injection.

6. **No circular dependencies.** Project references must be strictly one-directional: `App → Feature Layers → Infrastructure → Core`.

7. **Keep Android-only optimizations isolated.** Platform-specific code should be in `Platforms/Android/` within the App project, not mixed into shared code.

---

## MVVM Rules

8. **ViewModels never reference Views.** ViewModels expose data and commands. Views bind to them. Communication flows through data binding and events, never direct references.

9. **All UI state lives in ViewModels.** Panel visibility, selected items, input text — all state must be in ViewModel properties, not code-behind. Code-behind is limited to gesture handling and layout calculations.

10. **Use `BaseViewModel.SetProperty()`** for all property setters to ensure `PropertyChanged` is raised correctly.

11. **Child views receive BindingContext from parent XAML.** Views do not resolve ViewModels from DI directly. `MainShell.xaml` distributes sub-ViewModels via `BindingContext="{Binding PropertyName}"`.

---

## View Rules

12. **All child views must be `ContentView`, not `ContentPage`.** Only `MainShell` is a `ContentPage`. Nested `ContentPage` instances cause MAUI runtime crashes.

13. **Use compiled bindings.** All XAML views should specify `x:DataType` for compile-time binding validation.

14. **No hardcoded content in views.** All dynamic content must come from ViewModel bindings. Hardcoded placeholder text is only acceptable in `EmptyView` templates.

15. **Use `Border`, not `Frame`.** All card-style containers must use `Border` with `StrokeShape` and `Stroke` properties. `Frame` is deprecated in .NET 10+.

---

## Code Style

15. **Use `[GeneratedRegex]` for patterns.** Source-generated regex provides compile-time optimization (used in `AnsiParser`, `OutputLineParser`).

16. **Use async/await throughout.** All I/O-bound operations must be asynchronous. UI thread blocking is unacceptable.

17. **Use `ConcurrentDictionary` for shared state.** Multi-session tracking (terminals, processes) must use thread-safe collections.

18. **XML documentation on all public APIs.** All public classes, methods, and interfaces must have `///` XML doc comments.

---

## Cross-Cutting Rules

19. **All exceptions must be caught.** `GlobalExceptionHandler` is wired at startup. Unhandled exceptions are logged and shown to the user, never silently swallowed.

20. **Use `AppLogger` for diagnostic output.** Never use `Console.WriteLine` or `Debug.WriteLine` for persistent logging. Use `AppLogger.LogInfo/Warn/Error/Debug` instead.

21. **Theme resources via `ThemeService`.** Never hardcode theme changes. Use `ThemeService.SetTheme()` or `ThemeService.ToggleTheme()` to switch themes at runtime.