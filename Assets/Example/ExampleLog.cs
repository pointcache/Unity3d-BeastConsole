using UnityEngine;
using System;
using System.Collections.Generic;
using BeastConsole;

public class ExampleLog : MonoBehaviour {

	void Start()
    {
        BeastLog.Error("You've screwed up!");
        BeastLog.Log("some notification");
        BeastLog.Success("thing is working!");
        BeastLog.Warning("you better not!");

        GameObject test = new GameObject("Something to test");
        
        //lets identify it in console!
        BeastLog.Success("We just created " + test.id());
    }
}
