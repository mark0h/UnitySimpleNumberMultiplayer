using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerScore : NetworkBehaviour {

    public int playerScore = 0;


    public void AddScore(int amount)
    {
        playerScore += amount;
        if (isLocalPlayer)
        {
            NumberManager.singleton.localScore += amount;
        } else
        {
            NumberManager.singleton.remoteScore += amount;
        }
    }
}
