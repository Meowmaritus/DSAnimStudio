## **Changes/Additions**:
* Hovering over an event for a brief moment now shows a tooltip by your mouse cursor with detailed information about the event.
  * Functionality can be toggled with `Event Graph -> Show Event Info Popup When Hovering Over Event`
* Many visual improvements:
  * Better text alignment.
  * Rows now have gaps between them.
  * Horizontal row lines are now aligned better.
  * Main font now slightly bolder.
  * Extremely tiny events now display their name instead of a number, and also use a new extremely crisp font instead of the blurry mess they were before.
  * Spawn texts in viewport now use the same super crisp small font.
  * Selection rectangle now electric blue.
  * Event boxes now dark gray when not highlighted and deep electric blue while highlighted.
  * Animation list entries are slightly taller.
  * Scrollbar arrows are nicer.
  * When you zoom in enough to see individual frame numbers on the timeline, they are using the new crisp small font.
  * (Maybe other things I forgot.)
* Hovering over an event now activates that event temporarily, highlighting it and interacting with event simulations. This feature may help when there are several event simulations happening at once and you'd like to single each out.
  * Functionality can be toggled with `Event Graph -> Solo Highlight Event on Hover`
  * Does not work while animation is playing.
* Holding middle click and dragging on the graph now scrolls the graph.
* Made SpawnFFX_ChrType event not show all the parameters on the 3D-space text so it's slightly less obnoxious (for simulate misc DummyPoly spawns).
* "Ghost" playback cursor for starting point is now a solid blue line.
* A "Ghost" playback cursor now appears on the corresponding frame of the first loop if you go past the end of the animation to additional loops.
* Event boxes which start to the left of the screen now have `"<-"` as the prefix on the label instead of `"< "`
* DummyPoly ID 200 (which is all over the surface of the character) now only display a single text on the first 200 DummyPoly instead of cluttering the screen with text on every single point.
* Extremely tiny events now display their name instead of a number

## **Fixes**:
* Paired weapon attacks in DS3 that activate hitboxes on both weapons simultaneously no longer only activate on just one of them.
* Characters who have their textures stored in cXXX9.chrbnd[.dcx] or cXXX9.texbnd.dcx now automatically have their textures loaded, like the older versions of DS Anim Studio.
* Scrubbing the timeline past the end of the animation now properly plays events looped.
* Fixed InvokeCommonBehavior on NPCs not working.
* Fixed a couple of sound event types not showing up with misc DummyPoly spawn simulation enabled.
* No longer counts as hovering over boxes on the top of the screen when you're hovering over the timeline area.
* Help now explains the Home/End keys.
* Dragging a selection rectangle in the graph then releasing the mouse button while the mouse is outside the graph area no longer causes the selection rectangle to stay visible until the mouse is back inside the graph area.
* Event box horizontal position and width are now rounded to the nearest pixel preventing slight misalignment while resizing several at once while zoomed quite far out.
* Event box text no longer sometimes moves one pixel up/down at certain zoom levels.
* Individual frame numbers on timeline (when you zoom in far enough) are now represented in the animation's actual framerate, not 30 FPS always.



















[[[[NOTES]]]]
* Event boxes grayscale when not PlaybackHighlight:
    non-selected = new Color(80, 80, 80, 255);
    selected = new Color(130, 130, 130, 255)
* boxes always have white outlines, 1 pixel thin
* add pixel or 2 of spacing between rows
* when box is PlaybackHighlight:
    non-selected = new Color((30.0f / 255.0f) * 1, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);
    selected = Color.DodgerBlue




非常に悪いカメラピッチ制限リミッタTestFatCat

public static void Main(string[] args)
{
    var fatcat = new Fatcat();
}


