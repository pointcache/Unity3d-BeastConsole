using UnityEngine;
using System;
using System.Collections.Generic;


public class ExampleLog : MonoBehaviour {

    public bool BeastLogAlsoShowsInUnityConsole;
	void Start()
    {
        CFG.output_unity_log.Set(BeastLogAlsoShowsInUnityConsole);

        BeastLog.error("You've screwed up!");
        BeastLog.print("some notification");
        BeastLog.confirm("thing is working!");
        BeastLog.warning("you better not!");

        GameObject test = new GameObject("Something to test");
        
        //lets identify it in console!
        BeastLog.confirm("We just created " + test.id());
    }
}
