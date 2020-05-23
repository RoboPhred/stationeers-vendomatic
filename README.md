# Vendomatic for Stationeers

Vending machine QOL improvements

- Vending machines combine items into stacks internally
  - If an input stack goes above the maximum stack size of a stored stack, the stored stack will take as many as possible and a new stack created for the remainder.
  - Non-stackables are slotted as normal.

# Releases

To download, visit the [Releases](https://github.com/RoboPhred/stationeers-vendomatic/releases) page.

# Installation

Requires [BepInEx 5.0.1](https://github.com/BepInEx/BepInEx/releases) or later.

1. Install BepInEx in the Stationeers steam folder.
2. Launch the game, reach the main menu, then quit back out.
3. In the steam folder, there should now be a folder BepInEx/Plugins
4. Copy the `Vendomatic` folder from this mod into BepInEx/Plugins

# Multiplayer Support

This mod will work in multiplayer as long as the server has the mod. Clients do not need to install this mod for it to work on the server.
If a client with this mod installs joins a server without the mod, the mod will have no effect.
