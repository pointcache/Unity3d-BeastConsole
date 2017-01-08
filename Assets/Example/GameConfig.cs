using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BeastConsole;
using rvar;

public class GameConfig : ConfigBase {

    public static GameConfig cfg;

    [ConfigVar("game.cubesize", "size of cube")]
    public r_float Size = new r_float(1f);


    public override void OnEnable()
    {
        base.OnEnable();
        cfg = this;
    }
}
