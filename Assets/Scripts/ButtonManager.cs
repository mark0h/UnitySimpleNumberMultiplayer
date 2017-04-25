using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ButtonManager : NetworkBehaviour {

	// Use this for initialization
	void Start ()
    {
        Debug.Log("Button!!! NetId: " + netId);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
