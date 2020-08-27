## PROBABLY GETTING CUT:
* [NOT YET] Right clicking and dragging to move the camera now moves the same relative speed regardless of camera distance from pivot point.
* [NOT YET] New option in `Viewport Config` pane under `DISPLAY`: `Show Camera Pivot Point`, which is self explanatory.
* [NOT YET] New option in `Viewport Config` pane under `DISPLAY`: `Show Grid Origin`, which should be self explanatory.
* [NOT YET] New option `Animation -> Show Root Motion Path`, which renders the root motion path as a line.




## Changes/Additions:
* `Viewport Config` now renamed to `Toolbox`, but to avoid confusion, changes in this log related to it still say `Viewport Config`.
* DummyPoly ID text is now closer to the actual location 
* DummyPoly 200 shows regular ID text on all points instead of just one that says "(all over body)"
* New DummyPoly menu in `Viewport Config`. Has a list of DummyPoly for each model. Hovering over them shows just that DummyPoly in the viewport allowing you to tell where it is.
  * Click a DummyPoly to toggle it on/off. If off nothing related to it will show. Even SFX spawns etc will not show from that DummyPoly. The only exception is hitbox shapes which will always show entirely with hitbox simulation enabled.
* Some helper colors changed around.
* [NOT YET] New option `Scene -> Show c0000 Weapon Global DummyPoly ID Values (10000+)`, which displays the globally-offsetted values you would use for paired weapon / trick weapon AtkParams etc.
* Changed `Animation -> Lock to Frame Rate Defined in HKX` to use Math.Floor instead of Math.Round, which works more accurately with events during playback.
* `Scene -> Helper: Flver Skeleton` is now yellow and renders all bones with the appropriate orientation/length. Due to technical limitations it now displays as just a single line per bone like the debug menu option in the actual game engine.
* Animation blending now obeys the "lock to framerate defined in HKX" option.
* Manually selecting an animation either by clicking or my pressing Up or Down now cancels the currently playing combo if applicable.
* `PlaySound_ByStateInfo` event will now stop the sound when the event is no longer active. This allows it to fade out some looping sounds etc and works like it does ingame.
* Grid and hitbox color are now the originally intended duller colors that match the ingame debug colors.
* New section in viewport config window named COLORS. Allows you to adjust grid color, hitbox colors, various helper colors, etc.
* New render modes to debug mesh UV coordinates, with an option for TEXCOORD0 and another for TEXCOORD1.
* Now loads human body and face for the player kinda. Does not look very good.
* Missing diffuse textures are now gray instead of white, making things easier to see.
* Missing specular textures are now black instead of white, making things easier to see.
* New option `Simulation -> Simulate Character Tracking (A/D Keys)`
  * Preview character rotation speed.
  * Takes JumpTable 7 (Disable Turning) into account.
  * Takes Event 224 (SetTurnSpeed) into account.
  * Apply an automatic constant turn speed right at the top of `Viewport Config`.
* Volume bar now displays in percent instead of multiplier.
* Volume bar maximum increased to 200%
* New "Reset to 100%" button under volume.
* Some new separators added to viewport config imgui menu for increased readability.
* Made memory usage text on bottom right smaller.
* You now have to zoom in less to see individual frame numbers.
* Camera default orbit point makes more sense on more models
* [not finished] Camera will by default follow dummypoly 240, or if that doesn't exist, 220
* Lighting direction can now be set while lighting is set to follow camera, giving a sort of backlight effect etc.
  * New default has model lit from the side, giving a more dramatic look.
* [not yet] oading a new character now force stops all sound output (you can't manually end sound output until after it finishes loading the new character, so that was really annoying)
* Middle clicking to recenter the camera no longer resets the zoom.

## Fixes:
* [NOT YET] Spawn events that use the global weapon model offsets (10000+) now display correctly.
* Fixed a bug where weapon locations displayed in the location of the previous animation frame for 1 render frame after changing animation frames before suddenly snapping to the right location. This was extremely noticable for very drastic location changes such as Bloodborne trick weapon transformation animations, where you would see the weapon in an extremely incorrect location for 1 frame and it appeared very jarring.
* [NOT YET] Fixed bug where hitting Insert key to insert a new animation would scroll to a weird place in the animation list.
* Weapon animations now reset to frame 0 when the player's animation loops (useful for mods that are using a weapon animation that's longer than the player's).
* Fixed a bug from I dunno how long ago where model masks on player equipment were not ever applied.
* Zooming the graph way out no longer makes the timeline blank on top.
* Middle clicking to recenter camera now goes to the proper rotation if the model is root motion rotated and camera follows root motion rotation is on
* Fixed longstanding bug where playing an animation with rotational root motion then blending into an animation with lateral root motion would result in the model moving in a different direction than it is facing (sliding sideways on dashes etc)
* Fixed Edit Animation Properties being cutoff if you are using an abnormal windows DPI scale factor. Note that you must still do this if you use DPI scaling:
  * Right click `DS Anim Studio.exe`
  * Click `Properties`
  * Go to `Compatibility` tab
  * Go to `Change high DPI settings`
  * Check both checkboxes
  * Set lower scaling override dropdown to `Application`
  * Hit OK
  * Hit Apply 
  * Hit OK 
  * Close any running instance of DS Anim Studio and launch it again
* Sekiro Edit Animation Properties window no longer shows short animation ID format (aXX_YYYY) like DS1.

meowtodo 
* make ctrl+insert to a duplicate where it imports hkx from the current anim and has all events pasted
* [done] add toggle all dummypoly, show all dummypoly, hide all dummypoly
* [done] sort dummypoly by type 
* [done] make dummypoly list hover override override all spawn draws and disable hitbox draws etc 
* [meh] make flver skeleton fuchsia
* [kinda done?] change ds1 shading to same as ds3 except gloss is total value of specular map rgb squared or 1.25 power
* Make camera move with right click drag have a minimum speed when up close to prevent it from feeling hard to move 
* add to log
  [done]color pick 
  [done]new darker coolors
  [done]plysound_bystateinfo
* animated armor
* bug: time on frame 0 says " .0000" instead of "0.0000" or whatever
* [implented] change event type dialogue now copies over the bytes of the last event (basically i made this solely for quickly swapping between spawn ffx event types with same args)
* going to next anim in combo is now checked BEFORE events are checked so events after the combo cancel frame should not trigger the same frame it switches animations (ds1 seems to do it like this afaik)

direct mult 0.65
indirect mult 0.65
contrast 0.6
light h 1.8