

## Changes/Additions (v2.4.1):
* You can make the viewport bigger than before.
* Animation list a bit more compact, can also be made smaller than before, giving more room for graph and viewport.
* Middle clicking to reset the camera will now make it follow the character's lockon dummypoly automatically from that point onward, unless you right click and drag to move the pivot point.
* New option to snap to 45 degree angles, located in the Toolbox.
* Option to set raw mouse speed has actually been inplemented this time. I forgot it in 2.4.0.
* Toolbox can now be dragged around.
* Toolbox contents can be scaled with 2 new options at the top.
* Viewport text (like weapon animations etc) can now be rescaled with a new bar near the top of Toolbox.

## Fixes (From 2.4 regressions):
* Hotkeys now work after giving model viewer or inspector focus (aside from undo/redo/copy/paste/select all)
* Viewport can now be dragged again without the app freaking out.
* Minimizing no longer makes the model viewer tiny after restoring the window.

## Fixes (From 2.3 and earlier):
* Keyboard input in Toolbox no longer permanently stops working after loading a character, alt-tabbing, and various other actions.
  * You likely didn't realize it had keyboard input since it didn't work. Ctrl + Left Click on a slider value to do keyboard input for that value.