![alttext](https://img.shields.io/badge/Unity%20version-2022-lightgrey&?style=for-the-badge&logo=unity&color=lightgray) ![alttext](https://img.shields.io/badge/O.S-Windiws%2011-lightgrey&?style=for-the-badge&color=purple)
# DGConsone
Add a console for your project that can be used for ex. debugging. 
* Run commands and execute code with text inputs.
* Easy to customise your own commands.
* Unity events hooked up to commands with individual tags.
* Get log messages inside the game.

#### Settings options
Use commands to change settings or you can easily change setting in the json file created in your Application.persistentDataPath after first use.

#### How to use?
1. Import this package by downloading it and add it with the unity package manager or by adding a git repo
2. Add the GDConsole Canvas
3. Mark your methdos with [ConsoleAction] attribute to add them as command. Static methods and methods inside a Monobehaviour will work without further modification, methods inside other objects need a reference.

Look in the DemoScene for examples.

#### The UI
Toggle visibilty with "§" or BackQuote, can be changed.

#### Credits
Based on code from https://github.com/yasirkula/UnityIngameDebugConsole
