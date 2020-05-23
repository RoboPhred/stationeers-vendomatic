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

This mod will work in multiplayer if both the client and the server have the mod.
The item combining feature should work regardless if the client has the mod installed or not.
Choosing the amount of items to vend may result in desyncs if the client does not have the mod but the server does.
This mod works on dedicated servers.
