
## Changes/Additions:
* If an animated weapon has no animation to match the one the player is doing, it will now try to go to animation a999_000000 / a99_0000 if it exists, then try to go to animation a000_000000 / a00_0000 if it exists, and as a last ditch effort, try to go to the very first animation in the weapon's animation list.
* You can now press the F7 key to append the current animation to the Combo Viewer's list.
  * If the list is blank, it will default to a combo animation type of `PlayerRH` if `c0000` is open or `EnemyComboAtk` if anything else is open.
  * If the list already has animations in it, it will continue with the same combo animation type as the last item in the list.
* You can now press the F8 key to open the Combo Viewer tool.
* Bloodborne files now attempt to load sounds from a new made-up directory `dvdroot_ps4/sound_win` instead of `dvdroot_ps4/sound`. In theory, you can put converted .FSBs that use mp3 or vorbis instead of the proprietary atrac9 format along with their accompanying .FEVs and then the sounds will play. If I can figure out how to automate it, I can add a preprocessing option similar to downgrading Sekiro animations but for Bloodborne audio.
* Player equipment IDs are always shown even if a name string exists in the FMG.

## Fixes:
* Resetting the animation back to the start now also does the same for any animated weapons.
* The automatic weapon anim selection now works for Bloodborne transformed weapons that have suffixes on the anim names like a000_000000_2.hkx.
* Weapons now use the last digit of the binder file ID to determine the index rather than `_X` at the end of the name because FromSoft apparently sometimes just puts the wrong names.
* [NOT YET] Fixed some IDs getting ignored in equip params
* Fixed extreme limb lengthening happening on animations that used bone scale values (mostly affects Bloodborne).
* Fixed event mapping running into errors and failing on some Bloodborne enemies.
* Added several additional cross-threading locks, fixing several race condition errors.

testing my fuckin keyboard smh fatcat