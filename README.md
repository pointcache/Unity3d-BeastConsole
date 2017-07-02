
![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

![logo](https://kek.gg/i/5Xt-2j.png)
![screen](https://kek.gg/i/7WYj78.png)

This project is possible because of the effort of amazing people at Cranium Software
this project is based of SmartConsole https://github.com/CraniumSoftware/SmartConsole

BeastConsole is evolution (hopefully) of SmartConsole,
It is made with uGui.

BeastConsole is:
  * Console - user works with this class
  * Console backend (SmartConsole)- a backend allowing command registration and variables
  * Console ui - prefabs implementing console display

#Console
* Backend is created by folks at CraniumSoftware, heavily cleaned up and refactored.
* Register your own arbitrary commands/methods and provide parameters
* Autocomplete
* Add variables 
* Both commands and variables support any number of subscribers, modify all objects at once.

#Usage

setup:
 * drop EventSystem in scene, add BeastConsole to game object, launch - done.
 

```csharp

public float Volume = 1f;
Console.RegisterVariable<float>("Volume", "", x => Volume = x, this);

Console.RegisterCommand("MoveUp", "", this, MoveUp);
   
private void MoveUp(string[] val) {
   transform.position += new Vector3(0, System.Convert.ToSingle(val[1]) , 0);
}

```

You should unregister commands and variables when done with an object.

See Example folder for more examples.




#TODO:
 check Projects page
