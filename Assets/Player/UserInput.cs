using UnityEngine;
using System.Collections;


using RTS;

public class UserInput : MonoBehaviour {

	private Player player;
	private GUIManager guiManager;

	void Start () {
		player = transform.root.GetComponent< Player >();
		guiManager = transform.GetComponent< GUIManager >();
	}
	
	void Update () {
		if (player.human) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				OpenPauseMenu();
			}	
			MoveCamera();
			RotateCamera();
			MouseActivity();
		}
	}

	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);

		bool mouseScroll = false;
		
		//horizontal camera movement
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanRight);
    		mouseScroll = true;
		}
 
		//vertical camera movement
		if (ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
    		player.hud.SetCursorState(CursorState.PanDown);
    		mouseScroll = true;
		} else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanUp);
    		mouseScroll = true;
		}

		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		// TODO to comment to see the difference
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		//away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;


		//limit away from ground movement to be between a minimum and maximum distance
		if (destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}

		//if a change in position is detected perform the necessary update
		if (destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}

		if(!mouseScroll) {
    		player.hud.SetCursorState(CursorState.Select);
		}

	}

	private void RotateCamera() {

		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;

		//detect rotation amount if ALT is being held and the Right mouse button is down
		if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1)) {
    		destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
    		destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}
 
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
    		Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}

	}


	private void MouseActivity() {
    	if ( Input.GetMouseButtonDown(0) ) {
    		LeftMouseClick();
    	} else if ( Input.GetMouseButtonDown(1) ) { 
    		RightMouseClick();
    	}

    	// Currently handle the left click up here, as it is only used for finishing the selection process
    	if (Input.GetMouseButtonUp(0)) {
		    selectionBox = false;
		    boxRect = Rect.MinMaxRect(0, 0, 0, 0);
		}


		if (selectionBox && oldMouse != Input.mousePosition) {
		    UpdateBoxSelection(selectionStart, Input.mousePosition);
		}
		oldMouse = Input.mousePosition;

		//draw box
        guiManager.setSelectionBox(selectionBox, boxRect);

    	MouseHover();
	}


    private bool selectionBox;
    private Vector3 oldMouse;
    private Vector3 selectionStart;
    private Rect boxRect;


    /*  Handle the left mouse click (down, not when the left button goes up ! )
    * 	Left button is use to select/deselect.
    *
    *	Algorithm when the left button is pressed : 
    *		- If player trying to find a location for a building
    *			-> Construct the building	
    *		- Else
    *			- If a valid object has been hitten 
    *				-> Reset the current selection, and add the hitten object to the selection (if not ground/bridge)
    *			
    *			Start the selection box. 
    *
    */
	private void LeftMouseClick() {
		Debug.Log("LEFT CLICK !");
	    // Inside the playable screen
	    if ( player.hud.MouseInBounds() ) {

	    	// Specific behaviour when player is finding a location for his new building
	        if ( player.IsFindingBuildingLocation() ) {
	            if ( player.CanPlaceBuilding() ) {
	            	player.StartConstruction();
	            }
	        } else {

	        	GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
	        	Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
	        	
	        	// Object hitten must be valid
	        	if (hitObject && hitPoint != ResourceManager.InvalidPosition) {
	            	player.ResetSelection(); 
	            	
	            	if ( hitObject.name != "Ground" && hitObject.name != "Bridge" ) {
	                	
	                	WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();	
	                	if (worldObject) {
	                    	// we already know the player has no selected object
	                    	player.selections.Add(worldObject);
	                    	worldObject.SetSelection(true, player.hud.GetPlayingArea());
	                	}	
	            	} else {
	            		// TODO put here the selection box start?
	            	}
	        	}

		        //selection box
		        if ( Input.GetMouseButtonDown(0) ) {
		            selectionBox = true;
		            selectionStart = Input.mousePosition;
		        }

	        }
	    }

	}


    private void UpdateBoxSelection(Vector3 from, Vector3 to)
    {
        RaycastHit hit1, hit2;
        Ray mouseRay1 = Camera.main.ScreenPointToRay(from);
        Ray mouseRay2 = Camera.main.ScreenPointToRay(to);
        if (Physics.Raycast(mouseRay1, out hit1, 100) && Physics.Raycast(mouseRay2, out hit2, 100))
        {
            //draw box info
            float minx = Mathf.Min(from.x, to.x);
            float miny = Mathf.Min(from.y, to.y);
            float maxx = Mathf.Max(from.x, to.x);
            float maxy = Mathf.Max(from.y, to.y);
            float w = maxx - minx;
            float h = maxy - miny;
            //not double click?
            if (w != Screen.width || h != Screen.height)
            {
                boxRect = Rect.MinMaxRect(minx, Screen.height - maxy, maxx, Screen.height - miny);
            }
            //selection
            player.BoxSelection(hit1.point, hit2.point);
        }
    }



    /*  Handle the right mouse click (down, not when the right button goes up ! )
    * 	Right button is use to perform actions (move, attack, etc.).
    *
    *	Algorithm when the right button is pressed : 
    *		- If player trying to find a location for a building
    *			-> Cancel the placement.
    *		- Else
    *			Perform actions for each selected object.		
    *
    */
	private void RightMouseClick() {
		Debug.Log("RIGHT CLICK");
	    if (player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.selections.Count > 0  ) {
	        if (player.IsFindingBuildingLocation()) {
	            player.CancelBuildingPlacement();
	        } else {

	        	GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
	        	Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
	        	
	        	lock (player.selections) {
		        	foreach (WorldObject obj in player.selections) {
		        		Debug.Log("Action will start");
		        		obj.MouseClick( hitObject, hitPoint, player );
		        		Debug.Log("Action will end : " + player.selections.Count);
	        		}
        		}
	        }
	    } 
	}


	private void MouseHover() {
    	if(player.hud.MouseInBounds()) {
        	if(player.IsFindingBuildingLocation()) {
            	player.FindBuildingLocation();
        	} else {	
    			if(player.hud.MouseInBounds()) {
        			GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
        			//print(hoverObject);
        			if(hoverObject) {
            			if ( player.selections.Count > 0 ) 
            				// TODO put the HoverState to the most appropriate object in the list (-> priority : Attack -> collect -> move)
            				player.selections[0].SetHoverState(hoverObject);
            			else if (hoverObject.name != "Ground" && hoverObject.name != "Bridge") {
                			Player owner = hoverObject.transform.root.GetComponent< Player >();

                			if (owner) {
                    			Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
                    			Building building = hoverObject.transform.parent.GetComponent< Building >();
                    	
                    			if (owner.username == player.username && (unit || building)) 
                    				player.hud.SetCursorState(CursorState.Select);
                			}
            			}
        			}
    			}
			}
    	}
	}


	// *** Menu Methods ***
	private void OpenPauseMenu() {
	    Time.timeScale = 0.0f;
	    GetComponentInChildren< PauseMenu >().enabled = true;
	    // disable the user input as the player need to interact with the menu, not the game
	    GetComponent< UserInput >().enabled = false;			
	    Cursor.visible = true;
	    ResourceManager.MenuOpen = true;
	}

}
