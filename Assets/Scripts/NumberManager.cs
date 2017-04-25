using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NumberManager : NetworkBehaviour
{
    public static NumberManager singleton;
    public List<Player> players = new List<Player>();
    public Player currentTurnPlayer;
    public int currentPlayerIndex = 99;

    [SyncVar]
    public int playerCount;
    [SyncVar]
    public int localScore = 0;
    [SyncVar]
    public int remoteScore = 0;

    public Button buttonClick;

    public Text yourTurn;
    public Text topScoreText;
    public Text bottomScoreText;
    public Text sceneInfoText;
    public Text scoreInfoText;

    private bool gameReady = false;

    public void Awake()
    {
        singleton = this;
        yourTurn.text = "";
        bottomScoreText.text = remoteScore.ToString();
        topScoreText.text = localScore.ToString();
        Debug.Log("NumberManager Awake()");
    }
    
    //Need to wait until 2 players before starting game. 
    public void Update()
    {
        if (playerCount < 2)
        {
            buttonClick.interactable = false;
            if (isLocalPlayer)
                Debug.Log("Less than 2 players, setting yourturn.text");
                yourTurn.text = "waiting for players...";
            return;
        }            

        if (gameReady)
            return;

        //yourTurn.text = "Opponent's Turn";
        gameReady = true;
        NextPlayer();
    }

    public void ResetGame()
    {
        players = new List<Player>();
        currentPlayerIndex = 99;
        currentTurnPlayer = null;
        localScore = 0;
        remoteScore = 0;
        bottomScoreText.text = remoteScore.ToString();
        topScoreText.text = localScore.ToString();
        buttonClick.interactable = false;
        gameReady = false;
    }

    //Add player to players list
    public void AddPlayer(Player player)
    {
        players.Add(player);
        playerCount = players.Count;
    }

    //Remove player from players list, and reset game variables
    public void RemovePlayer(Player player)
    {
        ResetGame();
    }

    [Server]
    public void NextPlayer()
    {
        if (players.Count < 2)
            return;
        if(currentPlayerIndex == 99)  //It only equals 99 at the start of a new game, so will randomly choose player to go first
        {
            currentPlayerIndex = Random.Range(0, 2);
        }
        if(currentTurnPlayer != null) //Not a new game, so sets current player's turn to false
        {
            //currentTurnPlayer.RpcYourTurn(false, (currentPlayerIndex + 1));            
        }

        //Sets next player's turn to true
        currentPlayerIndex = (currentPlayerIndex - 1) * -1;
        currentTurnPlayer = players[currentPlayerIndex];
        //currentTurnPlayer.RpcYourTurn(true, (currentPlayerIndex + 1));        
    }    
    

    [Client]
    public void EnableButton()
    {
        buttonClick.interactable = true;        
        yourTurn.text = "YOUR TURN";        
    }

    [Client]
    public void DisableButton()
    {
        buttonClick.interactable = false;
        
        yourTurn.text = "Opponent's Turn";
        //currentTurnPlayer.scoreInfoBox.text = "You just played your turn!";
    }
    

    //[Command]
    public void ClickTheButton()
    {
        Debug.Log(" ClickTheButton() called");
        
        //currentTurnPlayer.GetComponent<Player>().CmdPressButton();
        
    }

    [Client]
    public void UpdateScoreBoxes(bool serverPlayer)
    {
        Debug.Log("Updating scores. Server? " + serverPlayer);
        if (serverPlayer)
        {
            bottomScoreText.text = localScore.ToString();
            topScoreText.text = remoteScore.ToString();
            sceneInfoText.text = "You are the server and your score is " + localScore.ToString();
        } else
        {
            bottomScoreText.text = remoteScore.ToString();
            topScoreText.text = localScore.ToString();
            sceneInfoText.text = "You are the remote  and your score is " + remoteScore.ToString();
        }
    }
}
