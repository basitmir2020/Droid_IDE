# Development Rules

1. Never mix UI and runtime logic.
2. All OS operations go through Infrastructure.
3. All dotnet CLI calls go through Runtime layer.
4. Editor must use Roslyn, not custom parsing.
5. Every feature must respect modular boundaries.
6. No circular dependencies.
7. Keep Android-only optimizations isolated.