using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    public delegate void GameEvent();
    public static event GameEvent pressButton;
    
        public static void triggerButtonPress()
    {
        if (pressButton != null)
            pressButton();
    }
}
