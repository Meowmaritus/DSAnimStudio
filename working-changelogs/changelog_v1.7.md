## **Changes/Additions**:
* Events imported from other animations are now shown as "ghosts" when in use.
  * The events in the currently selected animation (not the one the "ghosts" are from) are ignored by the game and as such are not shown or editable while an import is active. To see and/or edit them, remove the import in the animation properties.
* "Edit Anim Info..." is now accessible with F3 and renamed accordingly.
* Escape now closes the Edit Anim Info screen, discarding changes.
* Enter now closes the Edit Anim Info screen, keeping changes.
* New button "Goto Event Source (F4)". Goes to the entry that the "ghost" events are being imported from (if applicable).

## **Fixes**:
* Moving the mouse cursor off the graph section now cancels the selection for the `Event Graph` -> `Solo Highlight Event on Hover` option.