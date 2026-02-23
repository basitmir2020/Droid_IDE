# Git Layer — `DroidIDE.Git`

> **Status:** ✅ Implemented

The Git layer provides version control integration using LibGit2Sharp, enabling clone, commit, push, pull, branching, and file status tracking entirely on-device.

---

## Directory Structure

```
DroidIDE.Git/
├── GitService/
│   └── GitService.cs          # IGitService implementation
├── BranchManager/
│   └── BranchManager.cs       # Branch operations
└── RepositoryManager/
    └── RepositoryManager.cs   # Repository state tracking
```

---

## Features

### GitService (`IGitService`)

| Method | Description |
|--------|-------------|
| `CloneAsync(url, path, credentials)` | Clone remote repository |
| `InitAsync(path)` | Initialize new local repo |
| `CommitAsync(repoPath, message, author)` | Create commit from staged changes |
| `PushAsync(repoPath, credentials)` | Push to remote |
| `PullAsync(repoPath, credentials)` | Pull from remote |
| `GetStatusAsync(repoPath)` | File change status (modified, staged, untracked) |
| `GetBranchesAsync(repoPath)` | List all branches |
| `CheckoutAsync(repoPath, branchName)` | Switch branches |
| `StageAsync(repoPath, filePaths)` | Stage files for commit |
| `UnstageAsync(repoPath, filePaths)` | Unstage files |

### BranchManager
- List local and remote branches
- Create, delete, and rename branches
- Merge branches with conflict detection

### RepositoryManager
- Open/close repository tracking
- Working directory state management
- File status tracking per `GitFileStatus` enum (`Modified`, `Added`, `Deleted`, `Untracked`, `Renamed`)

### App Layer Integration
- **GitPanelView** — changed files list, staging area, commit message input, branch info
- **GitViewModel** — full MVVM data binding for Git operations
- **ActivityBar** git button toggles the Git sidebar panel
- **StatusBarView** displays current branch name

---

## Dependencies

- `DroidIDE.Core` (for `IGitService`, `GitFileStatus`)
- `LibGit2Sharp` NuGet package

---

## Security

Git credentials (tokens, passwords) are stored via `CredentialStore` in the Infrastructure layer. The `IGitService` interface accepts credentials as parameters — the App layer retrieves them from `CredentialStore` before passing to Git operations.