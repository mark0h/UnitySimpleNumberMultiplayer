using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{

    public Text playerText;


    private void Awake()
    {
        playerText = GetComponentInChildren<Text>();
    }

    // Use this for initialization
    void Start()
    {

    }


}
