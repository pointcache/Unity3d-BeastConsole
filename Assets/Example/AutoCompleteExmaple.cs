using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;

public class AutoCompleteExmaple : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Console.AddCommand("crab","", this, null);
        Console.AddCommand("crusty","", this, null);
        Console.AddCommand("credentials","", this, null);
        Console.AddCommand("credentialisation","", this, null);
        Console.AddCommand("credentialisationed","", this, null);
        Console.AddCommand("credulity","", this, null);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
