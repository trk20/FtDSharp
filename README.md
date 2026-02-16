# FtDSharp

> Replace Lua scripting in _From The Depths_ with C#.

FtDSharp is a mod that replaces the game's Lua-based scripting system with a C# scripting environment, compiled at runtime via Roslyn. It provides a fully custom, strongly-typed API that is designed to be intuitive and easy to use, preferring to expose information directly as properties rather than requiring lookups, and offering built-in helpers for common tasks like PID control and weapon coordination.

### Alpha Status

This mod is currently in early alpha. Although it already has a superset of the Lua API and many extra features, the API is still evolving rapidly based on testing and (now) user feedback. The wiki and in-box documentation are also still in progress. Expect possible breaking changes to the API as I iterate on the design and add more features. Once the API stabilizes and the documentation is more complete, I will move to a beta phase with a Steam Workshop release.

## Features

- **Full C# 10**: Namespaces, pattern matching, records, lambdas, LINQ, and more
- **Complete API coverage**: Features a complete\* superset of the Lua API, plus many new features and helpers
- **Custom typed API**: Weapons, Missiles, AI, Fleet, Drawing, Blocks, Propulsion, Warnings
- **200+ source-generated block accessors**: Typed interfaces for most block types in the game, like a GBG/GBS on steroids
- **Built-in helpers**: PID helper class, WeaponController for custom weapon groups
- **Extra Features**: Did I hear script controlled missile trails/smoke? Inbuilt vector drawing for debugging? Yes and yes (with more to come).
- **Significantly faster than Lua**: Roslyn-compiled C# runs faster than interpreted Lua, especially for complex scripts, and the API is designed to minimize allocations and expensive operations to keep performance impact low. Your missile guidance scripts are now less of a concern than the missiles themselves!
- **Security sandbox**: Reflection, file I/O, and network access are intentionally blocked to mitigate malicious scripts. This will be iteratively improved based on user feedback/as needed.
- **In-game error reporting**: Roslyn diagnostics with line numbers shown in the Lua box log (no more cryptic Lua errors!)
- **No perfect information**: Unlike vanilla Lua, FtDSharp does NOT expose perfect information about enemy vehicles. This means that the target information you get from the API is the same as with any other system.

_Note: platform support is untested but should work anywhere the game runs aside from multiplayer_

###### \* Some features might only be accessible through the source-generated classes which may be less intuitive. This will be improved over time based on user feedback. Additionally, multiplayer support is not yet implemented - I am hesitant to add it as it opens up a lot of potential for abuse. If there is significant demand for multiplayer support, I will consider adding it with appropriate safeguards.

## Future Improvements To Come

- **GitHub Wiki**: A full API reference and example gallery on the GitHub wiki, which will be the primary documentation source for the mod
- **In-box documentation**: Replacement for the obsolete Lua API docs in the Help tab, auto-generated from the wiki
- **Steam Workshop release**: Once the mod is (relatively) stable and well-documented, it will be released on the Steam Workshop for easier installation and updates
- **More Bespoke APIs**: Slow replacement of commonly used source-generated APIs with hand-crafted ones that are more intuitive, additional helpers for common patterns, behaviour overrides, and more
- **Performance Improvements**: Ongoing optimizations to reduce script impact on game performance.
- **Game Balance**: Based on user feedback, some API features may receive configuration options for game balance/tuning. These will be opt-in and clearly documented to avoid confusion.
- **IDE Integration**: Enabling better IDE support by providing a NuGet package to allow users to write scripts in their own IDE with full IntelliSense and compile-time checking, then copy-paste into the Programmable Block. Long-term extension of this could include a Visual Studio Code extension or similar for direct editing.
- **Improved Example Scripts**: The current example scripts are ad-hoc and mostly focused on demonstrating API features. Future additions will include more polished, real-world use cases and patterns.
- **Community Contributions**: As the mod matures, I hope to see contributions from the community in the form of new features, API improvements (including naming things), example scripts, and documentation enhancements.

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the zip into your `From The Depths/Mods/` folder (so that `plugin.json` is at `Mods/FtDSharp/plugin.json`)
3. Launch the game — the mod loads and does its thing!

## Quick Start

Place a **Programmable Block** (cosmetic change from **Lua Box** with the mod enabled). The default template compiles and runs immediately:

```csharp
using FtDSharp;
using static FtDSharp.Logging;
using static FtDSharp.Game;

public class SampleScript : IFtDSharp
{
    public SampleScript()
    {
        Log("FtDSharp script template running.");
        Log($"I am {MainConstruct.Name} at {MainConstruct.Position}");
    }

    public void Update(float deltaTime)
    {
        // TODO: implement your logic here.
    }
}
```

Your script must implement `IFtDSharp`, which requires a single method:

```csharp
void Update(float deltaTime);  // Called every game tick (~40 Hz at 1x speed)
```

The following namespaces are imported automatically: `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`, `FtDSharp`.

## API Overview

| Static Class | Purpose                                                        |
| ------------ | -------------------------------------------------------------- |
| `Game`       | `MainConstruct`, `Time`, `TicksSinceStart`                     |
| `AI`         | `Mainframes`, `HighestPriorityMainframe`                       |
| `Weapons`    | `All`, `Turrets`, `APS`, `MissileControllers`, typed accessors |
| `Guidance`   | Active missiles in flight                                      |
| `Friendly`   | `All`, `AllExcludingSelf`, `Fleets`, `MyFleet`                 |
| `Warnings`   | `IncomingProjectiles`, `IncomingMissiles`                      |
| `Drawing`    | `Arrow`, `Line`, `Sphere`, `Cross`, and more                   |
| `Blocks`     | Auto-generated typed accessors for all block types             |
| `Logging`    | `Log`, `LogWarning`, `LogError`, `ClearLogs`                   |

| Helper             | Purpose                                   |
| ------------------ | ----------------------------------------- |
| `PID`              | PID controller with bindable input/output |
| `WeaponController` | Coordinate multiple weapons and turrets   |
| `FrameCache<T>`    | Per-frame lazy caching                    |

| Key Interface | Description                                                |
| ------------- | ---------------------------------------------------------- |
| `IWeapon`     | Weapon block with `Track`, `Fire`, `AimAt`                 |
| `ITurret`     | Turret coordinating child weapons                          |
| `IMainframe`  | AI mainframe with `PrimaryTarget`, `GetAimpoint`           |
| `ITargetable` | Anything trackable: `Position`, `Velocity`, `Acceleration` |
| `IMissile`    | Script-controllable missile with typed parts               |
| `IBlock`      | Base for all blocks: `Position`, `Parent`, `IsOnRoot`      |

## Example Scripts

The [`ExampleScripts/`](ExampleScripts/) folder contains working examples demonstrating a large portion of the API surface:

| Script                            | Demonstrates                                                |
| --------------------------------- | ----------------------------------------------------------- |
| `BasicThrustControl.cs`           | Altitude hold + target tracking via Propulsion API          |
| `DrawingDemo.cs`                  | Visual debug Drawing API (arrows, spheres, gimbals)         |
| `FleetAwarenessDemo.cs`           | Fleet/friendly awareness with visualization                 |
| `GenericBlockGetterSetterDemo.cs` | Auto-generated block API with read/write properties         |
| `MissilePartsDemo.cs`             | Typed missile part access and visual control                |
| `MultiAIAimpointComparison.cs`    | Comparing aimpoints across multiple AI mainframes           |
| `NaiveMissileGuidance.cs`         | Simple missile guidance: fire + aim at target               |
| `PidControlDemo.cs`               | PID controller helper with bound inputs/outputs             |
| `ProjectileWarningsDemo.cs`       | Incoming projectile warning visualization                   |
| `SubObjectHierarchy.cs`           | SpinBlock parent hierarchy inspection                       |
| `SubTurretDemo.cs`                | Independent sub-turret targeting with `WeaponController`    |
| `TargetInfoPrinter.cs`            | Print detailed target information                           |
| `WeaponControlDemo.cs`            | Full weapon control with typed accessors + pattern matching |
| `WeaponTypePropertiesDemo.cs`     | Typed block interfaces extending `IWeapon`                  |

## Writing Your Own Scripts

TODO: guide on getting VSCode set up for IntelliSense (should just be a matter of adding the DLLs as references? Need to test this and write up instructions)

## Building from Source

### Prerequisites

- .NET SDK 8.0+
- A copy of _From The Depths_ (for the game's managed DLLs)

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/trk20/FtDSharp.git
   ```

2. Copy the game's managed DLLs into a `ftd-managed/` folder at the project root:

   ```
   <Steam>/steamapps/common/From The Depths/From_The_Depths_Data/Managed/*  →  ftd-managed/
   ```

3. Run the code generator to produce API bindings:

   ```bash
   dotnet run --project FtDSharp.CodeGen
   ```

4. Build:

   ```bash
   dotnet build
   ```

   The post-build step copies the required DLLs to the project root, ready for the game's mod loader.

## License

This project is currently licensed under [CC BY-SA 4.0](LICENSE.md).
