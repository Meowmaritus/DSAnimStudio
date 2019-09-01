## About DS Anim Studio:
* Edits the TimeAct Editor files of Dark Souls, Bloodborne, Dark Souls 3, and Sekiro (model viewer does not work for Sekiro due to massively different animation containers). These files control **everything** that happens on a specific frame of an animation, such as:
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
* Edits the .anibnd or .anibnd.dcx files of the games directly. **No need to use BND rebuilders.**

## User Instructions:
  1. Run application.
  1. Go to File -> Open
  1. Load a .anibnd or .anibnd.dcx.
  1. Select an animation ID on the left pane
  1. Drag some events around or otherwise mess with things (try the bottom-right pane for editing the highlighted event)
  1. (Optional) Play with the model viewer pane on the top-right.
  1. (Optional) Play with the options in the menu bar.
  1. Hit Ctrl+S to save.

## Breakdown of Game Support:
| Game                                        | Edit Support | Events Mapped / Identified | Anim Viewer Support  |
| ---                                         | ---          | ---                        | ---                  |
| **Dark Souls: Prepare to Die Edition**      | **Yes**      | No (Coming Soon)           | **Yes**              |
| **Dark Souls Remastered**                   | **Yes**      | No (Coming Soon)           | No (Not Soon)        |
| **Dark Souls II**                           | No           | No                         | No (Never)           |
| **Dark Souls II: Scholar of the First Sin** | **Yes**      | No                         | No (Never)           |
| **Dark Souls III**                          | **Yes**      | **Yes**                    | **Yes**              |
| **Bloodborne**                              | **Yes**      | No                         | **Yes**              |
| **Sekiro: Shadows Die Twice**               | **Yes**      | **Yes**                    | No (Not Soon)        |

## System Requirements:
* Windows 7/8/8.1/10 (64-bit only)
* [Microsoft .NET Framework 4.7.2](https://www.microsoft.com/net/download/thank-you/net472)
* **A DirectX 11 Compatible Graphics Device**, even if you're just modding DS1: PTDE

## Special Thanks
* River Nyxx - General .TAE file structure.
* TKGP - Made some discoveries about the TAE format.
* Pav - Tons of TimeAct events mapped.
* [Katalash](https://github.com/katalash) - Much help with animation file understanding.
* [PredatorCZ](https://github.com/PredatorCZ) - Reverse engineered Spline-Compressed Animation entirely.

## Libraries Utilized
* [My custom fork of SoulsFormats](https://github.com/Meowmaritus/SoulsFormats)
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* A custom build of MonoGame Framework by Katalash fixing some of the shitty limitations
* A small portion of [HavokLib](https://github.com/PredatorCZ/HavokLib), specifically the spline-compressed animation decompressor, adapted for C#