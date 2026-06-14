# Repowise Setup

Repowise is configured as a local developer tool for codebase intelligence. It is not part of the production web application.

## Installed Tool

Current local install:

```powershell
python -m pip install --user repowise
```

The MCP command in `.mcp.json` points to:

```text
C:/Users/Saksham/AppData/Roaming/Python/Python314/Scripts/repowise.exe
```

If Python is reinstalled or the path changes, update `.mcp.json` and `.repowise/mcp.json`.

## Refresh The Index

Run this from the repository root:

```powershell
& "C:\Users\Saksham\AppData\Roaming\Python\Python314\Scripts\repowise.exe" init . --index-only --mode fast --no-agents --no-codex --no-onboarding --skip-infra --skip-tests --yes
```

The repo commits only Repowise configuration. Local generated index files such as `.repowise/wiki.db`, `.pkl` caches, and `knowledge-graph.json` are intentionally ignored.

## Check Status

```powershell
& "C:\Users\Saksham\AppData\Roaming\Python\Python314\Scripts\repowise.exe" status
```
