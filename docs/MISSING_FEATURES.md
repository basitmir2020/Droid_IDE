# DroidIDE Feature Gap Analysis

This document outlines the current state of DroidIDE and identifies missing features required to reach parity with full-featured mobile IDEs.

## 1. Project & File Management
- **Context Menus**: Currently, the Explorer panel is read-only for navigation. It lacks right-click (or long-press) menus for:
    - Creating new files/folders.
    - Deleting items.
    - Renaming items.
- **Multi-Project Solutions**: While the `SolutionParser` can detect multiple projects, the UI and Build system are currently optimized for single-project folders.
- **Project Configuration UI**: No visual way to edit `.csproj` settings (Target Framework, Root Namespace, Package References) without editing XML directly.

## 2. Editor & IntelliSense
- **Advanced Navigation**: Roslyn services are present, but the following are not yet wired to the Monaco UI:
    - **Go to Definition**: Jumping to symbol declarations.
    - **Find All References**: Listing all usages of a variable/method.
    - **Symbol Search**: Quick navigation (Ctrl+Shift+O equivalent) within a file.
- **Refactoring UI**: No support for "Rename Symbol", "Extract Method", or "Simplify Code" via UI actions.
- **Code Formatting**: Missing "Format Document" (Shift+Alt+F) command.

## 3. Git Integration
- **Full Source Control UI**: The Git panel currently only shows file status (Modified, Added, etc.). Missing features include:
    - **Stage/Unstage**: Checkboxes or buttons to stage specific files.
    - **Commit Management**: A text area for commit messages and a "Commit" button.
    - **Sync Operations**: UI for "Push", "Pull", and "Fetch".
    - **Branching**: A way to create, switch, and delete branches visually.
    - **Merge Conflicts**: No specialized UI for resolving merge conflicts.

## 4. NuGet Package Management
- **Search & Discovery**: No UI to search the NuGet.org gallery.
- **Visual Management**: No panel to list installed packages, update them, or remove them.
- **Restore Reliability**: Current implementation depends on `dotnet restore` CLI, which faces sandbox restrictions on Android.

## 5. Debugger (Critical Gap)
- **Breakpoints**: No way to set line-based breakpoints.
- **Execution Control**: No implementation for "Step Over", "Step Into", or "Continue".
- **Inspection**: Missing "Locals", "Watch", and "Call Stack" panels.
*Note: This is the most complex missing feature, requiring a deep integration with the .NET Debugger protocol (DAP).*

## 6. Runtime & Environment
- **SDK Management**: Currently, the IDE depends on a manually installed .NET SDK (e.g., via Termux). A built-in "Runtime Manager" to download and manage SDKs within the app sandbox would significantly improve the first-run experience.
- **App Sandboxing**: Better integration for running web apps or MAUI apps directly from the IDE without manual path bridging.

## 7. UX & Personalization
- **Theme Gallery**: Expanded theme support beyond just Dark/Light.
- **Keyboard Shortcuts**: A UI to view and customize editor hotkeys.
- **Extension Support**: A plugin architecture for adding support for other languages (C++, Rust, etc.).
