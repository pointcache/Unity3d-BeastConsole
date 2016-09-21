#BeastConsole

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

