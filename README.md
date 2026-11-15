# P-06 Extended Mod (plugin version)

A gameplay mod for Sonic P-06, the Unity fan remake of Sonic the Hedgehog (2006). It adds new moves for Sonic, Shadow and Knuckles, moves that work for every character, and an in-game mod menu with cheats, speed tuning and saved settings.

This is the plugin version. It loads through MelonLoader and changes the game only at runtime with Harmony patches, so the game's files stay untouched and the mod can run alongside other mods. The first version of the mod was made by editing the game's decompiled assembly directly; this repo ports it to a plugin.

## Demo

**[Watch the demo video on YouTube](https://www.youtube.com/watch?v=bUYLfmtYh9E)**

<a href="https://www.youtube.com/watch?v=bUYLfmtYh9E">
  <img src="https://img.youtube.com/vi/bUYLfmtYh9E/maxresdefault.jpg" alt="Watch the demo on YouTube" width="480">
</a>

## Features

### All characters

- Wall jump. Falling against a steep wall makes the character cling to it, and A jumps off away from the wall. Knuckles and Rouge keep their own climbing, and Mach Speed Sonic and the snowboard are excluded. Omega gets a longer wall check because of his size.
- Side dodge. LB and RB do a quick sidestep, with the direction worked out from the camera.
- Water running. Coming down onto water fast enough keeps the character running across it instead of sinking.
- Waterslides and waterslide boosters work for every character, not only Sonic.

### Sonic

- Stomp. Press B in the air, or after a spring, jump panel, rainbow ring, pole or rope, to slam down. If you hold the stick when you land, the impact turns into a spin dash. Landing on an enemy bounces you back up, and otherwise Sonic does a short get-up.
- After-homing movement. The game normally stops Sonic in mid-air after a homing attack; with this on he keeps his forward speed, up to a configurable cap.
- A new effect for the side dodge.

### Mach Speed Sonic

- Stomp on B, which lands back into normal running.
- A second jump: press A again in the air, once per jump.

### Shadow

- Stomp, with the same landing rules as Sonic's.
- After-homing movement.

### Knuckles

- Climb All Walls lets him climb any wall, not only the ones the level marks as climbable.

### Mod menu

Press F12 in game to open the menu. It has these sections:

- Quick settings: ultra smooth FPS, custom music, a global speed multiplier, a speedometer, and switches for water running, wall jump, climb all walls and after-homing movement.
- Cheats: invincibility, infinite gauge, rings and lives, maxed-out gems, buttons that give each gem and story item, water immunity, faster chain jumps, and teleporting to a level section by name.
- Advanced speed control: separate multipliers for ground, air, spin dash, flying, climbing, homing and after-homing rotation.
- Other: the old camera controls, an experimental fixed-timestep fix, and a warning if your P-06 version doesn't match the one the mod was built for.
- Saving: save and load every setting above, by hand or automatically.

## How it works

MelonLoader loads `P06Extended.dll` from the game's `Plugins` folder and applies every `[HarmonyPatch]` class in it. The patches are prefixes and postfixes on the game's own methods, so the game's code is never rewritten, on disk or in memory. A few prefixes return `false` to skip the original method for that call (the waterslide triggers and Knuckles' `CanClimb`); that is how the mod replaces a behaviour without copying the whole method.

Each character gets a companion component. A postfix on the character's `Awake` or `Start` adds it to the player's GameObject:

| Game class   | Mod component | Adds |
|--------------|---------------|------|
| `PlayerBase` | `XPlayerBase` | Moves shared by every character |
| `SonicNew`   | `XSonicNew`   | Stomp, after-homing movement |
| `SonicFast`  | `XSonicFast`  | Stomp, second jump |
| `Shadow`     | `XShadow`     | Stomp, after-homing movement |
| `Knuckles`   | `XKnuckles`   | Climb all walls |

The new moves run inside the game's own state machine. `StateMachine.ChangeState` takes a method delegate and calls `<StateName>Start` and `<StateName>End` on the GameObject through `SendMessage`. The companion component sits on the same GameObject, so `XSonicNew.StateStomp` with its `StateStompStart` and `StateStompEnd` behaves like a built-in state, even though the game's `State` enum can't be extended. A postfix on each character's `Update` checks the input and conditions and switches into the new state.

Most of the fields the mod needs, such as the current speed, the ground raycast and the player state, are private. `Helpers.cs` wraps reflection so the code can read and write them by name, for example `I.Flt["CurSpeed"]` or `I.Boo["LockControls"]`. Because patches and reflection only add code around the game's, the mod can share a class with other mods that patch it. Two mods that both skip the same method would still conflict.

## Project layout

```
P06Extended/
  Main.cs                     MelonLoader entry point
  Properties/AssemblyInfo.cs  MelonInfo and MelonGame attributes
  XCommon.cs                  Version, data folder path, section teleport
  Helpers.cs                  Reflection wrappers, XInput (Rewired) button names
  Extensions.cs               Reflection extension methods (Get, Set, InvokeFunc...)
  Core/
    XDebug.cs                 Settings, cheats, speed multipliers, debug hotkeys and overlay
    XFiles.cs                 Loads the data files in Plugins\P06X and checks the game version
    XSingleton.cs             Lazy singleton base: finds or creates the manager object
    XValue.cs                 A setting value that runs a callback when it changes
    XUtility.cs               XML save/load and small extension helpers
  Effects/XEffects.cs         Particle and sound effects for the new moves
  UI/                         The F12 mod menu: XModMenu builds it from XUIMenu,
                              XUISection and the XUI* controls; XLogBox shows messages
  XSplashScreens.cs           Debug only: skips the splash screens to a set area
  XKnuckles.cs                Knuckles patches
  PlayerBase/                 XPlayerBase and its patches
    States/                   WallJump, VDodge (side dodge), WaterRun, WaterSlide, Boost
  SonicNew/                   XSonicNew, patches, Stomp and GetUpX states
  SonicFast/                  XSonicFast, patches, Stomp and SecondJump states
  Shadow/                     XShadow, patches, Stomp and GetUpX states
```

## Building

You need Visual Studio 2022 or the Build Tools with the .NET Framework 4.7.2 targeting pack, and a copy of Sonic P-06 with MelonLoader installed. The project compiles against the game's own DLLs, which are not in this repo.

1. Copy `Directory.Build.props.user.example` to `Directory.Build.props.user` and set `P06GameDir` to your game folder. A `P06GameDir` environment variable or `-p:P06GameDir=...` on the msbuild command line also works.
2. Open `P06X.sln` and build, or run:

   ```
   msbuild P06X.sln -p:Configuration=Release
   ```

The build closes the game if it is running, copies `P06Extended.dll` into the game's `Plugins` folder and starts the game again. To build without touching the game, add `-p:PreBuildEvent= -p:PostBuildEvent=`.

## Installing

The mod targets P-06 version 1.45 with MelonLoader v0.5.5.

1. Install MelonLoader v0.5.5 into the P-06 folder.
2. Put `P06Extended.dll` in `<game>\Plugins\`.
3. Put the mod's data files (`Particle.png`, `custom_music.ogg`, `stomp_land.ogg` and `ring.ogg`) in `<game>\Plugins\P06X\`. The mod shows an error on start if one is missing. It also keeps `xconfig.ini` and `user_settings.xml` in that folder.
4. Start the game. A message confirms the install, and F12 opens the menu.

## Controls

| Input | Action |
|-------|--------|
| F12 | Open or close the mod menu |
| A, while clinging to a wall | Wall jump |
| LB / RB | Side dodge |
| B, in the air | Stomp (Sonic, Mach Speed Sonic, Shadow) |
| A, in the air (Mach Speed Sonic) | Second jump |

Buttons refer to the game's default controller layout.

## Status

The mod works, and the port from the assembly version is still going. Next on the list are bug fixes and porting the remaining features. The boost, for example, already has its menu settings, but the move itself hasn't been ported yet.

## License

This project is licensed under GPL-3.0-with-Autoconf-exception. See LICENSE for details.
