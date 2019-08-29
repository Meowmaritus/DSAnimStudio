## About TimeAct Editor DX:
* Edits the TimeAct Editor files of Dark Souls 3 and Sekiro (in the middle of adding support for other games). These files control **everything** that happens on a specific frame of an animation, such as:
  * i-frames
  * parry windows
  * applying an "SpEffect" (special temporary statuses such as ring effects, poisoning, buffs, AI triggers, etc)
  * allowing animation cancelling
  * setting the flag for YOU DIED and respawning
  * creating "SFX" / "FFX" (both refer to the exact same files: visual effects)
  * playing sound effects such as footsteps, sword swooshes, etc.
  * invoking an attack behavior (does damage to opponent, drains stamina from player, etc all in one event)
  * invoking a "bullet" (projectile) behavior (fires projectile, drains stamina from player, etc all in one event)
  * invoking a "common" behavior (like attack behaviors but for simpler things such as falling on someone's head causing stagger)
  * creating motion blur on weapon swings
  * setting the opacity of a character (used for getting summoned into other worlds, dying, etc)
  * setting attack aim tracking speed of a character
  * playing a "RumbleCam" file (relative screen movement e.g. Smough's footsteps shaking screen)
  * playing additional animation layers (e.g. all of Gwyn's animations have events to play his clothes-blowing-in-wind animation layered on top of the other animations)
  * adjusting model render masks (showing/hiding specific parts of characters)
  * **many more that we haven't even figured out yet!**
* Shows an actual physical graph full of events represented as boxes.
* Snaps to 30-fps increments just like the vanilla files do.
* Allows you to add new events to animations by right-clicking (if a template is loaded).
* Allows you to delete events by highlighting them and pressing the Delete key.
* Allows you to modify the parameters passed to each event (click an event to highlight it, then the parameters appear in the pane on the right side of the window)
* **Has full undo/redo functionality with Ctrl+Z/Ctrl+Y**
* **Has full copy/paste functionality**:
  * **Ctrl+C**: Copy
  * **Ctrl+V**: Paste at Mouse Cursor
  * **Ctrl+Shift+V**: Paste In-Place (keeps original start/end times, useful for copying between animations)
* Edits the .anibnd or .anibnd.dcx files of the games directly. **No need to use BND rebuilders.**

## User Instructions:
  1. Run `TimeActEditorDX.exe`
  1. Go to File -> Open
  1. Load a .anibnd or .anibnd.dcx.
  1. Go to File -> Load Template...
  1. Load a .xml template file which corresponds to the type of ANIBND you are loading from the /res/ directory (it will automatically open to that directory)
  1. Select an animation ID on the left pane
  1. Drag some events around or otherwise mess with things (try the right pane for editing the highlighted event)
  1. Hit Ctrl+S to save. If the `File -> Force Refresh On Save` option is enabled and your game window is open, the character's files will immediately reload ingame and you can focus the game menu and test the TAE event changes. Additionally, you can press F5 or click the `File -> Force Refresh Ingame` option to force the character to reload.

## System Requirements:
* Windows 7/8/8.1/10 (64-bit only)
* [Microsoft .NET Framework 4.7.2](https://www.microsoft.com/net/download/thank-you/net472)
* A Direct3D11 Compatible Graphics Device

## Special Thanks
* River Nyxx - General .TAE file structure.
* Pav - Tons of events mapped.

## Libraries Utilized
* [My custom fork of SoulsFormats](https://github.com/Meowmaritus/SoulsFormats)
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* A custom build of MonoGame Framework
