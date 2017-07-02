using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;


/// <summary>
/// This is an example of how easily you can make a global config.
/// </summary>


public class ExampleConfig : MonoBehaviour {

    public float Volume = 1f;
    public bool Vsync = false;
    public int FrameLimit = 60;

    private void Awake() {
        //Simple setter
        Console.RegisterVariable<float>("volume", "", x => Volume = x, this);

        //Lambda
        Console.RegisterVariable<bool>("vsync", "", x=>
        {
            Vsync = x;
            QualitySettings.vSyncCount = x ? 1 : 0;
        }, this);

        //Method
        Console.RegisterVariable<int>("frameLimit", "", SetFramerate, this);
    }

    private void OnDestroy() {
        Console.UnregisterVariable<float>("volume", this);
        Console.UnregisterVariable<bool>("vsync",  this);
        Console.UnregisterVariable<int>("frameLimit", this);
        
    }

    void SetFramerate(int val) {
        FrameLimit = val;
        Application.targetFrameRate = FrameLimit;
    }
}
