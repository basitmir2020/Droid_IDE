# App Layer

Responsible for:

- Rendering UI
- Managing Views and ViewModels
- Handling user interactions
- Communicating with backend services

Must NOT:

- Execute dotnet commands directly
- Access filesystem directly
- Contain Roslyn logic

This layer depends on all other layers.