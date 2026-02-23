# Core Layer

Contains:

- Domain models
- Interfaces
- Enums
- Constants

Example:
- IProcessManager
- IProjectParser
- IGitService

Rules:

- No references to MAUI
- No OS-specific logic
- No external dependencies

Core must remain pure and framework-independent.