using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;


/// <summary>
/// This example is made by manually registering console variables. Most of the time you would want to 
/// use an attribute but sometimes you would need a manual method (for example for registering 
/// non monobehaviors.
/// </summary>
public class ExampleConfigManual : MonoBehaviour {

    public float Volume = 1f;
    public bool Vsync = false;
    public int FrameLimit = 60;

    private void Awake() {
        //Simple setter
        Console.AddVariable<float>("volume", "", x => Volume = x, this);

        //Lambda
        Console.AddVariable<bool>("vsync", "", x=>
        {
            Vsync = x;
            QualitySettings.vSyncCount = x ? 1 : 0;
        }, this);

        //Method
        Console.AddVariable<int>("frameLimit", "", SetFramerate, this);
    }

    private void OnDestroy() {
        Console.RemoveVariable<float>("volume", this);
        Console.RemoveVariable<bool>("vsync",  this);
        Console.RemoveVariable<int>("frameLimit", this);
        
    }

    void SetFramerate(int val) {
        FrameLimit = val;
        Application.targetFrameRate = FrameLimit;
    }
}
