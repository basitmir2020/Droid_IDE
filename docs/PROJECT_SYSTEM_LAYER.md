# Project System Layer — `DroidIDE.ProjectSystem`

> **Status:** 🔲 Planned for Phase 2

The ProjectSystem layer handles parsing of .NET solution and project files, enabling the IDE to understand project structure, dependencies, and build configuration.

---

## Planned Structure

```
DroidIDE.ProjectSystem/
├── SolutionParser/
│   └── SolutionParser.cs      # ISolutionParser implementation
├── ProjectParser/
│   └── ProjectParser.cs       # IProjectParser implementation
└── NuGetManager/
    └── NuGetManager.cs        # NuGet package management
```

---

## Planned Features

### SolutionParser (`ISolutionParser`)
- Parse `.sln` files to extract project list
- Support for `.slnx` (XML-based solution format)
- Detect startup project
- Return `SolutionInfo` model

### ProjectParser (`IProjectParser`)
- Parse `.csproj` XML files
- Extract: `TargetFramework`, `OutputType`, `RootNamespace`, `AssemblyName`
- Extract `PackageReference` list with versions
- Extract `ProjectReference` list for dependency graph
- Detect project type: Console, Web, Library, Test
- Return `ProjectInfo` model

### NuGetManager
- Run `dotnet restore` via `IDotnetCli`
- List installed packages and versions
- Check for package updates
- Add/remove packages via `dotnet add/remove package`

---

## Dependencies (Planned)

- `DroidIDE.Core` (for `IProjectParser`, `ISolutionParser`, `ProjectInfo`, `SolutionInfo`)
- System.Xml.Linq (BCL) for .csproj XML parsing