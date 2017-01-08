# SmartConsole

A Quake style debug console for Unity with autocomplete, a command history and the ability to register custom commands and variables

The implementation is broadly patterned after the debug console implementation from GLToy: https://code.google.com/p/gltoy/source/browse/trunk/GLToy/Independent/Core/Console/GLToy_Console.h

This document is formatted following the conventions of Markdown to aid readability - you can use websites like http://markable.in/editor/ to view it in a more visually pleasing format than plain-text.

## Examples

There is an example scene included in the package under SmartConsole/Scenes/Example.scene

## Usage

The SmartConsole component should be added to an otherwise empty game object and have a font set in the inspector. For ease of use static functions are provided so that the instance need not be tracked by calling code.

The console is accessed with the backtick ` key when a keyboard is available, otherwise touching or clicking the top right corner will open the console ready for input

Custom commands and variables can be registered using the SmartConsole class interface described later in this document. There are some built-in commands and variables.

## Built-in Commands

### clear

clear the console log

### cls

clear the console log (alias for clear)

### echo <string>

writes <string> to the console log

### help

lists all console commands with their descriptions and example usage (where available)

### list

lists all currently registered console variables

### print <string>

writes <string> to the console log (alias for echo)

### quit

quit the game (where possible)

### callstack.warning

display the call stack for the last warning message

### callstack.error

display the call stack for the last error message

### callstack.exception

display the call stack for the last exception message

## Built-in Variables

### show.fps

whether to draw framerate counter or not

### console.fullscreen

whether to draw the console over the whole screen or not

### console.lock

whether to allow showing/hiding the console

### console.log

whether to redirect log to the console

## SmartConsole Class Interface

the SmartConsole class provides most of the functionality in the package:

### public delegate void ConsoleCommandFunction( string parameters )

a delegate type for registering as a console command

### public static void Clear()

clear the console history

### public static void Print( string message )
### public static void WriteLine( string message )

write the message string to the debug console (only - not the log)

### public static void ExecuteLine( string inputLine )

execute a string as if entered to the console


### public static void RemoveCommandIfExists( string name )

unregister the named command, if it exists

### public static void RegisterCommand( string name, string exampleUsage, string helpDescription, ConsoleCommandFunction callback )
### public static void RegisterCommand( string name, string helpDescription, ConsoleCommandFunction callback )
### public static void RegisterCommand( string name, ConsoleCommandFunction callback )

register a console command with an optional example usage and help description


### public static Variable< T > CreateVariable< T >( string name, string description, T initialValue ) where T : new()
### public static Variable< T > CreateVariable< T >( string name, string description ) where T : new()
### public static Variable< T > CreateVariable< T >( string name ) where T : new()

used to create a console variable e.g.
    
    SmartConsole.Variable< bool > showFPS = SmartConsole.CreateVariable< bool >( "show.fps", "whether to draw framerate counter or not", false );

### public static void DestroyVariable< T >( Variable< T > variable ) where T : new()

destroy a console variable (so its name can be reused)

## SmartConsole.Variable< T > Generic Class Interface

SmartConsole.Variable< T > references are returned when using the CreateVariable function and should be kept if you want to modify or read a console variable


### public void Set( T val )

provided as a workaround to not being able to overload the assignment operator - sets the value of the console variable
		
### public static implicit operator T( Variable< T > var )

allows reading of a console variable in a syntactically convenient fashion e.g.

    float f = 0.0f;
    SmartConsole.Variable< float > myFloatVar = SmartConsole.CreateVariable< float >( "my.float", "an example", 1.0f );
    f = myFloatVar; // f == 1.0f

## Version History

### 1.0.3
- add a warning if the user pauses, makes code changes, then unpauses
  (it seems like this behaviour may have changed in recent Unity updates, but relying on edit-and-continue is not safe anyway!)

### 1.0.2

- add this document

### 1.0

- initial release
  - displays console on screen
  - redirects unity log to console
  - input and rendering tested with iOS and Android
  - command history
  - autocomplete
  - fps counter
  - command registration and deletion
  - variable registration and deletion
  - display call stacks in game
 
## Contact Us

info@cranium-software.co.uk

