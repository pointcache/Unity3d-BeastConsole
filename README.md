![Tested on Unity 5.4.0f3](https://img.shields.io/badge/Tested%20on%20unity-5.4.0f3-blue.svg?style=flat-square)&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

![logo](http://i.imgur.com/YJqW3LG.png)
![screen](http://i.imgur.com/Djw2FPc.png)

This project is possible because of the effort of amazing people at Cranium Software
this project is based of SmartConsole https://github.com/CraniumSoftware/SmartConsole
And has all the approvals to use it and build up on top of it.


BeastConsole is evolution (hopefully) of SmartConsole,
it is optimized to work with newest unity UI system, it introcudes new concepts and has more flexibility.

Why use this console:
Easy to hook up.
Works through rective variables, everything is serialized.
Out of the box serialization of all the configs.
In short - super easy, reliable, and looks good.

##**WATCH VIDEO WALKTHROUGH**
[![IMAGE ALT TEXT](http://img.youtube.com/vi/CzwG-N62g-g/0.jpg)](http://www.youtube.com/watch?v=CzwG-N62g-g "Walkthrough")

BeastConsole is a suite of components:
  * Console backend- a backend allowing command registration and variables, assignment
  * Console ui - prefabs implementing console display
  * Reactive variables (rVar) based on UniRx implementation, they are fully serializeable dynamic variables allowing you to subscribe to their changes and allowing the console to interact with them and the config system to inject values on deserialization.
  * Config system, dynamically binds reactive variables and creates entries in console for them
  * BeastLog layer allowing you to catch all unity logs in runtime build in console to avoid using developer build.

**Dependencies:** 
* FullSerializer
* SmartConsole (embedded) 




#Console
* Backend is created by folks at CraniumSoftware
* Register your own arbitrary commands/methods and provide parameters
* Autocomplete
* Add variables with Attribute

#Config
Config and rVar is a workhorse of the console. Config monobehavior containing our variables that go into the console,
and that we use across the whole game to drive simulation.
rVar is built after UniRx, meaning it's battletested and is worth using as a backbone for your game.



#Log

This class is fairly simple it allows you to relay messages to beast console avoiding unity debug logs, it also guarantees that you will see console messages even in release build, because that is what you most of the time want.

Check example scene and example script for usage, or just study the class.

#Usage

setup:
 * drop EventSystem in scene, add BeastConsole to game object, launch - done.
 
Adding variables to the console:
 * Config class acts as a collector for console variables
 1. Create a config class (e.g. GraphicsConfig, PhysicsConfig, GameConfig...) 
 2. Inherit from `ConfigBase` 
 3. override OnEnable with base.OnEnable() in it, so we can actually receive unity messages (thanks unity)
 4. add reactive variable of chosen type

```csharp

public r_float Size = new r_float(1f);

```

5. add attribute

```csharp

[ConfigVar("game.cubesize", "size of cube")]
    public r_float Size = new r_float(1f);

```
    
6. Done.

Adding commands to the console


#TODO:
 * Console autocompletion dropdown as you type
 * Complete game example
