using UnityEngine;
using Photon;
 
// This class allows information to be passed to other clients, so that they can update the position of the 
// character in a smooth way.
public class NetworkCharacter : Photon.MonoBehaviour
{
    private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
    private string parentName = "";
    private string name;
 
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine) {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
            transform.name = name;

            // If the parent name is not yet set, set it // TODO move this method in Start (but it crash...)
            if (parentName != "" && transform.parent == null ) {
            	if (GameObject.Find(parentName)) transform.parent = GameObject.Find(parentName).transform;
            }
        }
    }
 
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting) {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        	stream.SendNext(transform.name);			// TODO sent it in the data during the Instantiation
 		
 		} else {
        
            // Network player, receive data
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
            this.name = (string)stream.ReceiveNext();
        }
    }

    // Method to get data from the Instantiation of the object
    void OnPhotonInstantiate(PhotonMessageInfo info) {
    	object[] data = this.gameObject.GetPhotonView().instantiationData;

    	// If only one element, then it is the parent element
    	if (data != null && data.Length == 1) {
    		this.parentName = (string)data[0];
    	}
    }
}