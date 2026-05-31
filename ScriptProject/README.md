# ScriptProject

Open this folder in VS Code (with the C# Dev Kit extension) for IntelliSense while writing FtDSharp scripts.

## Requirements

This template expects the following files relative to the mod root (`Mods/FtDSharp/`):

- `FtDSharp.API.dll` — script API (included in the mod release)
- `FtDSharp.dll` — required for `Blocks.*` IntelliSense (included in the mod release)
- `References/UnityEngine.CoreModule.dll` — Unity types for IDE support (included in the mod release)

## Usage

1. Open **this folder** (`ScriptProject/`) in VS Code — not the whole mod directory.
2. Edit `MyScript.cs` (or add your own `.cs` files).
3. Copy the script source into a Programmable Block in-game.

Global usings in the `.csproj` mirror what the in-game compiler injects, so `Log()` works without imports and `Game.*` uses the class prefix.
