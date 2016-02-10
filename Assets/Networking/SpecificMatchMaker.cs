using UnityEngine;
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
		int playerID = PhotonNetwork.player.ID;




	}
}