# Imports the default stuff. "f" for functions, "ex" for extra functions, etc
from DarkSoulsScriptingBundle import *

# Standard python random library for shenanigans
import random

# Use WorldChrMan.LocalPlayer. to access the player chr structure.
WorldChrMan.LocalPlayer.DisableGravity2 = True # Yes there are 2 disable gravity values. The second one seems to be the only correct one though. Wulf still hasn't confirmed.

# WorldState is currently not mapped very much, expect more values to be added here later
WorldState.BonfireID = 1212962 # Sets respawn bonfire to Oolacile Township

f.SetEventSpecialEffect_2(10000, 33) # Immediately curse player
f.SetEventFlag(11210001, False) # Clears the "Artorias is dead" flag

# Some utility modules exist (more will be added as needed):
Utils.WaitLoadCycle() #Wait until after the game finishes loading.

# Use f. to  access ingame functions with mapped parameters
f.ChrFadeIn(10000, 5.0, 0.0)

# Because WorldChrMan.LocalPlayer is a Player-type Chr, it has a PlayerStats module, accessible with WorldChrMan.LocalPlayer.Stats:
WorldChrMan.LocalPlayer.Stats.STR = 99 #ur stronk now

# We also include many of the game's alternate methods of accessing things in memory, such as this:
Game.LocalPlayerStats.STR = 99 # Does same thing as previous

# Use f.Unmapped to access ALL ingame lua functions with no parameters listed (in case you need to use a function we haven't mapped out the parameters for)
print str(f.Unmapped.GetTravelItemParamId(7)) #No dea what this returns. If you find out, please let me know ;)

# Use ex. to access extra functions we've added to extend the functionality.
ex.MsgBoxOK("Hi there. Press OK to spawn your own Artorias bodyguard to help you through Oolacile Township.")

# Use Map.Find(area, block) to find a map by its area/block numbers
oolacile = Map.Find(12, 1) # m12_01_00_00

# Use the FindEnemy(id, instance) function on a MapEntry object to search for an enemy.
artorias = oolacile.FindEnemy(4100, 0) # The enemy named "c4100_0000" in the MSB

# Note: Map.Find(12, 1).FindEnemy(4100, 0) also works

# the ID property in Chr structure gets the ChrID of that Chr for use with various functions.
f.SetDisable(artorias.ID, False)

artorias.DisableEventBackreadState = False # Does most of what the previous thing does

# Do note, however, that there are some Chr's that literally always respawn and have no special events. 
# These Chr's will have -1 as their ID and, as such, these functions will not work.
# However, you can still do some of the things those functions do by directly modifying the Chr:
artorias.MovementCtrl.EnableLogic = True # Does the same thing as f.EnableLogic(artorias.ID, True)

artorias.TeamType = TEAM_TYPE.BattleFriend


# There are also some helper functions in many of the ingame structures mapped out that we added to make things easier:
artorias.WarpToPlayer(WorldChrMan.LocalPlayer) # Warps Artorias to the exact same location as the local player

#### As of Pre-Release 0.2: ####

# Many structs were updated to have new values
WorldChrMan.LocalPlayer.OmissionLevel = 8

# Structs added:
Game.ClearCount = 6 #Makes game NG+6

Game.Options.CameraAutoWallRecovery = False #Disables auto wall recovery in the options menu.

Game.Tendency.CharacterBlackWhiteTendency = 1 #Changes character tendency. Likely will have no effect whatsoever because this isn't Demon's Souls.

ChrDbg.AllNoMPConsume = True #Would make every chr in the game not consume MP, if this was Demon's Souls (don't worry, ChrDebug also has plenty of Dark Souls debug options)

ChrFollowCam.FovY = 0.8 #Messes with field of view

WorldState.Autosave = False #Disables autosaving. 
# Do note that this setting is set to False EVERY SINGLE TIME A SCRIPT BEGINS EXECUTING for safety purposes and it stays off after the script is done.
# If you wish to re-enable saving you must do so manually in your script or just exit and re-launch the game 
# (but doing so would cause you to lose all progress since you last ran a script and disabled autosave.

while True:
    ex.SetLineHelpTextPos(225, 192);
    ex.SetLineHelpText("Current Animation: " + str(Misc.PlayerCurrentAnimation)) # Displays player current animation. Misc module contains some other neat stuff too.
    ex.Wait(33)
