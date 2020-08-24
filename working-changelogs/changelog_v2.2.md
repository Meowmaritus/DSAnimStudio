TODO:
* Rename event 112 to SpawnFFX_Floor in every game
  * Rename "Size" to "DummyPolyID" in ds1
  
* Change event114's (and others like 118) CStyleType to DummyPolySource and change enum values to 
    * 0: Body
    * 1: Left Weapon
    * 2: Right Weapon
    * 3: Source 3
    * etc
  
## Changes/Additions:
* Event IDs are shown even when a name is present like `Blend[16]()` in all contexts.
* You no longer have to double click values in the inspector to edit them, just a single click is needed. Since you have to click again to open the dropdown list on dropdown parameters, those have effectively only been reduced from 3 total clicks to 2 total clicks.
* Combo previewer was reworked to be much easier to use/understand.
* Many TAE mapping updates.
  * Some things are corrected from new research.
  * Some things are renamed to be easier to understand.
  * Bloodborne mappings are now also contained within the DS3 mapping file as separate banks near the bottom of the file.
    * Many more Bloodborne events are mapped as it's almost identical to DS3.
    * Bloodborne TAE imported into DS3 now properly functions as it does ingame.

## Fixes:
* All events will now be active for at least one frame each loop, no matter what. This is how the game seems to do it. In previous versions of DS Anim Studio, if the playback cursor was moving fast enough over a short enough event, it could actually just not trigger it.
* Fixed various crashes. Well, they would be crashes if it wasn't for the error popups. Anyways I fixed a ton of them.
* Fixed multiple-second freeze when loading subsequent new files (it was caused by the animation list attempting to access some things that weren't loaded yet, leading to exception throw spamming, which has a lot of overhead, probably due to the way it winds back the call stack).
* Fixed combo previewer sometimes not recognizing that animations exist.
* Corrected misconception that anim references chained when they do not.
* Fixed lag spike upon changing to a new animation when lots of RAM was being used by the application (it was caused by calling garbage collection on animation change, soemthing I did as a test at one point and forgot to remove).
* Fixed typo that caused textures without mipmaps to attempt to display with mipmaps, making them fade to black as the camera got further away (no data for subsequent pixels = black). Only relevant for very specific textures, such as Cleric Beast's body textures.