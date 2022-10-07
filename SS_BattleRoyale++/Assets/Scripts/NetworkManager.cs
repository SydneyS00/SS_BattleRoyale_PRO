using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 10;


    //instance for Network Manager script
    public static NetworkManager instance;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //connect to Photon
    void Start()
    {
        //connect to the master server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //allows the user to see the different lobbies on the server
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom(string roomName)
    {
        //room settings/options when room is created
        //setting the max number of players to 10 
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;

        //creating a room 
        PhotonNetwork.CreateRoom(roomName, options);
    }

    //joins a room of the requested room name
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    //changes the scene through the system (photon)
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    //Disconnect stuff
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Menu");
    }
    
    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();

        if(PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }
    }
}
