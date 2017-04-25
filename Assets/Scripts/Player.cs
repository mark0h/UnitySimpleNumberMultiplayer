using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{

    #region main variables
    //When we want to debug our code
    public bool debugMode = false;

    private Text scoreText;
    private Text opponenetScoreText;
    private Text turnText;
    //private Text sceneInfoText;

    private Button gameButton;

    private PlayerData thisPlayerData;
    private PlayerData opponentPlayerData;

    private PlayerState playerState;

    [SyncVar]
    private GameState gameState;  //We want to sync this value to be the same for all players/clients/servers etc    
    #endregion


    /*
     * =======================================================================
     *   Awake,Start,Update METHODS
     * =======================================================================
     */
    #region Awake,Start,Update
    //We set what game object the scoreText and opponent score text are(the top and bottom ), the turn status text box, and the "scene info text" box which is used for debugging
    void Awake()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " Awake() Values: NetID: " + netId + " isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);

        scoreText = GameObject.Find("BottomScoreText").GetComponentInChildren<Text>();
        opponenetScoreText = GameObject.Find("TopScoreText").GetComponentInChildren<Text>();
        turnText = GameObject.Find("YourTurnText").GetComponentInChildren<Text>();
        //sceneInfoText = GameObject.Find("SceneInfoText").GetComponentInChildren<Text>();

        gameButton = GameObject.Find("ClickButton").GetComponentInChildren<Button>();


    }

    //Here we set the gameState to setup, and playerState to new game until another player arrives
    void Start()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " Start() Values: NetID: " + netId + " isLocalPlayer: " + isLocalPlayer);

        gameState = GameState.Setup;
        playerState = PlayerState.NewGame;
        UpdatePlayerScene();
        GameManager.singleton.AddPlayer(netId.Value);
    }

    public override void OnStartClient()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " OnStartClient() Values: NetID: " + netId + " isLocalPlayer: " + isLocalPlayer);

        gameObject.name = "Player " + netId.Value;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameButton.onClick.RemoveAllListeners();
        gameButton.onClick.AddListener(() => CmdPressButton());
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (playerState == PlayerState.YourTurn)
                gameButton.interactable = true;
            else
                gameButton.interactable = false;
        }
    }
    #endregion

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        GameManager.singleton.RemovePlayer(netId.Value);
    }

    //This will update the player scene
    private void UpdatePlayerScene()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " UpdatePlayerScene() Values: NetID: " + netId + " isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer + " playerState: " + playerState);

        //Only do this for the local player
        if (isLocalPlayer)
        {
            //Update score boxes
            scoreText.text = thisPlayerData.score.ToString();
            opponenetScoreText.text = opponentPlayerData.score.ToString();

            //Update yourTurnText and SceneInfoText
            switch (playerState)
            {
                case PlayerState.NewGame:
                    turnText.text = "Waiting for players...";
                    break;

                case PlayerState.OpponentTurn:
                    turnText.text = "Opponent's Turn";
                    break;

                case PlayerState.YourTurn:
                    turnText.text = "Your Turn";
                    break;
            }

        }
    }


    /*
     * =======================================================================
     *   ClientRpc METHODS
     *   These methods run on all instances of player objects
     * =======================================================================
     */
    #region ClientRpc methods
    [ClientRpc]
    public void RpcUpdateGameState(GameState newGameState)
    {
        gameState = newGameState;
        if (isLocalPlayer)
        {
            if (gameState == GameState.Setup)
                playerState = PlayerState.NewGame;
            UpdatePlayerScene();
        }
            
    }

    [ClientRpc]
    public void RpcUpdatePlayerData(PlayerData newPlayerData)
    {
        if (isLocalPlayer)
        {
            if (newPlayerData.playerID == netId.Value)
            {
                thisPlayerData = newPlayerData;
                if (thisPlayerData.currentTurn == true)
                    playerState = PlayerState.YourTurn;
                else
                    playerState = PlayerState.OpponentTurn;
            }
                
            else
                opponentPlayerData = newPlayerData;

            UpdatePlayerScene();
        }
    }

    #endregion

    [Command]
    public void CmdPressButton()
    {
        
        Debug.Log(" " + name + " PressButton() has been called isLocalPlayer: " +isLocalPlayer);
        GameManager.singleton.ClickedButton(netId.Value);
        //if (isLocalPlayer)
        //{
        //    GameManager.singleton.CmdClickedButton(netId.Value); 
        //}
    }    
}
