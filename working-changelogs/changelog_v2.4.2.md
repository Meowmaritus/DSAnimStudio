## Changes/Additions:
* Animation Overlays list in Toolbox now shows "No animation overlays found for this model." if none are present or "No model currently loaded." if there is no model currently loaded.
* Window size, animation list width, and model viewer size save to the configuration file now.
* Skybox brightness and Show Cubemap As Skybox now save in the config file.
* You can now disable the camera pivot point indicator box in the Toolbox next to the snap to 45 degree angles option.
* Moving the mouse now cancels tooltips in Toolbox.
* Skybox/cubemap settings are now grouped together in Toolbox.
* Snap Cam to 45 Degree Angles now moves to the nearest snap angle a little bit faster.
* You can now go up to 32x MSAA if MonoGame decides that your GPU supports it. If not, it will just use the max it can (internal MonoGame behavior).
* Hitbox, bullet spawn, and sfx spawn simulations now override the option to disable the DummyPoly in the list in the Toolbox.
* You can zoom the viewport in/out while moving or panning the camera.

## Fixes:
* Middle clicking the colors now resets them to default as intended.
* Tooltip that says light rotation doesn't work while Light Follows Camera is enabled has been edited.
* Removed Skybox Motion Blur Strength option as it was for a failed test during development and did nothing.
* Snap Cam to 45 Degree Angles no longer has the speed of the movement to the nearest snap angle tied to framerate.
* Manually typing in a vertical field of view value not within 1 to 179 degree range no longer causes an infinite error message loop it just corrects the value for you.
* Manually typing in the MSAA multiplier no longer lets you go below 1x or above 32x.
* Manually typing in the SSAA multiplier no longer lets you go below 1x or above 4x.
* The following color options in Toolbox now apply the color changes immediately instead of on next model load:
  * `Colors -> Helper -> Flver Bone Bounding Box`
  * `Colors -> Helper -> Flver Bone`