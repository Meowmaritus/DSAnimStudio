##MEOW TODO
Make Sound menu get cleared
move Sound menu to the right of Animation
remove "FEVs Loaded" thing


## Changes/Additions:
* Added full sound effect support for PTDE, DS1R, DS3, and Sekiro, which is on by default.
  * Disable `Simulation -> Simulate Sound Effects` to disable it entirely.
  * You can find the volume slider under `Viewport Config` in the top right of the viewport, where the render options are.
  * If you accidentally scrub over lots of loud overlapping sounds and it's painfully loud, click `Sound -> [STOP ALL]` to immediately cancel every single sound effect playing. Pressing the Escape key or giving another window focus will also immediately stop all audio playback.
  * You can select the player armor material and floor material in the Sound menu (changes which sounds play for some things).
* Selecting a new animation will only blend to it if you're currently playing an animation.

## Past changes that I forgot to mention previously:
* You can pan the event graph my middle clicking and dragging.
  
## Fixes:
* Fixed an oversight that caused severe framedrops (down to ~15 FPS) when opening c0000 without collapsing all TAE sections.
* Combo viewer now works for BB and Sekiro.
* Middle clicking to reset the camera direction now properly goes to the front of a character who has been rotated via rotational root motion.
* Fixed bug where combo viewer and Goto Anim ID screen wouldn't recognize an animation of an enemy which was a01/a001 or higher.
* Fixed lots of misc. bugs to make it more stable (I'm not purposefully being vague I literally can't remember *what* I fixed, just that it was necessary).


2.1.2 dzsgfdsgdfsgdf

## Changes/Additions:
* Pressing spacebar to play/pause is now activated on key press rather than key release, making it feel as responsive as it was before v2.0

## Fixes:
* Fixed Bloodborne files not loading (they still have no audio support due to atrac9 audio decoding requiring licensed software)
* Fixed middle click to reset camera attempting to compensate for rotational root motion when `Camera Follows Root Motion Rotation` is disabled.
* Fixed a race condition causing a crash during file loading when the 3D model hasn't loaded yet and the animation list tries to check if animations have loaded for the □ and ■ symbols.
* Added crash handlers that should hopefully show an error popup and log to a file so you can post them.
* Fixed oversight that prevented the right skybox being selected by default when running for the first time.
* Fixed oversight that caused skybox to rotate on a different axis than the camera.



######## 2.1.3 ########

## Changes/Additions

* You can now right click a sound event box to preview that sound.
* Placing new events is now done with Shift+Right Click to prevent you from accidentally inserting events while previewing sound effects.
* Added new option `Scene -> Helper: Sound Event Locations (Lime)`
* Made `Simulation -> Simulate Misc. DummyPoly Spawn Events` no longer show play sound events.
* Optimized event simulations a whole lot, reducing the amount of cpu cycles per frame by probably 90%.
* You can now still toggle meshes in the model menu while their assigned mask is currently hidden.
* Very minor event mapping improvements. Someday I'll get around to doing larger updates as more info is found. For now, document everything on the wiki.

## Fixes
* Fully fixed all 3D sound panning.
* Made it actually default to the new graph visual style like I intended.
* Fixed sound effects which spawn from specific DummyPoly not taking root motion into account. Now all sounds seem to pan correctly.
* Fixed the blue vertical line (which indicates where playback last started from) not following vertical scroll if you scrolled down.
* **Properly** fixed a race condition causing a crash during file loading when the 3D model hasn't loaded yet and the animation list tries to check if animations have loaded for the □ and ■ symbols.
* Made "Go To Event Source" button properly vertically aligned.
* Made it where disabling `Simulation -> Simulate Basic Blending` fully disables blending and renamed it to `Simulation -> Simulate Animation Blending`.
  * Also made a separate option `Simulation -> Simulate Animation Blending (Combo Viewer)` which can be enabled while the other is disabled to have animation blending only while playing a combo if you wanna do that.
* Fixed a regression in v2.0 where hiding/showing model masks or meshes wouldn't update until you clicked off the menu.
* Changed "Model XX" to "Mesh XX" in the model menu, which is what I meant to call it originally (which is why the other options said "Hide All *Meshes*" and "Show All *Meshes*").
* Fixed app not rendering while window is not in focus and a file is loading (it does not stop updating while out of focus if it's in the process of loading a file, so this was misleading and never the intended behavior).
* Fixed sounds not loading for enemies which have other character IDs for the event group name (e.g. c2140's sounds are in a group named `c3270` for some reason).


single frame blend
toggling masks

dhfgdhgfdhgfdhfgddddddddddddddddddddddddd
future


* [NOT YET] Added new checkbox to play phantom versions of sounds


