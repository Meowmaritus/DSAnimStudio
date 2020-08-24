## **Changes/Additions**:
* `Help -> Basic Controls` now contains info about scrubbing and multi-select.

## **Fixes**:
* **60 FPS animations no longer have double speed event simulation** (bug introduced in 1.3.1).
* Multiple draw mask change events in a row now simulate in the correct order (from the beginning of the animation to the current time).
* Event simulation frame update now happens after event highlight during playback/scrubbing so the event simulation no longer activates a frame later than the box is highlighted in the graph.