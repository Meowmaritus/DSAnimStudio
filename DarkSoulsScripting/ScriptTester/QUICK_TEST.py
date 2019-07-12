from DarkSoulsScriptingBundle import *

UPDATE_RATE = 60.0
gp = Gamepad.PS3()
CodeHooks.TargetedChrPtrRecorder.PatchCode()

def OnLoad():
    ex.Wait(2000)

def OnUpdate():
    pass

def UpdateLoop():
    global playingAs
    justLoaded = False
    while Utils.IsGameLoading():
        justLoaded = True
        ex.Wait(66)
    if justLoaded:
        OnLoad()
    gp.Read()
    OnUpdate()

ticks = Utils.GetTickSpanFromHz(UPDATE_RATE)
playingAs = None
while True:
    Utils.FixedTimestep(UpdateLoop, ticks)