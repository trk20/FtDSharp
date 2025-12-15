# Design Objectives

This mod aims to replace the Lua scripting in From The Depths with C#. However, this could be done in a great number of different ways.

Listed here are some overall objectives/goals the mod aims to fulfill as best it can.

### Replicate a superset of existing Lua functionality

Eventually, C# scripting should present a complete superset of things you can do in the base game with Lua.

### Ease of use and intuitiveness

The mod should, when possible, make it easier for a player to write scripts. This includes how things are named, structured, and exposed - for example, target information being accessible as a flat list instead of iterating over target indices for an AI and needing a getter. In the general case, it should attempt to follow the principle of least surprise - a component of a system should behave in a way that most users will expect it to behave, and therefore not astonish or surprise users.

Things that are currently indirectly possible with Lua but unintuitive (eg requiring calculating a target's block count from the AI priority) should be instead readily accessible (eg as a property of the target).

The mod should also expose generally useful capabilities, implemented in an intelligent and efficient way, that would otherwise require a player to implement it themselves. For example - instead of requiring a player to implement a PID, instead exposing a PID class that they can use from the get-go.

### Reduce unintuitive aspects and remove objectively unfair advantages

Some base game behaviour is not intuitive to a player - for example, missiles being able to access perfect target position, but having error added to the missile and aimpoint positions to compensate. Additionally, Lua has some capabilities that grant it an objectively unfair advantage - eg having perfect target position. Aspects like this should be corrected to fall in line with other control options.

In general, choosing control via script should be a _preference_, not an _optimization_.
