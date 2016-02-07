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
 
    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine) {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
            transform.name = name;
            //Debug.Log("Object : " + this);
            //Debug.Log("Name : " + transform.name);

            if (parentName != "") {
            	//Debug.Log("Parent name : " + parentName);
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
           
            //Debug.Log("Instance ID  : " + this.GetInstanceID());
            //Debug.Log("Ovject : " + GameObject.Find(this.GetInstanceID().ToString()) );
            // If no parent, send an empty string
            if (transform.parent) {
            	stream.SendNext(transform.parent.name);
        	} else {
        		stream.SendNext("");
        	}

        	stream.SendNext(transform.name);
 		
 		} else {
        
            // Network player, receive data
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
            this.parentName = (string)stream.ReceiveNext();
            this.name = (string)stream.ReceiveNext();
        }
    }
}