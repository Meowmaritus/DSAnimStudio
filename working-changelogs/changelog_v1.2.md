## **Changes/Additions**:
* Animation names now display. The strings such as "a000_000000.hkt" are actually likely just the display names From Software used in development as they do absolutely nothing ingame. Because of this, they are a good place to store animation names as you identify them. You can edit the names with `Edit -> Set Animation Name...` or by pressing F2. You can still edit animation names using the menu that the `Edit Anim Info...` button brings up.
* `Animation -> Set Playback Speed...` now shows current playback speed to the right of it.
  * Eventually there will be a "transport control" update which adds a play/pause/stop button cluster along with current animation 
* Various popup menus no longer stop animation from playing while open.
* Some menus slightly rearranged/renamed to make clearer.
* Made `3D Viewport -> Slow Light Spin` way slower
* **Menus no longer close upon clicking an item**, making toggling multiple things easier.
* Little information text at the top of the event graph is now more readable and includes the AnimFileReference value.

## **Fixes**:
* Hitting Cancel on the `Animation -> Set Playback Speed...` no longer complains about an invalid value.
* Old model menu items no longer sometimes do not get deleted in the background, which was causing the world's slowest memory leak, adding under 1 KB of memory for each model loaded.
* Old menu items no longer sometimes not update the lookup table, which was making some UI elements not respond correctly.
* The options for changing draw masks in scene -> current model now set the default reference draw mask instead of the current visible draw mask, making it not just get overrided by draw mask event simulation.
* No longer displays the names of every hitbox's attack ID at the origin upon loading a model, before switching animations or playing the current animation.
* Animated model is no longer in a pose from the wrong animation upon selecting a new animation and before playing/scrubbing the animation.
* Ghost playback cursor which shows where the current animation began is now properly moved to the beginning when changing animations.
* When an animation file is not found, there is an indication on the current animation file name indicator in the viewport.
* When an animation file is not found, the animated model no longer retains the last pose it was in but rather reverts to reference pose.
* Player weapon models no longer jitter when the player's hands move quickly.

## **Future Plans**:
* Eventually I want to add a transport control, which would act like the transport from an audio/video workstation with:
  * Play/pause button
  * Stop button
  * Go to start button
  * Go to end button
  * Current time in seconds and frames
  * Duration in seconds and frames
  * Current playback speed
  * Current animation framerate defined in HKX