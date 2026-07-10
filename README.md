# Setup

## Create TAS Environment

If you do not want this mod to affect your original game files, copy the entire contents of your Ultrakill directory to another location. Typically, your game files can be found on the path `C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL`.

Then, update `FreezeFrame.csproj.user` to match the file path to your game directory wherever it is.

## Extract BepInEx

Download BepInEx from `https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip` and unzip its contents into the Ultrakill game directory.

Modify the contents of `[YOUR ULTRAKILL FOLDER]\BepInEx\Config\BepInEx.cfg` to set `HideManagerGameObject = true`. Also, optionally but recommended, under `[Logging.Console]` set `Enabled = true` for debugging purposes.

# Building from Source

To build the plugin component, run the command `dotnet build src/FreezeFrame`. You should see a file `FreezeFrame.dll` automatically copied into your game's directory under `\BepInEx\plugins`.

To build and run the CLI component, use the command `dotnet run --project src/FreezeFrameCLI`.