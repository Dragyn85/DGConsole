![alttext](https://img.shields.io/badge/Unity%20version-2022-lightgrey&?style=for-the-badge&logo=unity&color=lightgray) ![alttext](https://img.shields.io/badge/O.S-Windiws%2011-lightgrey&?style=for-the-badge&color=purple)
# DGConsone
Add a console for your project that can be used for ex. debugging. 
* Run commands and execute code with text inputs.
* Easy to customise your own commands.
* Unity events hooked up to commands with individual tags.
* Get log messages inside the game.

#### Settings options
Use commands to change settings or you can easily change setting in the json file created in your Application.persistentDataPath after first use.

#### Quick start?
1. Import this package by downloading it and add it with the unity package manager or by adding a git repo
2. Add the GDConsole prefab to a scene
3. Mark your methdos with [ConsoleAction] attribute to add them as a command. Static methods and methods inside a Monobehaviour will work without further modification, methods inside other objects need a reference.
4. Press play and enter the command name in the console.

Look in the DemoScene for examples.

#### Creating commands
You can create your own commands easily.
Add [ConsoleAction] to any method in your scripts. 
You can also overwrite the name of the method and parameter aswell as adding a description
[ConsoleAction(Command = "command name", Description="command description", ParameterNames = new string[]{"param1","param2"}]

#### Type of methods
##### static
If your methods are static ones they work as soon a you mark them with the attribute.
##### MonoBehaviour
When executing a command that exist on a MonoBehaviour the CommandManager will use the first GameObject (of correct type) it finds.

You can specify a certain GameObject be enterint @gameobjectname when entering a command. The @ is for identifying that its a name and can be changed in the prefab.

##### instance method
If your method belongs to any other object you need to registrer that object in the CommandManager with RegisterObjectInstance(object) method. This will register the object to all commands related to this object.
#### UI
The UI is a simple window with an input field and a message area, all configurations are made with commands or changing the settings.json file in the persistant data path.

Toggle visibilty with "§" or BackQuote, can be changed.

#### Credits
Based on parts of code from https://github.com/yasirkula/UnityIngameDebugConsole

