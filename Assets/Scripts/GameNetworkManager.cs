using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager {

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server has stopped");
        //GameManager.singleton.ResetGame();
    }
    
}
