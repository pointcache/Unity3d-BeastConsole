using System.Collections;
using System.Collections.Generic;
using BeastConsole;
using UnityEngine;

public class MultilineExample : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Console.WriteLine("Welcome to BeastConsole Example. This is and example of a multiline message. \n" +
            "- Backend is created by folks at CraniumSoftware. \n" +
            "- Register your own arbitrary commands/methods and provide parameters \n" +
            "- Autocomplete \n" +
            "- Add variables \n" +
            "- Both commands and variables support any number of subscribers, modify all objects at once.");
	}
	
	
}
