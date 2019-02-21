
![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

# Beast Console

![screen](https://i.imgur.com/HwZk4ZJ.jpg)


![Video](https://www.youtube.com/watch?v=nmFolz6C8Tk)

## Recently updated to remove any trace of UGUI. Now its pure IMGUI, with no additional clutter.

This project is possible because of the effort of amazing people at Cranium Software
this project is based of SmartConsole https://github.com/CraniumSoftware/SmartConsole

BeastConsole is evolution (hopefully) of SmartConsole,


# Console
* Backend is created by folks at CraniumSoftware.
* Use attributes to bind members 
* Register your own arbitrary commands/methods for any object(non monob as well) and provide parameters
* Autocomplete
* Both commands and variables support any number of subscribers, modify all objects at once.
* Multiline
* Suggestions
* History
* ConsoleUtility class containing methods to draw tables. (You can actually use ConsoleUtility pretty much by itself instead of Console class).

# Usage

setup:
	Drop Console prefab in scene, change parameters to your liking.
 
Attributes:

Console uses reflection to find attributes, to speed up the whole process we should mark any class that has uses console 
attributes with `[ConsoleParse]` attribute.

```csharp

[ConsoleParse]
public class AttributeTest : MonoBehaviour {

    [ConsoleVariable("testvar", "")]
    public float TestVariable = 5f;

    [ConsoleVariable("testproperty", "")]
    public bool TestPropertyVariable
    {
        set {
            Console.WriteLine("testproperty was set to: " + value);
        }
    }


    [ConsoleCommand("testMethod", "")]
	void TestMethod() {
        BeastConsole.Console.WriteLine("test method works");
    }

    [ConsoleCommand("testMethodWithParams", "")]
    public void TestMethodWithParams(float param1) {
        BeastConsole.Console.WriteLine("works :" + param1.ToString());

    }

    [ConsoleCommand("testMethodWith2Params", "")]
    public void TestMethodWith2Params(float param1, int param2) {
        BeastConsole.Console.WriteLine("works :" + (param1+ param2).ToString());

    }
}


```

Manual:

```csharp

public float Volume = 1f;
Console.RegisterVariable<float>("Volume", "", x => Volume = x, this);

Console.RegisterCommand("MoveUp", "", this, MoveUp);
   
private void MoveUp(string[] val) {
   transform.position += new Vector3(0, System.Convert.ToSingle(val[1]) , 0);
}

```
Manual method requires you to unregister commands and variables when done with an object.

See Example folder for more examples.

