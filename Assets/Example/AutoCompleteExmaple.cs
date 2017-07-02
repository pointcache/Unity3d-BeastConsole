using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;

public class AutoCompleteExmaple : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Console.RegisterCommand("crab","", this, null);
        Console.RegisterCommand("crusty","", this, null);
        Console.RegisterCommand("credentials","", this, null);
        Console.RegisterCommand("credentialisation","", this, null);
        Console.RegisterCommand("credentialisationed","", this, null);
        Console.RegisterCommand("credulity","", this, null);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
