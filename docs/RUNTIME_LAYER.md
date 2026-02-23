# Runtime Layer — `DroidIDE.Runtime`

> **Status:** 🔲 Planned for Phase 2

The Runtime layer will wrap all `dotnet` CLI commands, providing build/run/test execution with real-time output streaming.

---

## Planned Structure

```
DroidIDE.Runtime/
├── DotnetManager/
│   └── DotnetCliService.cs    # IDotnetCli implementation
├── Build/
│   └── BuildService.cs        # Build execution + MSBuild output parsing
├── Run/
│   └── RunService.cs          # dotnet run for console + web apps
└── ServiceManager/
    └── ServiceManager.cs      # Microservice process tracking
```

---

## Planned Features

### DotnetCliService (`IDotnetCli`)
Wraps the dotnet CLI via `IProcessManager`:

| Method | CLI Command | Description |
|--------|------------|-------------|
| `NewAsync(template, name, output)` | `dotnet new` | Create new project |
| `RestoreAsync(projectPath)` | `dotnet restore` | Restore NuGet packages |
| `BuildAsync(projectPath)` | `dotnet build` | Compile project |
| `RunAsync(projectPath)` | `dotnet run` | Execute project |
| `TestAsync(projectPath)` | `dotnet test` | Run unit tests |
| `CleanAsync(projectPath)` | `dotnet clean` | Clean build artifacts |
| `AddPackageAsync(project, packageId)` | `dotnet add package` | Add NuGet package |

All methods support `Action<string> onOutput` for real-time streaming.

### BuildService
- Execute `dotnet build` with structured output parsing
- Extract errors/warnings from MSBuild output (using Terminal's `OutputLineParser`)
- Track build status (`Idle` → `Building` → `Success`/`Failed`)
- Return `BuildResult` with duration and diagnostic counts

### RunService
- Execute `dotnet run` with process lifecycle management
- Support start/stop/restart of running application
- Port detection for ASP.NET Core apps (Phase 4)
- Output streaming to terminal panel

### ServiceManager (Phase 4)
- Track multiple independently running services
- Start/stop individual microservices
- Health monitoring dashboard

---

## Dependencies (Planned)

- `DroidIDE.Core` (for `IDotnetCli`, `BuildResult`, `BuildStatus`)
- `DroidIDE.Infrastructure` (for `IProcessManager`)