## MEOW V2.0 RELEASE APPROACHING TODO LIST
* [done] Change repeat current anim shit from enter key to something else because pressing enter in inspector smh
* [done] Change combo viewer to cancel the transition on the first anim it plays
* [done] Make combo only update if PlaybackCursor.IsPlaying is true ree
* [done] Implement invalid combo clipboard message.
* [done] Implement invalid combo message(s).
* [done] Check combo viewer on c0000.
* [done] Check if new c0000 equip edit apply changes doing form.Activate() so that you immediately see the 3D update actually works.
* [didnt do this but it seems to behave right now after making it a different form border style?] Make combo menu only AlwaysOnTop if Main.Active
* [done] Put a proper name on combo menu form title
* [removed icon fatcat] Adjust icon for it and stuff
* [done] fix weapons not rendering at all in ds1

* [done] see why c4100 this combo has jitter between 3000 and 3001:
a00_3000|23: End If AI ComboAttack Queued
a00_3001|23: End If AI ComboAttack Queued
a00_3002|23: End If AI ComboAttack Queued


## MEOW TODO LIST

* [DONE] Investigate c0000 - a036_030000 on saint bident - has sphere hitbox that is squished
* [done probably] Make weapon animations clear when no matching one was found instead of just using the last one it found until you manually clear it fatcat
* [DONE] Switch back to using XX00 as the fallback behaviorVariationId for weapons with behaviorVariationId of XXXX rather than wepmotionCategory * 100 
* [not gonna do this lol fatcat] Add hover over popup for playback transport buttons saying the name of the action and the hotkeys
* [done] Fix the model viewer vertical size drag not lining up after adding transport
* [done] Make model viewer default bg color Color.DimGray instead of the same as the transport bg color
* [done] Make transport a bit taller (button size increase as well)
* [done] Do readout like this:
    Frame: 0000.000 / 0
	 Time: 0000.000 / 0.000	 
* [Avoided this by calling orbit cam func] Make camera following root motion rotation a separate float then do (inputtedMouseRotationFloat + rootMotionFloat) to get final euler Y rotation.
* [DONE] Make all anim scrubbing relative. this allows blended animations to have different loop lengths.






## **Changes/Additions**:
* Developer Stuff: The custom fork of SoulsFormats has been completely eliminated from the solution and all Havok animation code has been refactored and separated into [a new project named SFAnimExtensions](https://github.com/Meowmaritus/SFAnimExtensions) which extends the `SoulsFile<TFormat>` class of SoulsFormats to add my fancier XML template TAE class and new Havok related formats.
* Added new option `Tools -> Downgrade Sekiro/DS1R ANIBND(s)...`
  * Converts one or more `*.ANIBND.DCX` file(s) from DS1R or Sekiro into `*.ANIBND.DCX.2010` file(s) that anim studio will then use to display those animations.
  * It's lame and slow as heck (up to multiple minutes per character) but you only have to do this process one time and it opens normally like other games after that. It's better than just not being able to preview these animations!
* Added `Tools -> Combo Viewer`, which lets you view chained animations with proper blending and cancel timings. 
![image of combo viewer menu](https://user-images.githubusercontent.com/3039830/82632924-5710d080-9bbf-11ea-87f3-fdb8e6799b67.png). 
  * The status is shown in the model viewer.
![image of combo viewer status](https://user-images.githubusercontent.com/3039830/82633015-9b9c6c00-9bbf-11ea-8c0c-689d4ebbc22e.png)
  * You can copy to clipboard as text as well as paste from clipboard as text (if the text is valid).
* Enum values now appear on the actual graph intead of only in the hover info box or inspector.
* "Snap to 30 FPS" or whatever option now lets you choose None, 30 FPS, or 60 FPS.
  * This option will now show vertical guidelines where the frames are.
* When you open a DS1R or Sekiro file, it now defaults to `MESH DEBUG: Normals (Mesh Only)` shading mode, since most DS1R/Sekiro files are hard to see with the missing textures and glitchiness.
* Previous/Next animation is now the arrow up/down keys rather than page up / page down. This is actually to avoid a strange bug I'm experiencing where MonoGame will not properly detect Page Down presses ._.
* The end of the animation now has a thick white vertical line in the graph.
* Changed to a playback transport like an audio workstation has. ![Image of new playback transport](https://i.imgur.com/0rrsyVI.png)
* When events are active, their text and outline both renders as yellow.
* When you select events it no longer changes the outline color. Instead, it dims all other events that are not selected, leaving the selected ones as the only ones rendering at normal brightness.
* **Redesigned the layout/functionality of the event graph**: ![Image of new graph style](https://i.imgur.com/Ztryh5i.png)
  * Event text is all rendered with the smaller, far more crisp font.
  * Event boxes are less tall since there's smaller text to fit in them.
  * While scrubbing or playing, the currently active and highlighted events will render on top of other events and show their full arguments list, no matter how small they are.
  * On by default, but disableable. The option is `Event Graph -> Use New Graph Design`
* Starts zoomed out a bit more.
* Removed option for disabling accumulative root motion. Not only do you no longer need it (see next couple of additions) but it also would've been *very* difficult to implement with my complete rewrite of the root motion code to work with my new animation blending and transition system.
* Added option for camera to follow root motion rotation as well (on by default).
* Added option to automatically (and seamlessly) loop the character's motion back to the middle of the graph so they don't walk off the graph (on by default).
  * When I say "seamlessly" I'm talking like Mario 64's endless Bowser stairs but way harder to notice.
* All animations are blended and transition from the last animation selected to the current.
  * You can press Backspace to remove the blend transition currently active and just preview the animation exactly as it is in that single file.
  * You can press Ctrl+Spacebar to re-transition to the current animation.
  * You can press Ctrl+Shift+Spacebar to restart the current animation with no blend transition.
  * The animation filename it's currently transitioning from is shown in the viewport under the current one. 
![image](https://user-images.githubusercontent.com/3039830/82633245-3eed8100-9bc0-11ea-88ee-bc59d0bb15bd.png)
  * When a transition is active, the transition is shown via the names' colors in the animation list. They will fade between gray and yellow, with the amount of yellow representing the weight of the animation.
![image](https://user-images.githubusercontent.com/3039830/82633193-1f565880-9bc0-11ea-8a56-fb59615a256e.png)
* Resizing/moving/removing/adding events or undoing/redoing all update visualizer simulations like hitboxes etc.
* Reset root motion hotkey is now a Hard Reset Current Animation hotkey and it's Ctrl+R rather than R.
* Modifying player equipment now resets to the beginning of the animation due to the way the new code works (otherwise the weapon's animation would start from the beginning while the player animation was wherever you last scrubbed to).
* Animations are now cached to prevent hitching while viewing combos. You can see which animations are loaded into memory based on whether this new square by their name is filled. 
![Image of animation cache indicators](https://i.imgur.com/eiSAU7I.png)
* New option `Simulation -> Simulate SpEffects` . Shows all active SpEffect IDs/names on the current frame in the model viewer window.
![image](https://user-images.githubusercontent.com/3039830/82633105-ecac6000-9bbf-11ea-8ba6-887f3fe09ebe.png)

## **Fixes**:
* No longer crashes if you equip a left hand weapon and no right hand weapon.
* Sekiro NpcParam model masks are now correct.
* A few textures will now load in Sekiro.
* FLVER texture map scales are now read. No idea if it's ever used for characters.
* Fixed player weapons still having root motion when root motion was disabled.
* Fixed some weird inconsistencies with animation looping and frame count.
* Event boxes are no longer visibly rounded to the nearest frame when snapping to frames is enabled.
* Playback event simulation timing is now more accurate.
* Eveything on the graph is better aligned and jitters far less.
* When rapidly switching animations, the graph no longer flickers.
* Can no longer hold down the next and previous anim buttons at the same time.
* Made app update significantly more lazily while in the background. This helps performance when using other apps on your PC.
  * Note: during the ANIBND loading, model loading, texture loading, or ANIBND downgrading processes, the app will still go full throttle in the background so that it doesn't take ages.
* Fixed bug where viewport became tiny when you minimized then un-minimized.
* Fixed typo that made event boxes slightly purplish when highlighted instead of blue.
* The Next Frame / Previous Frame actions (left/right on arrow keys and now in the Playback Transport added in this update) now properly stop playback. They also now snap to the next closest frame in that direction instead of directly going 1.0 frames in either direction (e.g. if you're on frame 3.2 it will now go to frame 4 instead of frame 4.2).
* Animated weapons now automatically switch to idle (animation 0) when a matching animation is not found, rather than remaining on the last animation found.
* Fixed a bug causing hitboxes on certain models such as Dark Souls 3's Saint Bident to be deformed.
* Made weapons with behaviorVariationId `XXXX` fall back to behaviorVariationId `XX00` rather than `(wepmotionCategory * 100)`. I didn't exactly test this much because I'm tired of working on this update at this point. If this doesn't work properly, then prepare for 2.0.1 I guess.
* [Developer Stuff] I'm no longer calling the get input state functions multiple times per frame, which may improve performance a bit if you're running at really high framerates, not really sure.
* You can no longer see the skybox move subtly as the camera leaves the origin.
* Added error handler for weapon model loading.
* You can no longer place a glitched out invalid event inside the timeline by right clicking the timeline.

## **Known Issues**:
* Models that use multiple UV maps only use the first one, resulting in many things looking wrong. This would be an extremely difficult thing to fix requiring lots of new shader code.
* Many things in DS1R/Sekiro are just solid chrome since they are not loading textures properly. Cannot figure out the issue.
* Hotkeys are plentiful and the option in Help sucks. I'm oing to eventually update the README.md to include all hotkeys.
* With the new rewrite of most time and root motion related code, sometimes root motion can get confused with rotations incolved and start sliding the wrong direction. Press Ctrl+R to hard reset the current animation to fix it, at the cost of cancelling the current transition if you were previewing one.
* Sometimes animation blending can get slightly confused and play weirdly. Press Ctrl+R to hard reset the current animation to fix it, at the cost of cancelling the current transition if you were previewing one.
* Sometimes when going from c0000 of one game to c0000 to another game, the equipment menu will stay open with the values from the other game. Don't try to use it, just close it then choose the menu option to open it again.
* Disabling the `Simulation -> Simulate Basic Blending` just breaks animation blending and doesn't properly disable it. Do not disable this option.







