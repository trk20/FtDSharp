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
- **In-game compile and runtime error reporting**: Roslyn diagnostics with line numbers shown in the Lua box log (no more cryptic Lua errors!)
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

For a more detailed guide on getting started and more advanced topics, check the [GitHub wiki](https://github.com/trk20/FtDSharp/wiki/Getting-Started)

## Why C#?

FtD's built-in Lua API is functional, but writing anything non-trivial means wrestling with index-based loops, manual vector math, zero type safety, and terrible error handling. FtDSharp replaces all of that with a strongly-typed C# API that handles the boilerplate for you, extends scripting capabilities to match breadboards, and gives you the full (ish) power of C# to write and debug your scripts.

### Missile Guidance: Lua vs C#

A real community Lua missile guidance script - 90+ lines of index loops, manual vector math helper functions, and raw table manipulation:

<details>
<summary><strong>Lua: 90+ lines</strong></summary>

```lua
-- Credit to bigbean6142 on the OFD discord for this Lua missile guidance example
local recentVelocities = {}
local recentAccelerations = {}
local v = 0
local maxTicks = 0.05 * 40
local sumVelocity = {x = 0, y = 0, z = 0}
local sumAcceleration = {x = 0, y = 0, z = 0}
local count = 0

function Update(I)
    local targetInfo = I:GetTargetInfo(0, 0) -- hardcoded to first mainframe's primary target
    if not targetInfo.Valid then return end

    local previousVelocity = recentVelocities[v] or targetInfo.Velocity
    recentVelocities[v] = targetInfo.Velocity
    recentAccelerations[v] = Vector3_Subtract(targetInfo.Velocity, previousVelocity)

    sumVelocity = {x = 0, y = 0, z = 0}
    sumAcceleration = {x = 0, y = 0, z = 0}
    count = 0

    for _, velocity in pairs(recentVelocities) do
        sumVelocity = Vector3_Add(sumVelocity, velocity)
        count = count + 1
    end
    for _, acceleration in pairs(recentAccelerations) do
        sumAcceleration = Vector3_Add(sumAcceleration, acceleration)
    end

    local averageVelocity = Vector3_Divide(sumVelocity, count)
    local averageAcceleration = Vector3_Divide(sumAcceleration, count)
    v = (v + 1) % maxTicks

    for i = 0, I:GetLuaTransceiverCount() - 1 do
        for j = 0, I:GetLuaControlledMissileCount(i) - 1 do
            local missile = I:GetLuaControlledMissileInfo(i, j)
            local distanceToTarget = Vector3_Distance(missile.Position, targetInfo.Position)
            local timeToImpact = distanceToTarget / Vector3_Magnitude(missile.Velocity)

            local predictedPosition = Vector3_Add(
                Vector3_Add(targetInfo.Position, Vector3_Multiply(averageVelocity, timeToImpact)),
                Vector3_Multiply(Vector3_Multiply(averageAcceleration, timeToImpact), 0.5 * timeToImpact)
            )

            I:SetLuaControlledMissileAimPoint(i, j, predictedPosition.x, predictedPosition.y, predictedPosition.z)
        end
    end
end

function Vector3_Add(a, b)
    return {x = a.x + b.x, y = a.y + b.y, z = a.z + b.z}
end
function Vector3_Subtract(a, b)
    return {x = a.x - b.x, y = a.y - b.y, z = a.z - b.z}
end
function Vector3_Multiply(v, scalar)
    return {x = v.x * scalar, y = v.y * scalar, z = v.z * scalar}
end
function Vector3_Divide(v, scalar)
    return {x = v.x / scalar, y = v.y / scalar, z = v.z / scalar}
end
function Vector3_Magnitude(v)
    return math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z)
end
function Vector3_Distance(a, b)
    return Vector3_Magnitude(Vector3_Subtract(a, b))
end
```

</details>

**C#: 28 lines**, same logic, actually readable:

```csharp
using FtDSharp;
using UnityEngine;

public class MissileGuidance : IFtDSharp
{
    public void Update(float deltaTime)
    {
        var target = AI.HighestPriorityMainframe.PrimaryTarget;
        foreach (var controller in Weapons.MissileControllers)
            controller.Fire();

        if (target == null)
        {
            foreach (var m in Guidance.Missiles) m.Detonate();
            return;
        }

        foreach (var missile in Guidance.Missiles)
        {
            float timeToImpact = Vector3.Distance(missile.Position, target.Position)
                               / missile.Velocity.magnitude;

            Vector3 predicted = target.Position + target.Velocity * timeToImpact;

            missile.AimAt(predicted);
        }
    }
}
```

No index loops, manual vector math, or annoying boilerplate.

### PID Control: Lua vs C#

Lua PID: you have to build everything yourself:

<details>
<summary><strong>Lua: 50+ lines just for the PID implementation</strong></summary>

```lua
-- Credit to errorstringexpectedgotnil on the OFD discord for this Lua PID library implementation
PIDLib = {}

PIDLib.sdt = 0.025

function PIDLib.new(Kp, Ti, Td)
  local pid = {}
  pid.Kp = Kp
  pid.Td = Td
  pid.errSum = 0
  pid.prevErr = 0

  if Ti == 0 then
    pid.inverseTi = 0
  else
    pid.inverseTi = 1/Ti
  end

  setmetatable(pid, PIDLib.mt)

  return pid
end

function PIDLib.iterate(pid, err, deltaTime)
  pid.errSum = pid.errSum + err * deltaTime
  local output = pid.Kp * (err + pid.inverseTi * pid.errSum + pid.Td * ((err - pid.prevErr) / deltaTime))
  pid.prevErr = err
  return output
end

function PIDLib.opinion(pid, err, deltaTime)
  return pid.Kp * (err + pid.inverseTi * (pid.errSum + err) + pid.Td * ((err - pid.prevErr) / deltaTime))
end

function PIDLib.reset(pid)
  pid.errSum = 0
  pid.prevErr = 0
end

function PIDLib.standardIterate(pid, err)
  return PIDLib.iterate(pid, err, PIDLib.sdt)
end

function PIDLib.standardOpinion(pid, err)
  return PIDLib.opinion(pid, err, PIDLib.sdt)
end

PIDLib.mt = {}

PIDLib.mt.__add = PIDLib.standardIterate
PIDLib.mt.__mul = PIDLib.standardOpinion
PIDLib.mt.__unm = PIDLib.reset

-- Then in your main script, manually wire it up:
local altPid = PIDLib.new(0.1, 50, 0.5)
function Update(I)
    local error = targetAltitude - I:GetConstructPosition().y
    local output = PIDLib.iterate(altPid, error, 0.025)
    I:RequestThrustControl(5, output)  -- raw axis index
end
```

</details>

**C#: just bind and update:**

```csharp
using FtDSharp;

public class AltitudeHold : IFtDSharp
{
    private readonly PID _altPid = PID.Bind(
        input: () => Game.MainConstruct.Position.y,
        output: v => Game.MainConstruct.Propulsion.Hover = v,
        setpoint: () => 200f // defaults same as AI PIDs - kP=0.05f, kI=250f, kD=0.3f
    );

    public void Update(float deltaTime) => _altPid.Update(deltaTime);
}
```

The `PID` helper class handles error computation, integral accumulation, derivative smoothing, and output clamping. You just bind inputs and outputs and call `Update()`.

### Weapon Control: Lua vs C#

Firing cannons at a target with velocity prediction in Lua:

<details>
<summary><strong>Lua — manual lead calculation with no gravity or ballistic arc</strong></summary>

```lua
-- Credit to maglor6 on the OFD discord for this Lua weapon control example
function Update(I)
    for AI = 0, (I:GetNumberOfMainframes() - 1) do
        for T = 0, (I:GetNumberOfTargets(AI) - 1) do
            TI = I:GetTargetPositionInfo(AI, T)
            TP = TI.Position
            TV = TI.Velocity
            R = TI.Range
            for W = 0, (I:GetWeaponCount() - 1) do
                WI = I:GetWeaponInfo(W)
                GP = WI.GlobalPosition
                V = WI.Speed
                IT = R / V
                APX = TP.x - GP.x + IT * TV.x
                APY = TP.y - GP.y + IT * TV.y
                APZ = TP.z - GP.z + IT * TV.z
                I:AimWeaponInDirection(W, APX, APY, APZ, 0)
                I:FireWeapon(W, 0)
            end
        end
    end
end
```

Even with all this manual math, this Lua version only handles simple velocity leading - not accounting for gravity, possible high vs low ballistic arcs, or terrain checks. That could easily be a few hundred more lines of Lua code to implement properly.

</details>

**C# — one-line tracking with full ballistic prediction:**

```csharp
using FtDSharp;

public class SimpleWeaponControl : IFtDSharp
{
    public void Update(float deltaTime)
    {
        var target = AI.HighestPriorityMainframe?.PrimaryTarget;
        if (target == null) return;

        foreach (var weapon in Weapons.All)
            if (weapon.Track(target).CanFire)
                weapon.Fire();
    }
}
```

`weapon.Track()` handles velocity leading, acceleration, gravity, ballistic arc selection, checks terrain blocking, and returns a status you can access easily, no index loops or manual vector math required.

## Error Handling

**Lua**: Errors are cryptic, usually unhelpful, and only show happen at runtime when the offending code is executed:
![alt text](image.png)

**FtDSharp**: Clear Roslyn diagnostics with line numbers, shown in the Lua box log:
![alt text](image-1.png)

##### Note: the pretty UI shown here is part of jalansia's [AtsuLuaEditor mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3405611847), but FtDSharp's error reporting works with or without it.

---

## Brief API Overview

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

| Key Interface | Description                                                |
| ------------- | ---------------------------------------------------------- |
| `IWeapon`     | Weapon block with `Track`, `Fire`, `AimAt`                 |
| `ITurret`     | Turret coordinating child weapons                          |
| `IMainframe`  | AI mainframe with `PrimaryTarget`, `GetAimpoint`           |
| `ITargetable` | Anything trackable: `Position`, `Velocity`, `Acceleration` |
| `IMissile`    | Script-controllable missile with typed parts               |
| `IBlock`      | Base for all blocks: `Position`, `Parent`, `IsOnRoot`      |

For detailed API documentation and examples, see the [GitHub wiki](https://github.com/trk20/FtDSharp/wiki/Getting-Started#type-hierarchy-condensed)

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
