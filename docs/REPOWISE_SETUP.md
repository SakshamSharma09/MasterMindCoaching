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

## Start The Local UI

Run this from the repository root:

```powershell
$env:REPOWISE_EMBEDDER = "mock"
& "C:\Users\Saksham\AppData\Roaming\Python\Python314\Scripts\repowise.exe" serve --host 127.0.0.1 --port 7338 --ui-port 7339
```

Open:

```text
http://localhost:7339
```

The mock embedder keeps the server local and avoids requiring external API keys. Chat/search quality is limited until a real embedder is configured.

## Generate Full AI Wiki Pages

Full `repowise init` documentation generation sends source-code context to the selected LLM provider. Do this only after explicitly choosing and approving a provider.

Example after setting a Gemini key:

```powershell
$env:GEMINI_API_KEY = "<your-key>"
& "C:\Users\Saksham\AppData\Roaming\Python\Python314\Scripts\repowise.exe" init . --provider gemini --embedder gemini --coverage 0.20 --wiki-style comprehensive --no-agents --no-codex --no-onboarding --skip-infra --skip-tests --yes
```

## Check Status

```powershell
& "C:\Users\Saksham\AppData\Roaming\Python\Python314\Scripts\repowise.exe" status
```
