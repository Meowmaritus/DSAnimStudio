## **Changes/Additions**:
* New Option: `Player Settings -> Behavior / Hitbox Source`. Allows you to choose whether behaviors and hitboxes come from player body, right weapon, or left weapon, by default (things like paired weapons will still automatically pick the correct hand for you if you leave it set to right hand mode probably).
* HideEquippedWeapon events how hide your equipped weapons if `Simulation -> Simulate Draw Mask Changes` is enabled.
* Animation names are now gold.
* The box that highlights the currently selected animation in the list is now slightly larger and a deeper blue color.
* Made DummyPoly helper orange and use a **new arrow graphic** to show orientation:
    * Long 3D arrow = forward
    * Long perpendicular stem = up
    * Short perpendicular stem = right
* SFX and bullets that spawn on DummyPoly now show the 3D arrow indicators (same as the ones DummyPoly helper now uses) with 2 new toggleable options:
  * `Simulation -> Simulate SFX Spawns`
  * `Simulation -> Simulate Bullet Spawns`
* **(Rewrote lots of various graphics code and adjusted visual aesthetics of various things)**.
* Root motion is now toggleable with `Animation -> Enable Root Motion`
* Camera following root motion is now toggleable with `Animation -> Camera Follows Root Motion`
* Added `3D Viewport -> Override FLVER Shading Mode -> MESH DEBUG: Vertex Color Alpha`
* Added `3D Viewport -> Disable Texture Blending` option, which disables texture blending and uses just the first texture.
* Backups are now saved as \*.dsasbak (existing \*.taedxbak backups will still be there, but the app will see that there is no \*.dsasbak and make one based on the current file, so just be aware of that).
* The X == 0 and Z == 0 lines of the grid are now brighter to let you know that where they converge is the origin.
* Most of the DummyPoly management and hitbox management code has been rewritten, giving more accurate hit spheres/capsules in some edge cases .
* Reworked Home button in general:
  * Home = go to current start point (does not stop playback)
  * Shift+Home = go to frame 0 (does not stop playback)
  * Ctrl+Home = go to current start point (stops playback)
  * Ctrl+Shift+Home = go to frame 0 (stops playback)
* Reworked End button in general:
  * End = go to last frame of animation (does not stop playback)
  * Ctrl+End = go to last frame of animation (stops playback)
* Find window's Event Name and Event Type Num search results now include the event box's full text, showing parameters and such, in the "Matched Value" column.
* Length of bones with `Scene -> Helper: FLVER Skeleton (Cyan)` enabled now have more accurate lengths.
* Current left/right weapons' movesets appears in overlay over 3D viewport, next to current weapons' animations.
* You can now manually save the editor's configuration file with `File -> Manually Save Config` (usually it only saves upon exiting).
* AtkParam row names are now displayed on hitboxes next to the AtkParam row IDs (row name gets cut off after 32 characters to prevent screen clutter).

## **Fixes**:
* `3D Viewport -> Override FLVER Shading Mode -> TEX DEBUG: Diffuse/Albedo Map` now obeys the `3D Viewport -> Disable Texture Alphas` option.
* Attacks which spawn spheres on a DummyPoly ID now spawn it on all instances of that DummyPoly ID
* Fixed Dancer of the Boreal Valley facing to the right of the screen by default and sliding sideways during attacks (this may fix any other characters with the same issue, but I haven't encountered it on any other character personally).
* Perfectly mathematically vertical (both points having the exact same X and Z coordinates but a different Y coordinate) hitbox capsules no longer disappear due to a matrix calculation resulting in NaN (none of the ingame animations were ever precise enough to trigger this issue before, but in this version with the rewritten hitbox system, weapon hit capsules are in local weapon mesh space, meaning swords have perfectly vertical hit capsules, thus allowing me to notice this issue).
* Camera no now also considers how long a model is instead of just how tall it is when deciding the default zoom level (fixes camera starting inside Wolnir's chest and potentially others).
* No longer loads chalice dungeon variants of Bloodborne enemies, which were cluttering the list and making it take upwards of 2 minutes to load the character. If no non-chalice dungeon variant is found, it will load the first chalice dungeon variant, then stop; just a failsafe so it has an NpcParam to work with.
* Toggling an option in the `Simulation` menu now properly toggles the effects of the simulation and syncs with the current frame.
* Checking the various flip checkboxes in player equip change menu no longer requires you to click back onto the main window to apply changes. It works like several versions ago.
* UI layout no longer appears disheveled if the window isn't in focus as it opens.
* `File -> Recent Files -> Clear All Recent Files...` no longer fails to update the list in the menu.
* Text which appears on a location in 3D space in the viewport is no longer very dark and hard to see color (it was actually a rendering bug).











[[[[[[[[[[[[DELETE ME]]]]]]]]]]]]
[pretty much does] Make sure player hitbox selection works now!

[working now] c5160 a001_003002 hitbox not working 

[does, but doesnt update player weapons until after all equipment is loaded] do weapon after anim update after loading model and going to first frame of anim

[does] make sure draw skeleton works
[no] add draw havok skeleton
[not yet] add dummypoly draw back
[not yet] add attack id text draw back



* [NOT YET] No longer crashes when opening Sekiro files (**Note: Sekiro's animations are still not supported; only event editing currently.**)





