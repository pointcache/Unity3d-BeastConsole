
![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

# Beast Console

![screen](https://kek.gg/i/3zZFVr.png)
![screen](https://kek.gg/i/7K8kgq.gif)


This project is possible because of the effort of amazing people at Cranium Software
this project is based of SmartConsole https://github.com/CraniumSoftware/SmartConsole

BeastConsole is evolution (hopefully) of SmartConsole,
It is made with uGui.

BeastConsole is:
  * Console - user works with this class
  * Console backend (SmartConsole)- a backend allowing command registration and variables
  * Console ui - prefabs implementing console display

# Console
* Backend is created by folks at CraniumSoftware, heavily cleaned up and refactored.
* Use attributes to bind methods/fields/properties 
* Register your own arbitrary commands/methods for any object(non monob as well) and provide parameters
* Autocomplete
* Add variables 
* Both commands and variables support any number of subscribers, modify all objects at once.
* Multiline
* Suggestions
* History
* Out of the box support for RVar(not included)  https://github.com/pointcache/unity-reactive-var

# Usage

setup:
 * drop EventSystem in scene, add BeastConsole to game object, launch - done.
 
Attributes:

```csharp

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




# TODO:
 check Projects page
