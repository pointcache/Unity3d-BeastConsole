using UnityEngine;
using System;
using System.Collections.Generic;
using BeastConsole;

public class ExampleConfigAttribute : MonoBehaviour {

    [ConsoleVariable("player.money", "How much money player has")]
    public float PlayerMoney = 10000f;

    private int _playerPhysicsLayer;
    [ConsoleVariable("player.physicsLayer")]
    public int PlayerPhysicsLayer
    {
        set {
            BeastConsole.Console.WriteLine("Player Physics Layer was set to : " + value);
            _playerPhysicsLayer = value;
        }
        get {
            return _playerPhysicsLayer;
        }
    }
}
