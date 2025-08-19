## Basic User Instructions (Unpacked Game):
  1. Run application.
  1. Go to `File`->`Open`
  1. Load a .anibnd or .anibnd.dcx.
  1. Enter the information required, such as game executable, ModEngine folder (if applicable), etc.
  1. Select where the unpacked ANIBND should be saved (the "project directory").
  1. Select an animation ID on the left pane
  1. Drag some events around or otherwise mess with things (try the bottom-right pane for editing the highlighted event)
  1. (Optional) Play with the model/animation viewer pane on the top-right.
  1. (Optional) Play with the options in the menu bar.
  1. Hit Ctrl+S to save.
  1. Make sure your game is setup to load the edited loose ANIBND file. This is more of a general Souls modding thing that is too specific to cover here.

## Basic User Instructions (Packed Game):
  1. Run application.
  1. Go to `File`->`Open From Packed Game Data Archives...`
  1. Enter the information required, such as game executable, ModEngine folder (if applicable), etc.
  1. Select which ANIBND/CHRBND you would like to use.
  1. Select where the unpacked ANIBND should be saved (the "project directory").
  1. Select an animation ID on the left pane
  1. Drag some events around or otherwise mess with things (try the bottom-right pane for editing the highlighted event)
  1. (Optional) Play with the model/animation viewer pane on the top-right.
  1. (Optional) Play with the options in the menu bar.
  1. Hit Ctrl+S to save.
  1. Make sure your game is setup to load the edited loose ANIBND file. This is more of a general Souls modding thing that is too specific to cover here.
  
## About DS Anim Studio:
* Edits the TimeAct Editor files of Dark Souls, Bloodborne, Dark Souls 3, and Sekiro. These files control **everything** that happens on a specific frame of an animation, such as:
  * Activating invulnerability frames.
  * Parry windows
  * Applying an "SpEffect" (special temporary statuses such as ring effects, poisoning, buffs, AI triggers, etc)
  * Allowing animation cancelling
  * Setting the flag for YOU DIED and respawning
  * Creating "SFX" / "FFX" (both refer to the exact same files: visual effects)
  * Playing sound effects such as footsteps, sword swooshes, etc.
  * Invoking an attack behavior (does damage to opponent, drains stamina from player, etc all in one event)
  * Invoking a "bullet" (projectile) behavior (fires projectile, drains stamina from player, etc all in one event)
  * Invoking a "common" behavior (like attack behaviors but for simpler things such as falling on someone's head causing stagger)
  * Creating motion blur on weapon swings
  * Setting the opacity of a character (used for getting summoned into other worlds, dying, etc)
  * Setting attack aim tracking speed of a character
  * Playing a "RumbleCam" file (relative screen movement e.g. Smough's footsteps shaking screen)
  * Playing additional animation layers (e.g. all of Gwyn's animations have events to play his clothes-blowing-in-wind animation layered on top of the other animations)
  * Adjusting model render masks (showing/hiding specific parts of characters)
  * Many more that we haven't even figured out yet.
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
* Edits the .anibnd or .anibnd.dcx files of the games directly. No need to use BND rebuilders.
  

## Breakdown of Game Support:
| Game                                        | Edit Support | Events Mapped / Identified | Anim Viewer Support  |
| ---                                         | ---          | ---                        | ---                  |
| **Dark Souls: Prepare to Die Edition**      | **Yes**      | Lots of them               | **Yes**              |
| **Dark Souls Remastered**                   | **Yes**      | Lots of them               | Sort of\*...         |
| **Dark Souls II**                           | No           | No                         | No (Never)           |
| **Dark Souls II: Scholar of the First Sin** | No (may change in the future) | No        | No (Never)           |
| **Dark Souls III**                          | **Yes**      | Lots of them               | **Yes**              |
| **Bloodborne**                              | **Yes**      | Lots of them               | **Yes**              |
| **Sekiro: Shadows Die Twice**               | **Yes**      | Lots of them               | **Yes**              |
| **Elden Ring**                              | **Yes**      | Lots of them               | **Yes**              |
\* For DS1R/Sekiro/Elden Ring, an animation downgrade process is no longer needed.

## System Requirements (End Users):
* Windows 7 SP1/8.1/10/11 (64-bit only)
* [.NET Desktop Runtime 6.0 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.12-windows-x64-installer)
* **A DirectX 11 Compatible Graphics Device**, even if you're just modding DS1: PTDE
  * Note: even though Elden Ring uses DirectX 12, DS Anim Studio still uses DirectX 11.
  





## System Requirements (Developers):
* Windows 7 SP1/8.1/10/11 (64-bit only)
* Visual Studio 2022
* [.NET 6.0 SDK (Windows x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.404-windows-x64-installer)
* **A DirectX 11 Compatible Graphics Device**, even if you're just modding DS1: PTDE
  * Note: even though Elden Ring uses DirectX 12, DS Anim Studio still uses DirectX 11.

## Developer Instructions:
* Clone repository (use `git clone --recursive`).
* Ensure that all NuGet packages are installed (should in theory do it on its own now).
* `Build Solution` should succeed.




## Special Thanks
* [TKGP](https://github.com/JKAnderson) - Made some discoveries about the TAE format + made SoulsFormats, which this application depends on.
* [Pav](https://github.com/JohrnaJohrna) - Tons and tons of TAE events mapped across all the games.
* [Katalash](https://github.com/katalash) - Much help with animation file understanding.
* [PredatorCZ](https://github.com/PredatorCZ) - Reverse engineered Havok Spline-Compressed Animation entirely.
* [Horkrux](https://github.com/horkrux) - Reverse engineered the header and swizzling used on non-PC platform textures.
* StaydMcButtermuffin - Many hours of helping me write and debug the shaders + reversing some basic Dark Souls 3 shaders to aid in the process. He also helped me understand Sekiro and Elden Ring shaders for the new versions of the application.

## Libraries Utilized
* [SoulsAssetPipeline](https://github.com/Meowmaritus/SoulsAssetPipeline)
* [SoulsFormatsNEXT](https://github.com/soulsmods/SoulsFormatsNEXT)
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* An edited version of MonoGame Framework where I added support for newer texture types.
* A small portion of [HavokLib](https://github.com/PredatorCZ/HavokLib), specifically the spline-compressed animation decompressor, adapted for C#
* A small portion of [Horkrux's copy of my fork of Wulf's BND Rebuilder](https://github.com/horkrux/DeS-BNDBuild), specifically the headerization and deswizzling of PS4 and PS3 textures, adapted for C# and modified to load the texture directly into MonoGame instead of save to a file.