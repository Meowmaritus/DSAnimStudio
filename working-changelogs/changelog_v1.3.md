## **Changes/Additions**:
* Added "\<CurrentFrame\>/\<MaxFrame\>" readout on timeline next to playback cursor, similar to 3DS Max
* Removed smoothing on auto scroll during playback. It was only really added originally so you didn't notice the misalignment and jitter, which I fixed in this version.
* Timeline always moves smoothly while playing, even when `Animation -> Lock to Frame Rate Defined in HKX` is enabled.
  * Still snaps to exact frame positions while scrubbing or doing previous/next frame.
* Individual frame labels when zoomed in are now white instead of cyan.
* Bone limit extended from 1,275 to 1,530.
* Added warning before current animation name displayed in the viewport if model exceeds the max bone count.
* Added animation framerate after the end of the current animation name displayed in the viewport.

## **Fixes**:
* **The helper toggles etc. in the Scene menu are no longer deleted when that menu opens**.
* **Various models are no longer invisible** (mainly affects DS1 weapons and objects, but theoretically could fix other types of models in other games as well). The bug was caused by From Software not defining bone weights on some objects and my code dividing by 0 while trying to skin the mesh.
* **No longer occasionally fails to change the 3D model's animation when an animation is selected in the graph**.
* **Zooming in/out with Ctrl + scroll wheel is now properly centered on the mouse cursor horizontal offset** (this bug was *immensely* irritating in my experience).
* Current animation name texts in 3D viewport now show the name of the actual animation file playing, rather than the animation file it is *attempting* to play.
* Animations now transition completely correctly between "blocks" (One "block" in a Havok animation is 255 frames long. So you would previously see a weird jitter somewhere around frame 255, frame 511, etc).
* The seconds display on the timeline now disappears if you zoom in close enough to see individual frame numbers instead of overlapping.
* Added text to `Help -> Basic Controls` about how to zoom in/out in the timeline.
* Ctrl+(+/-) on numpad now zooms as well.
* Added text to `Help -> Basic Controls` about previous/next frame keys.
* Previous/next frame keys now function properly while `Animation -> Lock to Frame Rate Defined in HKX` is enabled.
* Previous/next frame keys now update the current playback start position ghost line thing.
* Playback auto scroll is now updated before the current frame is drawn, preventing misalignment and jitter.
* Fixed a rare crash due to a race condition with one thread trying to display the current animation ID text on the viewport before the other thread was done loading the animations.
* The onscreen size of DummyPoly ID text and hitbox attack ID text no longer scales inversly with `3D Viewport -> Resolution Multiplier` (the spritefont pixel size now is multiplied by resolution multiplier to counteract the inverse scale)
* The placeholder DS1 PTDE shader is back to being the same code as the DS3 shader like it was supposed to be rather than some half broken code that made it look really washed out.
* No longer crashes if current model exceeds bone limit.