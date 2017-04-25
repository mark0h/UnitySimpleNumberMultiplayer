using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

#region enum and struct
public enum GameState { Setup, Play, Waiting, End }
public enum PlayerState { NewGame, YourTurn, OpponentTurn}

public struct PlayerData
{
    public uint playerID;
    public int score;
    public bool currentTurn;
}
#endregion

public class GameManager : NetworkBehaviour {


    #region main variables
    public static GameManager singleton;
    //When we want to debug our code
    public bool debugMode = false;

    private GameState gameState;
    private List<PlayerData> playerDataList;

    public int playersRequired = 2; //we can incease this if we want more players
    #endregion

    public void Awake()
    {
        singleton = this;
    }

    //When GameManager starts, create a new playerDataList and players list
    void Start ()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " Start() Values: NetID: " + netId + " isServer: " + isServer);

        playerDataList = new List<PlayerData>();
	}

    //When the client starts, we wait for another player
    public override void OnStartClient()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " OnStartClient() Values: NetID: " + netId + " isServer: " + isServer);

        StartCoroutine(WaitForPlayers());
    }

    //When a player is added
    public void AddPlayer(uint playerID)
    {
        PlayerData newPlayer = new PlayerData();
        newPlayer.playerID = playerID;
        newPlayer.score = 0;
        newPlayer.currentTurn = false;

        playerDataList.Add(newPlayer);
    }

    //When a player drops
    public void RemovePlayer(uint playerID)
    {
        playerDataList.RemoveAll(item => item.playerID == playerID);
        uint currentPlayerID = playerDataList[0].playerID;
        Debug.Log(" " + name + " RemovePlayer() is keeping player with the ID of: " + currentPlayerID);
        ResetGame(currentPlayerID);
    }

    private void ResetGame(uint currentPlayerID)
    {
        playerDataList = new List<PlayerData>();
        PlayerData currentPlayer = new PlayerData();
        currentPlayer.playerID = currentPlayerID;
        currentPlayer.score = 0;
        currentPlayer.currentTurn = false;
        playerDataList.Add(currentPlayer);
        gameState = GameState.Setup;
        SyncGameState();
        StartCoroutine(WaitForPlayers());
    }
    //This is called when changing data on a player(increasing score, setting turn, etc)
    private void UpdatePlayerData(PlayerData newData)
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " UpdatePlayerData() Values: playerDataList.Count: " + playerDataList.Count + "NetID: " + netId + " isServer: " + isServer);

        for (int i = 0;i<playerDataList.Count; i++)
        {
            if(playerDataList[i].playerID == newData.playerID)
            {
                playerDataList[i] = newData;
                return;
            }
        }

        playerDataList.Add(newData);
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + " UpdatePlayerData() Values: playerDataList.Count: " + playerDataList.Count);

    }

    /*
     * =======================================================================
     *   GAME DATA SYNCING METHODS
     *   These will send out information to each player prefab that the netowrk manager has created, to sync data about the game
     *   More can be added as info is needed
     * =======================================================================
     */
#region  Game Sync Methods
    private void SyncGameState()
    {
        if (debugMode)
            Debug.Log("Syncing GameState to: " + gameState);

        //Find all player prefabs and put in a list
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        //loop through each player and update their gamestate to the server gamestate
        foreach(GameObject player in players)
        {
            Player playerScript = player.GetComponent<Player>();
            playerScript.RpcUpdateGameState(gameState);
        }
        
    } 

    private void SyncPlayerData()
    {
        if (debugMode)
            Debug.Log("Syncing PlayerData ");

        //Find all player prefabs and put in a list
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        //Loop through each player, then loop through each playerData we have in the list and send it out. The Player script will determine which one is their own in the list to update with
        foreach(GameObject player in players)
        {
            Player playerScript = player.GetComponent<Player>();
            
            foreach(PlayerData data in playerDataList)
            {
                playerScript.RpcUpdatePlayerData(data);
            }
        }
    }
    #endregion

    //[Command]
    public void ClickedButton(uint playerID)
    {
        Debug.Log("Button clicked by playerID: " + playerID);
        CmdAddPlayerPoints(playerID, 1);
    }

    [Command]
    public void CmdAddPlayerPoints(uint playerID, int score)
    {
        if (isServer)
        {
            PlayerData data = GetPlayerData(playerID);
            data.score += score;
            UpdatePlayerData(data);
            CmdSetPlayerTurn(2);

            SyncPlayerData();
        }
    }

    [Command]
    public void CmdSetPlayerTurn(int turnNumber)
    {
        if (debugMode)
            Debug.Log("CmdSetPlayerTurn()" + " isServer: " + isServer + " playerDataList size: " + playerDataList.Count);

        int playerIDTurn = Random.Range(2, 4);
        if (isServer)
        {
            for (int i = 0; i < playerDataList.Count; i++)
            {
                PlayerData tempData = playerDataList[i];
                if (turnNumber == 1) //First turn
                {
                    if (playerDataList[i].playerID == playerIDTurn)
                        tempData.currentTurn = true;
                } else
                {
                    if (tempData.currentTurn == true)
                        tempData.currentTurn = false;
                    else
                        tempData.currentTurn = true;
                }
                UpdatePlayerData(tempData);
                SyncPlayerData();
            }
            
                
        }
    }

    private PlayerData GetPlayerData(uint playerID)
    {
        if (debugMode)
            Debug.Log(" " + name + "GetPlayerData()" + " playerID: " + playerID);
        foreach (PlayerData player in playerDataList)
        {
            if (player.playerID == playerID)
                return player;
        }

        PlayerData newData = new PlayerData();
        newData.playerID = playerID;
        newData.score = 0;
        newData.currentTurn = false;

        return newData;
    }

    //This is our Wait for second player Coroutine. It will stay in the yield loop until a second player arrives
    private IEnumerator WaitForPlayers()
    {
        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + "WaitForPlayers() Values: NetID: " + netId + " isServer: " + isServer);

        yield return new WaitForSeconds(1);

        if (debugMode)
            Debug.Log(" WaitForSeconds(1)" + name + ": " + GetType() + "WaitForSecondPlayer() Values: NetID: " + netId + " isServer: " + isServer);

        if (isServer)// && isLocalPlayer)
        {
            Debug.Log("TotalPlayerObjects(): " + TotalPlayerObjects() + "playersRequired: " + playersRequired);
            while (TotalPlayerObjects() < playersRequired) //Still dont have the player count we want, in this case only 2(other games maybe 2+?). so we still yield
            {
                Debug.Log("WaitForPlayers()  Waiting for more players...");
                yield return new WaitForSeconds(1);
            }

            //We have the player count we want!
            if (debugMode)
                Debug.Log("We have all players!");
            gameState = GameState.Play;
            CmdSetPlayerTurn(1);
            SyncGameState();
        }
    }

    //This returns the player object count
    private int TotalPlayerObjects()
    {
        GameObject[] playerPreFabs = GameObject.FindGameObjectsWithTag("Player");

        if (debugMode)
            Debug.Log(" " + name + ": " + GetType() + "TotalPlayerObjects() Values: playerPreFabs.Length: " + playerPreFabs.Length + " isServer: " + isServer);

        return playerPreFabs.Length;
    }
}
