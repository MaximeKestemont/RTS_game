﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Pathfinding;

public class Unit : WorldObject {

	public float moveSpeed, rotateSpeed;

	protected bool moving, rotating;   // TODO refactor this in a state class
 
	private Vector3 destination;
	private Quaternion targetRotation;
    private GameObject destinationTarget;

    private Seeker seeker;                       // seeker to ask a path to the A*
    private Path path;                           // calculated path by the A*
    public float nextWaypointDistance = 3;       // max distance from a waypoint to continue to the next waypoint
    private int currentWaypoint = 0;             // the waypoint we are currently moving towards

    // Audio related variables 
    public AudioClip moveSound;
    public float moveVolume = 1.0f;

    // Variable to know if the position has changed or not (only relevant in multiplayer mode)
    private Vector3 oldPosition = new Vector3(0, 0, 0); 


    // Method to initialize the unit, make the name corresponding to the player and sending the parent object name to the network
    // so that other players can re-create the object hierarchy
    public static GameObject InstantiateUnit(GameObject playerObject, string prefabName, Vector3 unitPosition, Quaternion unitQuaternion) 
    {
        int playerID = PhotonNetwork.player.ID;

        // Get the Units object
        Units units = playerObject.GetComponentInChildren< Units >();

        // Store the name of the parent 
        object[] data = new object[1];
        data[0] = units.name;           // parent name

        // Create the unit, and send the parent name so that it can be retrieved from other player views to re-build the hierarchy
        GameObject unitObject = PhotonNetwork.Instantiate(prefabName, unitPosition, unitQuaternion, 0, data);
        unitObject.name = prefabName + playerID;
        unitObject.transform.parent = units.transform;

        return unitObject;
    }


	/*** Game Engine methods, all can be overridden by subclass ***/

	protected override void Awake() {
		base.Awake();

        //Get a reference to the Seeker component we added earlier
        seeker = GetComponent<Seeker>();

        // TODO remove either the one from awake or from start (does not work if only in awake...)
        if (transform.root.GetComponent< Player >()) {
            Debug.Log("Add unit to player ");
            transform.root.GetComponent< Player >().AddUnitInList(this);
        }
	}

	protected override void Start() {
		base.Start();


        if (transform.root.GetComponent< Player >()) {
            Debug.Log("Add unit to player ");
            transform.root.GetComponent< Player >().AddUnitInList(this);
        }
	}

	protected override void Update () {
    	base.Update();

    	if (rotating) 
    		TurnToTarget();
    	else if (moving) 
    		MakeMove();
        // Check if the position has changed. If it has, update the bounds.
        // Necessary for the multiplayer, as the position is synced, but not the bounds.
        else if (oldPosition != transform.position) {
            oldPosition = transform.position;
            CalculateBounds();
        }

        // Update the list of nearby objects // TODO to refactor and try to find the nearby object of the nearby object !
        nearbyObjects = WorkManager.FindNearbyObjects(transform.position, detectionRange);

        // Avoid the stacking of units.
        CalculateFlocking();
	}

	protected override void OnGUI() {
		base.OnGUI();
	}


    // Call back method from the seeker, taking the path as argument.
    public void OnPathComplete (Path p) {
        //Debug.Log ("Object " + objectId + " Path Computed. | Did it have an error? " + p.error);
        if ( !p.error ) {
            path = p;
            
            //Reset the waypoint counter
            currentWaypoint = 0;
            
            // Initial rotation to the first waypoint
            targetRotation = Quaternion.LookRotation (path.vectorPath[0] - transform.position);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);      // Lock the rotation for the x and z axis
        }
    }

    public void OnDisable () {
        seeker.pathCallback -= OnPathComplete;
    }


    // No decision to take when moving or rotating
    protected override bool ShouldMakeDecision () {
        if (moving || rotating) 
            return false;
        else 
            return base.ShouldMakeDecision();
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    // Called when the unit is created (useful to link the unit to the building having created it, etc.)
    public virtual void SetBuilding(Building creator) {
        //specific initialization for a unit can be specified here
    }   

	public override void SetHoverState(GameObject hoverObject) {
    	base.SetHoverState(hoverObject);
    	//only handle input if owned by a human player and currently selected
    	if (player && player.human && currentlySelected) {
        	bool moveHover = false;
            if ( hoverObject.name == "Ground" || hoverObject.name == "Bridge" ) {
                moveHover = true;
            } else {
                Resource resource = hoverObject.transform.parent.GetComponent< Resource >();
                if (resource && resource.isEmpty()) {
                    moveHover = true;
                }
            }
            if(moveHover) player.hud.SetCursorState(CursorState.Move);
    	}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
    	base.MouseClick(hitObject, hitPoint, controller);
    	
        //only handle input if owned by a human player and currently selected
    	if (player && player.human && currentlySelected) {
            bool clickedOnEmptyResource = false;
            if (hitObject.transform.parent) {
                Resource resource = hitObject.transform.parent.GetComponent< Resource >();
                if (resource && resource.isEmpty()) {
                    clickedOnEmptyResource = true;
                }
            }
            // If valid ground OR an empty resource, the unit can move to the click destination
        	if ( (hitObject.name == "Ground" || hitObject.name == "Bridge" || clickedOnEmptyResource ) && hitPoint != ResourceManager.InvalidPosition) {
            	float x = hitPoint.x;
            	//makes sure that the unit stays on top of the surface it is on
            	float y = hitPoint.y + this.transform.position.y; //player.SelectedObject.transform.position.y;
            	float z = hitPoint.z;
            	Vector3 destination = new Vector3(x, y, z);
            	StartMove(destination);
        	}
    	}
	}

	public virtual void StartMove(Vector3 destination) {
        if (audioElement != null) 
            audioElement.Play (moveSound);
    	this.destination = destination;
        destinationTarget = null;
    	rotating = true;
    	moving = false;
        path = null;            // Important : delete the old path, so that it can generate a new one without interference

        // Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath (transform.position, this.destination, OnPathComplete);
	}	

    public virtual void StartMove(Vector3 destination, GameObject destinationTarget) {
        StartMove(destination);
        this.destinationTarget = destinationTarget;
    }

	private void TurnToTarget() {
    	transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
    	//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
    	Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
    	if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
        	rotating = false;
        	moving = true;
    	}
    	CalculateBounds();

        // TODO refactor that without using this global destinationTarget where the behaviour differs if null or not...
        if ( destinationTarget ) {
            CalculateTargetDestination();
        }
	}

	private void MakeMove() 
    {
        //Debug.Log ("Object " + objectId + "| Make move");
        if (path == null) {
            //We have no path to move after yet
            return;
        }

        // TODO currently, it is stopping a bit before the last point because of the "NextWayPoint Distance". Need to adjust the target, or to 
        // have special logic for the last point.
        if (currentWaypoint >= path.vectorPath.Count) {
            Debug.Log ("Object " + objectId + "| End Of Path Reached");
            moving = false;
            movingIntoPosition = false;
            return;
        }

        //Direction to the next waypoint
        transform.position = Vector3.MoveTowards(transform.position, path.vectorPath[currentWaypoint], Time.deltaTime * moveSpeed);
        
        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
            currentWaypoint++;

            // Rotate towards the new waypoint
            if (currentWaypoint < path.vectorPath.Count) {
                targetRotation = Quaternion.LookRotation (path.vectorPath[currentWaypoint] - transform.position);

                // Adjust the rotation if too small of a change, but do not stop the movement.
                if (Quaternion.Angle(targetRotation,transform.rotation) > 15 ) {
                    rotating = true;
                    //moving = false;
                }
            }
            return;
        }

    	CalculateBounds();
	}

    private void CalculateTargetDestination() {
        // Calculate number of unit vectors from unit centre to unit edge of bounds
        Vector3 originalExtents = selectionBounds.extents;
        Vector3 normalExtents = originalExtents;
        normalExtents.Normalize();
        float numberOfExtents = originalExtents.x / normalExtents.x;
        int unitShift = Mathf.FloorToInt(numberOfExtents);
        
        // Calculate number of unit vectors from target centre to target edge of bounds
        WorldObject worldObject = destinationTarget.GetComponent< WorldObject >();
        if ( worldObject ) {
            originalExtents = worldObject.GetSelectionBounds().extents;
        } else {
            originalExtents = new Vector3(0.0f, 0.0f, 0.0f);
        }
        normalExtents = originalExtents;
        normalExtents.Normalize();
        numberOfExtents = originalExtents.x / normalExtents.x;
        int targetShift = Mathf.FloorToInt(numberOfExtents);
        
        //calculate number of unit vectors between unit centre and destination centre with bounds just touching
        int shiftAmount =  targetShift + unitShift;
        
        // calculate direction unit needs to travel to reach destination in straight line and normalize to unit vector
        Vector3 origin = transform.position;
        Vector3 direction = new Vector3(destination.x - origin.x, 0.0f, destination.z - origin.z);
        direction.Normalize();

        // destination = center of destination - number of unit vectors calculated above
        // this should give a destination where the unit will not quite collide with the target
        // giving the illusion of moving to the edge of the target and then stopping
        destination -= direction*shiftAmount;

        destination.y = destinationTarget.transform.position.y;
        //Debug.Log("Destination " + destination);
        destinationTarget = null;
    }


    // TODO REFACTOR WITH A REAL FLOCKING ALGORITHM
    private void CalculateFlocking()
    {
        Vector3 ret = Vector3.zero;

        foreach ( WorldObject nearbyObject in nearbyObjects ) {

            Unit unit = nearbyObject as Unit;

            if ( unit != null && this != unit ) {
                Vector3 delta = this.transform.position - unit.transform.position;
                float dist = delta.magnitude;
                // minimal distance required between the 2 objects
                float mindist = 2; // TODO to replace with the bounds computation ... this.transform.Radius + unit.transform.Radius; 
                
                // If too close, move the unit a bit on the delta direction 
                if ( dist < mindist ) {
                    //if ( dist < 0.1 ) delta = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    float weight = mindist - dist;
                    if ( weight < 0 ) weight = 0;
                    ret += delta.normalized * weight;
                }
            }

        }

        this.transform.position = new Vector3(this.transform.position.x + ret.x, this.transform.position.y, this.transform.position.z + ret.z) ;
    }



    // Display the tooltip for buildings that the unit can create
    public override void DisplayActionTooltip(string tooltipName) {
        base.DisplayActionTooltip(tooltipName);

        GameObject obj = ResourceManager.GetBuilding(tooltipName);

        if ( obj && obj.GetComponent<Building>() ) {
            Building building = obj.GetComponent<Building>();
            GUI.skin = null;

            GUI.BeginGroup(new Rect(
                Screen.width - ResourceManager.ORDERS_BAR_WIDTH - ResourceManager.TOOLTIP_WIDTH + ResourceManager.MARGIN, 
                ResourceManager.RESOURCE_BAR_HEIGHT + ResourceManager.MARGIN, 
                ResourceManager.TOOLTIP_WIDTH, 
                ResourceManager.TOOLTIP_HEIGHT));
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), ""); // put the texture here instead of ""

            float x_offset = 0;
            float y_offset = 0;

            // Display the name of the object
            GUI.Label (new Rect( 
                ResourceManager.MARGIN, 
                y_offset += ResourceManager.MARGIN, 
                Screen.width, 
                Screen.height), 
                "" + building.objectName);

            // Display the gold cost (below the object name)
            if ( building.costValue.Length > 0 ) {
                GUI.Label (new Rect(
                    ResourceManager.MARGIN * 2, 
                    y_offset += ResourceManager.TEXT_HEIGHT, 
                    Screen.width, 
                    Screen.height), 
                    "" + building.costValue[0]);
            }
                
            // Display the gold icon (right of the cost)
            GUI.DrawTexture(new Rect(
                ResourceManager.MARGIN * 2 + 30, 
                y_offset, 
                ResourceManager.RESOURCE_IMAGE_WIDTH / 3, 
                ResourceManager.RESOURCE_IMAGE_HEIGHT / 3), 
                ResourceManager.GoldImage);


            // Display the requirements
            bool firstRequirement = true;       // to only draw the label once

            foreach (string requirement in building.GetBuildingNameRequirements()) {
                if ( firstRequirement ) {
                    GUI.Label (new Rect( 
                        x_offset += ResourceManager.MARGIN, 
                        y_offset += ResourceManager.RESOURCE_IMAGE_WIDTH / 3, 
                        Screen.width, 
                        Screen.height), 
                        "Needs : ");
                    firstRequirement = false;
                }

                // Draw the image of the requirement
                Texture2D requirementImage = ResourceManager.GetBuilding(requirement).GetComponent<Building>().GetBuildImage();
                GUI.DrawTexture(new Rect(
                    x_offset, 
                    ResourceManager.MARGIN + ResourceManager.TEXT_HEIGHT + 35, 
                    ResourceManager.RESOURCE_IMAGE_WIDTH / 1.5f, 
                    ResourceManager.RESOURCE_IMAGE_HEIGHT / 1.5f), 
                    requirementImage);

                // Update the x offset 
                x_offset += ResourceManager.RESOURCE_IMAGE_WIDTH / 1.5f;
            }

            GUI.EndGroup();
        }    
    }



    protected override void InitialiseAudio () {
        base.InitialiseAudio ();
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        if(moveVolume < 0.0f) moveVolume = 0.0f;
        if(moveVolume > 1.0f) moveVolume = 1.0f; 
        sounds.Add(moveSound);
        volumes.Add(moveVolume);
        audioElement.Add(sounds, volumes);
    }

}