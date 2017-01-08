using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BeastConsole;
using rvar;
public class ExampleConfig : ConfigBase {

    [ConfigVar("somefloat")]
    public r_float testfloat = new r_float(4f);
    [ConfigVar("somebool", "this is a test boolean")]
    public r_bool testbool = new r_bool(true);

    [ConfigVar("player.name")]
    public r_string PlayerName = new r_string("Player");

    public override void OnEnable()
    {
        base.OnEnable();
    }
}


