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
        Console.RegisterVariable<float>("Volume", "", x => Volume = x, this);

        //Lambda
        Console.RegisterVariable<bool>("Vsync", "", x=>
        {
            Vsync = x;
            QualitySettings.vSyncCount = x ? 1 : 0;
        }, this);

        //Method
        Console.RegisterVariable<int>("FrameLimit", "", SetFramerate, this);
    }

    private void OnDestroy() {
        Console.UnregisterVariable<float>("Volume", this);
        Console.UnregisterVariable<bool>("Vsync",  this);
        Console.UnregisterVariable<int>("FrameLimit", this);
        
    }

    void SetFramerate(int val) {
        FrameLimit = val;
        Application.targetFrameRate = FrameLimit;
    }
}
