![Tested on Unity 5.4.0f3](https://img.shields.io/badge/Tested%20on%20unity-5.4.0f3-blue.svg?style=flat-square)&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

#BeastConsole
![screen](http://i.imgur.com/Djw2FPc.png)

This project is possible because of the effort of amazing people at Cranium Software
this project is based of SmartConsole https://github.com/CraniumSoftware/SmartConsole
And has all the approvals to use it and build up on top of it.


BeastConsole is evolution (hopefully) of SmartConsole,
it is optimized to work with newest unity UI system, it introcudes new concepts and has more flexibility.

BeastConsole is a suite of components:
  * Console - a backend allowing command registration and variables, assignment
  * Config class and dynamic variables allowing binding and direct usage of global variables as event raisers
  * Alternative log layer allowing you to catch all unity logs in runtime build in console to avoid using developer build.

There a lot of very useful features and ways to use it that i will expand in this doc with time.

**Dependencies:** 
* DOTween for tweening
* SmartConsole (embedded) 

#TODO:
* Modular configs, hook your config file/class instead of directly writing into BeastConsole classes
* External config file support 
* Console autocompletion dropdown as you type

#Console
* 80% of it is created by folks at CraniumSoftware
* Register your own commands/methods and provide parameters
* Autocomplete
* Variables can be split into groups separated by '.' like "gfx.targetFramerate"
* rich text support
* relays your own messages (BeastLog) as well as unity ones

Usage :
  1. Make sure the scene has unity EventManager
  2. Create new object "BeastConsole" add a BeastConsole script to it, set its parameters
  3. Make sure BeastConsole is set very high in Script Execution as it initializes config and that should happen before all config usage happens
  4. Press play, press the button you specified for console.
  5. start typing, press TAB to attempt autocomplete
  6. if you type "g" and press TAB it will autocomplete til "gfx." the dot at the end means its a group and you can continue typing second part with actual command/variable name
  7. when correct name is filled, press enter to see its current value, or to launch the command
  8. when correct name is filled, add a value after a space to insert a value "gfx.vsync = false"
  9. press arrow up to go through previous commands
  10. press ctrl + backspace to erase the line

#Config

Currently config is a static class you can modify adding your own global variables that look like this
``` csharp
public static Variable<int> targetFramerate;
```

once declared scroll down to the methods where we actually construct the variables

``` csharp
targetFramerate = new Variable<int>(GraphicsGroup, "targetfps", "set max fps for game",  true);
```
This will create a wrapper object - dynamic generic variable to which you can bind methods, allowing you to have very flexible interactions with your game global variables.

Variable<T>( group ("gfx." for example) , name , description, is this variable accessible through console? )

``` csharp
targetFramerate.SetSilent(Application.targetFrameRate);
```
We initialize the variable, we use SetSilent:
 there are 2 ways to set a variable
 * Set(value) -> will set its value AND raise OnChanged event, notifying all subscribers
 * SetSilent -> will not raise an event

``` csharp
targetFramerate.OnChanged += SetTargetFramerate;

static void SetTargetFramerate(int val)
{ Application.targetFrameRate = val; }
```
We subscribe method that sets the framerate in unity to that variable, now every time its Set(), the method processes it.
Or use lambdas 
``` csharp 
CFG.vsync.OnChanged += x => QualitySettings.vSyncCount = x == true ? 1 : 0;
CFG.fov.OnChanged += x => Camera.main.fieldOfView = x;
```
To get the value we just use =
``` csharp
int currentFramerate = CFG.fps
```
#Log

This class is fairly simple it allows you to relay messages to beast console avoiding unity debug logs, it also guarantees that you will see console messages even in release build, because that is what you most of the time want.

Check example scene and example script for usage, or just study the class.

