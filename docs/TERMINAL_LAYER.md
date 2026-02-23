# Terminal Layer — `DroidIDE.Terminal`

The Terminal layer manages **interactive shell sessions** on Android, providing the terminal panel in the IDE. It handles session lifecycle, output streaming, ANSI escape code processing, and MSBuild output parsing.

---

## Directory Structure

```
DroidIDE.Terminal/
├── ProcessSession/
│   └── ProcessSessionManager.cs   # ITerminalService implementation
├── OutputParser/
│   ├── AnsiParser.cs              # ANSI escape code stripper
│   └── OutputLineParser.cs        # MSBuild diagnostic parser
└── Shell/                         # Planned: PTY-like shell sessions
```

---

## ProcessSessionManager

Implements `ITerminalService` using `IProcessManager` for process execution.

| Method | Description |
|--------|-------------|
| `CreateSessionAsync(workingDir)` | Starts a new shell process, returns `TerminalSession` |
| `SendCommandAsync(sessionId, command)` | Writes input to session's stdin |
| `GetOutputAsync(sessionId)` | Returns accumulated output lines for a session |
| `CloseSessionAsync(sessionId)` | Kills the process and cleans up |
| `OutputReceived` event | Fires on each new output line (for real-time UI streaming) |

**Key behavior:**
- Sessions are tracked in a `ConcurrentDictionary<string, (TerminalSession, IRunningProcess)>`
- Output from `IRunningProcess.OutputReceived` events is captured and forwarded
- Session IDs are generated via `Guid.NewGuid()`
- Disposed sessions are removed from tracking

---

## AnsiParser

Strips ANSI escape codes from terminal output for clean display.

```csharp
// Usage
string clean = AnsiParser.StripEscapeCodes(rawOutput);
```

| Pattern Handled | Example |
|----------------|---------|
| CSI sequences | `\x1B[31m` (colors), `\x1B[1A` (cursor movement) |
| OSC sequences | `\x1B]0;title\x07` (window title) |

**Implementation:** Uses `[GeneratedRegex]` (source-generated) for compile-time optimized pattern matching.

---

## OutputLineParser

Parses MSBuild-format output lines into structured `DiagnosticItem` objects.

```csharp
// Usage
DiagnosticItem? result = OutputLineParser.TryParseDiagnostic(outputLine);
```

**MSBuild format matched:**
```
Program.cs(12,5): error CS1002: ; expected
```

Extracted fields: `FilePath`, `Line`, `Column`, `Severity`, `Code`, `Message`

**Implementation:** Uses `[GeneratedRegex]` matching the pattern `filepath(line,col): severity code: message`.

---

## Dependencies

- `DroidIDE.Core` (for `ITerminalService`, `TerminalSession`, `DiagnosticItem`)
- No MAUI, no NuGet packages