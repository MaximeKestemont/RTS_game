﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using RTS;

public class Player : MonoBehaviour {

	public HUD hud;
	public string username;
	public bool human;
	public Color teamColor; // TODO should be the player color, not the team color
	private Team team;
	private GenericRace race;

	private List<Unit> unitsList = new List<Unit>();
	private List<Building> buildingsList = new List<Building>();
	public List<WorldObject> selections;

	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	public Dictionary< ResourceType, int > resources, resourceLimits;

	public Material notAllowedMaterial, allowedMaterial;		// Material for the building placement
 
 	// Variables for the building
	public Building tempBuilding;
	private Unit tempCreator;
	private bool findingPlacement = false;


	// Method to initialize the player
	public static GameObject InstantiatePlayer(Vector3 playerPosition) 
	{
		int playerID = PhotonNetwork.player.ID;

		GameObject playerObject = PhotonNetwork.Instantiate("Player", playerPosition, Quaternion.identity, 0); 
		Player myPlayer = playerObject.GetComponent<Player>();
		myPlayer.name = "MyNetworkPlayer" + PhotonNetwork.player.ID; // TODO must come from a GUI
		myPlayer.username = "MyNetworkPlayer" + PhotonNetwork.player.ID;	// TODO when checking if the owner is the same, must either check the userName, or the name. Not both


		// Update name of children to be specific to this player and unique (so that it can be retrieve with a Find)
		playerObject.GetComponentInChildren<Units>().name += playerID;
		playerObject.GetComponentInChildren<Buildings>().name += playerID;
		playerObject.GetComponentInChildren<RallyPoint>().name += playerID;


		// Enable the scripts related to the player
	 	myPlayer.transform.GetComponent<UserInput>().enabled = true;
	 	myPlayer.transform.GetComponent<GUIManager>().enabled = true;

	 	return playerObject;
	}

	/*** ------------------------------------------------------ ***/
	/*** Game Engine methods, all can be overridden by subclass ***/
	/*** ------------------------------------------------------ ***/

	void Start () {
		hud = GetComponentInChildren< HUD >();
		
		// Init resources
		AddStartResourceLimits();
		AddStartResources();
	}
	
	void Awake() {
    	resources = InitResourceList();
    	resourceLimits = InitResourceList();
    	ResourceManager.AddPlayer(this);
	}
	
	void Update () {
		//bool result = CanCreate(ResourceManager.GetBuilding("Refinery").GetComponent<WorldObject>());
		//Debug.Log("CAN CREATE REFINERY ???? : " + result);

		if ( human ) {

    		// If the player is in the process of placing a building, check the validity of the placement
    		if (findingPlacement) {
    			tempBuilding.CalculateBounds();
    			if ( CanPlaceBuilding() ) {
    				tempBuilding.SetTransparentMaterial(allowedMaterial, false);
    			} else {
    				tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
    			}
			}

			//foreach (WorldObject obj in selections) {
			//	obj.currentlySelected = true;
			//}
		}
	}


	/*** ------------------------------------------------------ ***/

	public void AddUnitInList(Unit unit) {
    	unitsList.Add(unit);
    }

    public void RemoveUnitInList(Unit unit) {
    	unitsList.Remove(unit);
    }

    public List<Unit> GetUnitList() {
    	return unitsList;
    }

    public void AddBuildingInList(Building building) {
    	buildingsList.Add(building);
    }

    public void RemoveBuildingInList(Building building) {
    	buildingsList.Remove(building);
    }

    public List<Building> GetBuildingList() {
    	return buildingsList;
    }

    // Remove the object from all list of the players.
    public void RemoveFromList(WorldObject obj) {
    	if ( obj is Unit ) {
    		RemoveUnitInList((Unit)obj);
    	}
    	if ( obj is Building ) {
    		RemoveBuildingInList((Building)obj);
    	}
    }


	// Initialize the resource list of the player
	private Dictionary< ResourceType, int > InitResourceList() {
    	Dictionary< ResourceType, int > list = new Dictionary< ResourceType, int >();
    	list.Add(ResourceType.Money, 0);
    	list.Add(ResourceType.Power, 0);
    	return list;
	}

	private void AddStartResourceLimits() {
		resourceLimits[ResourceType.Money] += startMoneyLimit;
		resourceLimits[ResourceType.Power] += startPowerLimit;
	}
 
	private void AddStartResources() {
		resources[ResourceType.Money] += startMoney;
		resources[ResourceType.Power] += startPower;
	}

	// Check if the player is dead (i.e. not having a single unit or building left)
	public bool IsDead() {
	    Building[] buildings = GetComponentsInChildren< Building >();
	    Unit[] units = GetComponentsInChildren< Unit >();
	    if(buildings != null && buildings.Length > 0) return false;
	    if(units != null && units.Length > 0) return false;
	    return true;
	}

	public Team GetTeam() { return team; }
	public void SetTeam(Team team) { this.team = team; }
	
	public GenericRace GetRace() { return race; }

	/*** -------------------------------------- ***/
	/*** 			ADDING A UNIT   			***/
	/*** -------------------------------------- ***/

	// Check if the player can remove the resources resourcesToSubstract
	public bool CanRemoveResources( Dictionary<ResourceType, int> resourcesToSubstract ) 
	{
		// For each resource to substract, check that the player has enough resource to do it.
		foreach( KeyValuePair<ResourceType, int> resource in resourcesToSubstract ) {
			if ( resources[resource.Key] < resource.Value ) return false;
		}	
		// If the function manages to go here, it means that the player had enough resources
		return true;	
	}

	// Remove the resources resourcesToSubstract from the player resources
	public void RemoveResources( Dictionary<ResourceType, int> resourcesToSubstract ) 
	{	
		// Substract each resources
		Debug.Log("REMOVE RESOURCE");
		Debug.Log("Resource to substract : " + resourcesToSubstract.Count);
		foreach( KeyValuePair<ResourceType, int> resource in resourcesToSubstract ) {
			resources[resource.Key] -= resource.Value;
			Debug.Log(" Resource " + resource.Key + " : " + resources[resource.Key]);
		}
	}

	public void AddResource(ResourceType type, int amount) {
		resources[type] += amount;
	}

	public int GetResourceAmount(ResourceType type) {
	    return resources[type];
	}


	// Method to add a new unit to the player.
	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator) 
	{
    	Debug.Log ("add " + unitName + " to player");

		GameObject newUnit = Unit.InstantiateUnit(this.gameObject, unitName, spawnPoint, Quaternion.identity);
		
		// parent of unit = Units (list of all units of the player)
		Units units = GetComponentInChildren< Units >();
		newUnit.transform.parent = units.transform;

		Unit unitObject = newUnit.GetComponent< Unit >();

		// Initialize the unit and make it move to the rally point
    	if ( unitObject && spawnPoint != rallyPoint ) {
    		unitObject.SetBuilding(creator);
    		unitObject.StartMove(rallyPoint);
    	}

	}

	/*** -------------------------------------- ***/
	/*** 			SELECTION METHODS 			***/
    /*** -------------------------------------- ***/

	// Method to create a box selection, and add the units inside the box to the player selection
	float epsilon = 2.0f;
    public void BoxSelection(Vector3 from, Vector3 to)
    {

        ResetSelection();

        for ( int i = 0 ; i < unitsList.Count ; ++i ) {
        	Unit u = unitsList[i];
            
            if (((u.GetPosition().x + epsilon > from.x && u.GetPosition().x - epsilon < to.x) || (u.GetPosition().x - epsilon < from.x && u.GetPosition().x + epsilon> to.x)) &&
               ((u.GetPosition().z + epsilon > from.z && u.GetPosition().z - epsilon < to.z) || (u.GetPosition().z - epsilon < from.z && u.GetPosition().z + epsilon> to.z)))
   			{
                AddSelection(u);   
            }
        }

    }

    public void AddSelection(WorldObject u)
    {
        u.SetSelection(true, this.hud.GetPlayingArea()); 
        selections.Add(u);
    }

    public void ResetSelection()
    {
        foreach (WorldObject obj in selections) {
            obj.SetSelection(false);
        }
        selections.Clear();
    }

    public List<WorldObject> GetSelection() { return selections; }

    public void RemoveFromSelection(WorldObject obj) {
    	selections.Remove(obj);
    }




	/*** -------------------------------------- ***/
	/*** 			BUILDINGS METHODS 			***/
    /*** -------------------------------------- ***/

	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
		
		// Destroy the ghost remnant (if the player does not rightclick, but directly click on a new Building, then, a ghost would remain without that fix)
		if (tempBuilding) {
			Destroy(tempBuilding.gameObject);
		}

		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
    	
    	// Create a ghost building to show the player where the placement will be    	
    	tempBuilding = newBuilding.GetComponent< Building >();
    	if (tempBuilding) {
        	tempCreator = creator;
        	findingPlacement = true;
        	tempBuilding.hitPoints = 0;
        	tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
        	tempBuilding.SetColliders(false);
        	tempBuilding.SetPlayingArea(playingArea);
   	 	} else {
   	 		WorldObject.RPCDestroy(newBuilding);
   	 	}
	}

	public bool IsFindingBuildingLocation() {
    	return findingPlacement;
	}
 
	public void FindBuildingLocation() {
    	Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
    	tempBuilding.transform.position = newLocation;
	}

	// Check if the building can be placed at the mouse location of the player
	// TODO move in another class (Building) ? 
	public bool CanPlaceBuilding() {
    	bool canPlace = true;
 
    	Bounds placeBounds = tempBuilding.GetSelectionBounds();
    	// shorthand for the coordinates of the center of the selection bounds
    	float cx = placeBounds.center.x;
    	float cy = placeBounds.center.y;
    	float cz = placeBounds.center.z;
    	// shorthand for the coordinates of the extents of the selection box
    	float ex = placeBounds.extents.x;
    	float ey = placeBounds.extents.y;
    	float ez = placeBounds.extents.z;
 
	    // Determine the screen coordinates for the corners of the selection bounds
	    List< Vector3 > corners = new List< Vector3 >();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz-ez)));
	 
	 	// For each corner, find the first object hit (from the corner, not the mouse !). If not ground, and an existing world object -> invalid position
	    foreach ( Vector3 corner in corners ) {
	        GameObject hitObject = WorkManager.FindHitObject(corner);
	        if ( hitObject && hitObject.name != "Ground" && hitObject.name != "Bridge" ) {
	        	//Debug.Log("Hit object : " + hitObject + " / " + hitObject.name);
	            WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
	            if (worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) {
	            	canPlace = false;
	            }
	        } else {
	        	// TODO compute the derivate of the vector joining the corners, and make sure that this is not too sharp to build.
	        }
	    }
	    return canPlace;
	}


	public void StartConstruction() 
	{
	    // Stop the placement mode
	    findingPlacement = false;

	    // Create the real building, through the network so that other players can see it too
	    Debug.Log("Name : " + tempBuilding.name);
	    GameObject newBuilding = Building.InstantiateBuilding(this.gameObject, tempBuilding.objectName, tempBuilding.transform.position, tempBuilding.transform.rotation);

		// Destroy the temp ghost building
		Destroy(tempBuilding.gameObject);
	    
		tempBuilding = newBuilding.GetComponent< Building >();

	    // Place the building
	    Buildings buildings = GetComponentInChildren< Buildings >();
	    if (buildings) {
	    	tempBuilding.transform.parent = buildings.transform;
	    }

	    // Initialize the building and start the construction
	    tempBuilding.SetPlayer();
	    tempBuilding.SetColliders(true);
	    tempCreator.SetBuilding(tempBuilding);
	    tempBuilding.StartConstruction();
	    tempBuilding.hitPoints = 0;
	    
	    // Reset it to null so that the building is not deleted by the CreateBuilding method
	    tempBuilding = null;
	}


	// Cancel the building placement. Triggered by right-clicking.
	public void CancelBuildingPlacement() 
	{
	    findingPlacement = false;
	    WorldObject.RPCDestroy(tempBuilding.gameObject);
	    tempBuilding = null;
	    tempCreator = null;
	}


	// Check that the player can create a specific object, by checking that he already possess every requirements needed
	public bool CanCreate(WorldObject obj) 
	{
		List<Type> requirements = obj.GetBuildingRequirements();

		// Check that for each type requirement, there is an object in the children of the player
		foreach (Type requirement in requirements) {
			Component objInChildren = this.GetComponentInChildren( requirement );

			// If no object of type requirement was found, then the player cannot create the object yet
			if (objInChildren == null) {
				return false;
			}
		}
		return true;
	}
}
