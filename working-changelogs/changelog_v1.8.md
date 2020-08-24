## **Changes/Additions**:
* Dark Souls 3 shading now a bit more accurate. ![DS3 Shading](https://i.imgur.com/mvcy9cr.jpg)
* Bloodborne shading is now far more accurate. ![Bloodborne Shading](https://i.imgur.com/instzwi.jpg)
* Dark Souls: Remastered is **still not supported** but you can now open files from it without the app crashing. 
  * Event editing fully functional.
  * Model will be T-posed at origin instead of animating. 
  * Textures and shaders will be missing/wrong.
  * ![Image of DS1R model loaded.](https://i.imgur.com/GXJPxmX.png)
* Added `Animation` -> `Accumulate Root Motion` option. Makes character root motion not reset to origin when animation loops (only valid if root motion is enabled).
  * Press R to move the root motion back to where it would normally be if the accumulation was disabled.
* Removed "[Shader Config]" menu thing because it was really ugly and hard to add new stuff to.
* Added new on-screen display with **many** existing and new configuration options. 
  * Click the arrow next to "Viewport Config" on the top right of the screen to un-collapse the menu. 
  * Click anywhere off the menu to return to controlling the camera with the menu still there but transparent. 
  * Click the arrow again to collapse the menu.
  * This menu is extremely easy for me to add new options to, so suggestions are welcome.
* Removed "3D Viewport" menu entry as all of its options are available in the new "Viewport Config" menu.

## **Fixes**:
* Fixed occasional GUI threading crash when loading characters and resizing window at same time.
* Fixed occasional sudden cross threading crash (caused by modifying Graph.sortedByRow while in the middle of drawing from that collection), or at least I'm pretty sure I fixed it.
* [SoulsFormats Fork] Fixed crash if your TAE animation properties were invalid (only possible to make invalid using older versions so I didn't think of this until now).
* Fixed oversight where toggling texture alphas didn't function if the model didn't utilize 2 texture blending.
* Camera no longer moves/turns slower at higher framerates and faster at low framerates.



finish overlay descriptions
finish accumulative root motion reset shit



