# CasLib
> [!WARNING]
> This is in active development. Game-breaking bugs may occur.

CasLib is a library mod for Casualties: Unknown modding (using BepInEx).

This should not be installed if you don't have any mods that require it as a dependency.

# Installation
This mod depends on BepInEx. Install BepInEx before anything, if you have not installed it already.

Once BepInEx installed, you should extract `CasLib.zip` from a github release into the BepInEx folder, and merge it's contents.

# Usage
Todo: Add useful documentation.

# Compiling from source
This plugin was developed on linux. If you're on windows, I do not have instructions for you. Good luck, but I have no clue how to compile software on windows.

**Only Arch Linux instructions are tested.**

On linux, first, install prerequisites:

- Arch: `sudo pacman -S dotnet-sdk`
- Ubuntu: `sudo apt install dotnet-sdk-10.0`
- Fedora: `sudo dnf install dotnet-sdk-10.0`

Once you have the prerequisites installed, copy all of the library files from Casualties: Unknown, and all of the library files from BepInEx into `lib`

After you've added the library files, run `./build` with the source directory open. This will build the mod.

Once the mod is built, create a folder in your BepInEx plugins folder, called `CasLib`. Place the contents of `bin/Debug/netstandard2.1` into this folder.

# Significant Bugs
- For some god-forsaken reason, after a while registered items lose their associated object.