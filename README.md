# Persistent RimWorlds
Turns a rimworld into a persistent rimworld, explore fallen colonies where pirates may lurk and stop by your predecessors.

This RimWorld mod allows you to convert any chosen 'game' (save file) into a persistent rimworld, which will allow you to have multiple colonies on the same world.

I aim to have this mod compatible with many other mods out there, so this mod runs primarily using Harmony to patch, however checking first if have loaded in a persistent world - in order to not interfere with single colony games. The mod menu can be accessed through the main menu screen which has been 'transpiled' using harmony so it only adds a few lines of code during runtime that allow the menu button to show up and be accessed.

The downside of having separated files from a world/game is that there are special classes, lets call them 'Logic' classes which handle loading/saving of the persistent rimworld which can interfere with mods that modify the ```Game.LoadGame()``` or ```Game.InitNewGame()``` methods.

This mod will require extensive testing and so any help is appreciated in stabilizing mod support as well as end-game support as there is still a lot of testing that needs to be conducted and bugs to be fixed, as well as features to be implemented. Any contributions are welcome that improve mod compatibility, bugs or improvements on current mod features. Any suggestions can be reported in the Issues tab with using the 'enhancement' label. Any bugs that need to be reported can be labeled with the 'bug' label.
