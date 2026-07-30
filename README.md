# Setup

## Create TAS Environment

If you do not want this mod to affect your original game files, copy the entire contents of your Ultrakill directory to another location. Typically, your game files can be found on the path `C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL`.

## Install BepInEx

Download BepInEx from `https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip` and unzip its contents into the Ultrakill game directory. Then, boot the game to initialize the rest of the BepInEx files.

Modify the contents of `[GAME DIR]\BepInEx\Config\BepInEx.cfg` to set `HideManagerGameObject = true`. Also, optionally but recommended, under `[Logging.Console]` set `Enabled = true` for debugging purposes.

## Plugin Dependencies

To build from source, you need to go to `[GAME DIR]\ULTRAKILL_Data\Managed` and copy the following .dll files into `FreezeFrame\lib`.
 * `Assembly-CSharp.dll`
 * `Unity.InputSystem.dll`

## Building from Scratch

Both components can be built using Visual Studio. After building the plugin, you can copy the assembly from `FreezeFrame\bin\Debug\netstandard2.1\FreezeFrame.dll` into `[GAME DIR]\BepInEx\plugins` to install it.