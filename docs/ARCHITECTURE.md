# System Architecture

DroidIDE follows a modular clean architecture.

App (UI Layer)
↓
Feature Layers
↓
Core (Domain)

## Principles

1. UI must not contain business logic.
2. Core must not depend on any other project.
3. Infrastructure handles OS interactions.
4. Editor handles Roslyn and IntelliSense.
5. Runtime manages dotnet CLI execution.
6. Each layer has a single responsibility.

This ensures:
- Maintainability
- Scalability
- Testability
- Long-term extensibility