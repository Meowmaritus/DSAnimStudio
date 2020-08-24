## MEOW TODO (DELETE ME LATER)
* Fix the friede scythe and other shit event in ds3
* [done] FIX ADDITIVE BLEND ON DS1R/SDT WTF REE (check a00_3303 on c2310 btw)
* [done] Make PlayerRH, PlayerLH, PlayerDodge, etc set the default dummy source to rh, LH, body
* Add an option to record to HKX for ds1/ds1r/maybe ds3 
  * Would include a thing to record a combo ofc 
  * Would save to interleaved uncompressed hkx and/or xml and use hork compressor
* Add Demon's Souls animation support
* Add option to delete all results in Ctrl+F
* Fix Ctrl+F result animation IDs showing up as a000_123456 when the result is clearly in a123.tae etc
* [NOT YET] You can now press the F7 key to append the current animation to the Combo Viewer's list.
  * If the list is blank, it will default to a combo animation type of `PlayerRH` if `c0000` is open or `EnemyComboAtk` if anything else is open.
  * If the list already has animations in it, it will continue with the same combo animation type as the last item in the list.
* [NOT YET] Now loads player's body in DS3 and SDT and their face in PTDE/DS1R
* [NOT YET] Bloodborne shader now more matte
* [NOT YET - MIGHT NOT DO BECAUSE HARDER THAN I THOUGHT LOL] Model mask changes persist throughout a combo instead of resetting upon starting each animation of a combo.


## Changes/Additions:
* No more selecting weapon model indices and weird stuff like that; DS Anim Studio now works just like ingame. For dual weapons (and Bloodborne trick weapons), simply put the desired weapon in your right hand then select the "Right Weapon Two-Handed" for the Weapon Style etc. The offhand weapon will be sheathed and everything will function properly with all hitboxes showing up exactly as they do ingame.
* If an animated weapon has no animation to match the one the player is doing, it will now try to go to animation a999_000000 / a99_0000 if it exists, then try to go to animation a000_000000 / a00_0000 if it exists, and as a last ditch effort, try to go to the very first animation in the weapon's animation list.
* You can now press the F8 key to open the Combo Viewer tool.
* New combo entry type: `NoCancel <Animation ID>` which does not cancel the previous animation in the combo and instead waits until it finishes.
* You can now put start/end frame numbers on combo entries. Syntax is `<Entry Type> <Animation ID> [<Start Frame> [<End Frame>]]`
  * e.g. `EnemyComboAttack a01_3001 12 24`
  * Optional feature (you can use old syntax and it just assumes entire animation until it should cancel).
  * If start frame is -1 or you don't put frame numbers, it starts at the beginning of the animation.
  * If end frame is -1 or you don't put frame numbers or you only put the start frame, it ends at wherever it would end normally (like the cancel event or just the end of the animation with the new `NoCancel` entry type).
* Bloodborne files now attempt to load sounds from a new made-up directory `dvdroot_ps4/sound_win` instead of `dvdroot_ps4/sound`. In theory, you can put converted .FSBs that use mp3 or vorbis instead of the proprietary atrac9 format along with their accompanying .FEVs and then the sounds will play. If I can figure out how to automate it, I can add a preprocessing option similar to downgrading SDT animations but for Bloodborne audio.
* Player equipment IDs are always shown even if a name string exists in the FMG.
* Equipment names are now loaded in SDT.
* Animation list is slightly more compact.
* Added `Go To Animation Section ID...` (Ctrl+H). Lets you go to a specific section, rather than having to type the exact ID of the first animation in that section.
* Holding down the previous/next anim key now goes faster.
* Next/previous anim section now works for enemies where all sections are in the same TAE (they still do not show up as collapsable sections yet, because I would need to rewrite lots of the GUI code for the animation list for that to be possible and I don't feel like it).
* Now checks one directory above the data root directory for UXM extracted files if you're editing files from within a modengine folder.
* Downgrading DS1R / SDT animations is now noticably faster.
* Sound effect spawn location helper now defaults to disabled like I originally intended it to.
* Combo Viewer combos now sets the appropriate default hitbox viewing source (right weapon / left weapon / body) for you.
  * For anyone unfamiliar, this setting has always been there (under Player Settings) and is used to figure out where to put hitboxes where FromSoft didn't specify which fucking model to spawn them on.
  * `PlayerRH` and `PlayerLH` select their respective weapons as the hitbox DummyPoly source and anything else sets the character's body as the source.
  * Only used on Player model.


## Fixes:
* Fixed a regression that caused camera rotation speed to scale inversely with framerate again. Not sure how this change got undone but, okay, whatever.
* Resetting the animation back to the start now also does the same for any animated weapons.
* The automatic weapon anim selection now works for Bloodborne transformed weapons that have suffixes on the anim names like a000_000000_2.hkx.
* Weapons now use the last digit of the binder file ID to determine the index rather than `_X` at the end of the name because FromSoft apparently sometimes just puts the wrong names.
* Fixed some IDs getting ignored in equip params.
* Fixed extreme limb lengthening happening on animations that used bone scale values (mostly affects Bloodborne).
* Fixed event mapping running into errors and failing on some Bloodborne enemies.
* Added several additional cross-threading locks, fixing several race condition errors
* Previous/next animation/group keys now skip over sections with no animations in them instead of just stopping and requiring you to use the mouse to skip over them.
* Fixed a cross-threading error where you'd attempt to load a file and it wouldn't exist and the app would ask "would you like to delete it from the recent file list?" and it would try to access the recent file list from the wrong thread.
* Fixed a few errors on the xml event template for DS3 OBJBNDs
* Fixed a crash when TagTools fails to downgrade a DS1R or SDT file. Now shows an error popup.
* Fixed MSGBND files with renamed FMG files inside (such as mods that rename them to English instead of their original Japanese names) not loading correctly.
* No longer requires the text files to be present in order to load correctly. If all the text files weren't found, you will be warned of the consequences and be able to use the app.
* Fixed longstanding oversight which caused the Change Player Equipment window to not close if you loaded another character. The equip menu from a previously-loaded character did nothing if you were wondering.
* Fixed a bug that would lock the app into an infinite error loop when certain errors happened in the middle of a draw call.
* The default player equipment for each game has been updated to be useable for each game.
* Made loading far more stable and less likely to have errors.
* Fixed Find (Ctrl+F) dialogue's event type number search type showing resulting events as unmapped.
* Fixed Find (Ctrl+F) dialogue not showing the event type number after the mapped name (was `EventName()` before, now it's `EventName[ID]()` like on the graph).
* DummyPoly movement now takes into account the animation of both the reference bone and the attach bone instead of only the attach bone (only place this has been utilized that I've come across so far is a Bloodborne chalice dungeon object but it seems likely that various other objects use it).
* Failed load operations no longer result in the mouse cursor being a spinny loading circle indefinitely.
* Objects now load sounds.
* Fixed additive blend animations on downgraded DS1R and SDT animations.

## Known Issues That I Didn't Feel Like Fixing Yet:
* Find window results always say "a000_" / "a00_" even if in other groups.
* Clicking a Find window result doesn't refresh the main screen until you click the main screen.

PlayerRH a142_030000
PlayerRH a142_030100
PlayerRH a142_030200
PlayerWeaponSwitch a142_033300
PlayerRH a142_040010
PlayerWeaponSwitch a142_043310



















[2.3.1]




