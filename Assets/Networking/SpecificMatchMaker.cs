﻿using UnityEngine;
using Photon;
using RTS;
 

// Class to spawn in a specific scene, with players and units already created through the Unity Editor
public class SpecificMatchMaker : Photon.PunBehaviour
{
    // Use this for initialization
    void Start()
    {
    	// Set the log level of Photon
    	//PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("0.1");
    }
 
    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString() + " Player : " + PhotonNetwork.player.ID);
    }

    // TODO : this is called automatically because "Auto-join lobby" is checked. Should change that at a point.
    public override void OnJoinedLobby()
	{
    	PhotonNetwork.JoinRandomRoom();
	}

	// If the auto-join fails, it means that there is no room, and that one must be created.
	void OnPhotonRandomJoinFailed()
	{
	    Debug.Log("Can't join random room!");
	    PhotonNetwork.CreateRoom(null);
	}

	void OnJoinedRoom()
	{

        Debug.Log("Current player : " + PhotonNetwork.player.ID);
        
        Vector3 playerPosition = new Vector3(0, 7, 0);

        // TODO to move in the future lobby script
        // Starting position for each player
        switch (PhotonNetwork.player.ID) {
            case 1:
                playerPosition.x = 30;
                break;
            case 2: 
                playerPosition.x = -30;
                playerPosition.z = 30;
                break;
            default :
                break;
        }


        // Creation of the player + 2 units
        GameObject playerObject = Player.InstantiatePlayer(playerPosition);
        Unit.InstantiateUnit(playerObject, "Builder", new Vector3(playerPosition.x + 2, playerPosition.y, playerPosition.z), Quaternion.identity);
        Unit.InstantiateUnit(playerObject, "Tank", playerPosition, Quaternion.identity);
        Building.InstantiateBuilding(playerObject, "Refinery",  new Vector3(playerPosition.x + 5, playerPosition.y, playerPosition.z), Quaternion.identity);

	}
}