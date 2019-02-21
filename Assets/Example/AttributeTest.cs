using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;

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
