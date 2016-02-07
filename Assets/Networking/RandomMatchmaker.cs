using UnityEngine;
using Photon;
using RTS;
 
public class RandomMatchmaker : Photon.PunBehaviour
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

		// Initialize player
		Vector3 playerPosition = new Vector3(10, 0, 10);
		GameObject playerObject = PhotonNetwork.Instantiate("Player", playerPosition, Quaternion.identity, 0); 
		Player myPlayer = playerObject.GetComponent<Player>();
		myPlayer.name = "MyNetworkPlayer" + playerID; // TODO must come from a GUI

		// Update name of children
		playerObject.GetComponentInChildren<Units>().name += playerID;
		playerObject.GetComponentInChildren<Buildings>().name += playerID;
		playerObject.GetComponentInChildren<RallyPoint>().name += playerID;


		// Enable the scripts related to the player
	 	myPlayer.transform.GetComponent<UserInput>().enabled = true;
	 	myPlayer.transform.GetComponent<GUIManager>().enabled = true;

		// Get the Units object
		Units units = playerObject.GetComponentInChildren< Units >();

		// Create units
		GameObject builder1 = PhotonNetwork.Instantiate("Builder", playerPosition, Quaternion.identity, 0);
		builder1.name += playerID;
		builder1.transform.parent = units.transform;

		GameObject tank1 = PhotonNetwork.Instantiate("Tank", playerPosition, Quaternion.identity, 0);
		tank1.name += playerID;
		tank1.transform.parent = units.transform;

		/*
		tank1.name = tank1.GetInstanceID().ToString();
		Debug.Log("Instance ID  : " + tank1.GetInstanceID());
        Debug.Log("Ovject : " + GameObject.Find(tank1.GetInstanceID().ToString()) );
	*/



/*

		// TODO : TEST CODE, TO REMOVE 

	    GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

	    Player player = null;
	    // TODO dirty hack - hardcoded currently - will need to create the player dynamically, and sending a RPC to other clients to add the Player to the PlayerList.
	    foreach (GameObject obj in playerObjects) {
	    	if ( obj.GetComponent<Player>().name == "Player" + PhotonNetwork.player.ID) {
	    		player = obj.GetComponent<Player>();
	    	}
	    }
	   
	 	Debug.Log("Player : " + player);
*/
	 	// Enable the script for the right player.
	 	//player.transform.GetComponent<UserInput>().enabled = true;
	 	//player.transform.GetComponent<GUIManager>().enabled = true;


	}
}