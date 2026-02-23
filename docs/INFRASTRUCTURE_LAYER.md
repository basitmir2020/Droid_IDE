# Infrastructure Layer — `DroidIDE.Infrastructure`

The Infrastructure layer handles all **OS-level operations** — file system access, process execution, Linux environment configuration, and credential storage. It implements the interfaces defined in Core.

---

## Directory Structure

```
DroidIDE.Infrastructure/
├── FileSystem/
│   └── FileSystemService.cs     # IFileSystemService implementation
├── Process/
│   └── ProcessManager.cs        # IProcessManager implementation
├── Linux/
│   └── LinuxEnvironmentManager.cs   # SDK path and env variable setup
└── Security/
    └── CredentialStore.cs       # JSON-based credential persistence
```

---

## FileSystemService

Implements `IFileSystemService` using `System.IO`.

| Method | Description |
|--------|-------------|
| `ReadFileAsync(path)` | Reads file content as string |
| `WriteFileAsync(path, content)` | Writes content, auto-creates directories |
| `FileExists(path)` | Checks file existence |
| `DirectoryExists(path)` | Checks directory existence |
| `GetFileTreeAsync(rootPath, maxDepth)` | Recursively builds `FileItem` tree (default depth: 5) |
| `GetDirectoryContentsAsync(path)` | Lists sorted contents (directories first, then files) |
| `CreateDirectoryAsync(path)` | Creates directory |
| `DeleteFileAsync(path)` | Deletes file |
| `DeleteDirectoryAsync(path, recursive)` | Deletes directory |

**Key behavior:** File tree is depth-limited to avoid scanning massive `node_modules`-style trees. Results are sorted alphabetically with directories listed first.

---

## ProcessManager

Implements `IProcessManager` using `System.Diagnostics.Process`.

**Two execution modes:**

| Mode | Method | Use Case |
|------|--------|----------|
| **Run-to-completion** | `RunAsync(command, args, cwd)` | Short commands: `dotnet build`, `git status` |
| **Streaming (long-running)** | `StartAsync(command, args, cwd)` | Interactive: `dotnet run`, shell sessions |

The `IRunningProcess` interface (returned by `StartAsync`) provides:
- `OutputReceived` / `ErrorReceived` events for real-time streaming
- `WriteInputAsync(text)` for stdin
- `Kill()` for termination
- `WaitForExitAsync()` for waiting

---

## LinuxEnvironmentManager

Configures the Linux environment for running .NET SDK on Android.

| Method | Description |
|--------|-------------|
| `ConfigureEnvironment()` | Sets `DOTNET_ROOT`, `HOME`, updates `PATH` |
| `IsSdkAvailable()` | Checks if `dotnet` executable exists at configured path |
| `GetSdkVersion()` | Runs `dotnet --version` and returns the result |

**Environment variables set:**
- `DOTNET_ROOT` → `/data/data/{app}/files/dotnet`
- `HOME` → `/data/data/{app}/files/home`
- `PATH` → prepends SDK directory

---

## CredentialStore

JSON-based credential persistence for Git tokens and other secrets.

| Method | Description |
|--------|-------------|
| `StoreAsync(key, value)` | Saves a credential |
| `GetAsync(key)` | Retrieves a credential (null if not found) |
| `RemoveAsync(key)` | Removes a credential |
| `RemoveAllAsync()` | Clears all credentials |

**Storage:** `{LocalApplicationData}/droidide_credentials.json`

> **Note:** On Android, the app's internal storage directory is sandboxed per-app, providing OS-level isolation. The Infrastructure project targets `net10.0` (not a platform-specific TFM), so MAUI's `SecureStorage` API is unavailable — file-based storage in the sandboxed directory is the practical alternative.

---

## Dependencies

- `DroidIDE.Core` (for interfaces and models)
- `System.IO`, `System.Diagnostics.Process`, `System.Text.Json` (BCL only)
- **No MAUI, no NuGet packages**