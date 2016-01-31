using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour {

	public HUD hud;
	public string username;
	public bool human;
	public Color teamColor;

	private List<Unit> unitsList = new List<Unit>();
	public List<WorldObject> selections;

	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	private Dictionary< ResourceType, int > resources, resourceLimits;

	public Material notAllowedMaterial, allowedMaterial;		// Material for the building placement
 
 	// Variables for the building
	private Building tempBuilding;
	private Unit tempCreator;
	private bool findingPlacement = false;

	// Use this for initialization
	void Start () {
		hud = GetComponentInChildren< HUD >();
		
		// Init resources
		AddStartResourceLimits();
		AddStartResources();
	}
	
	void Awake() {
    	resources = InitResourceList();
    	resourceLimits = InitResourceList();
	}
	
	// Update is called once per frame
	void Update () {
		if ( human ) {
    		hud.SetResourceValues(resources, resourceLimits);

    		// If the player is in the process of placing a building, check the validity of the placement
    		if (findingPlacement) {
    			tempBuilding.CalculateBounds();
    			if ( CanPlaceBuilding() ) {
    				tempBuilding.SetTransparentMaterial(allowedMaterial, false);
    			} else {
    				tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
    			}
			}
		}
	}

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

	public void AddResource(ResourceType type, int amount) {
		resources[type] += amount;
	}

	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator) {
    	Debug.Log ("add " + unitName + " to player");

		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		
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
    

    public void BoxSelection(Vector3 from, Vector3 to)
    {
        ResetSelection();
        
        Debug.Log("UNITS : " + unitsList.Count);
        for ( int i = 0 ; i < unitsList.Count ; ++i ) {
        	Unit u = unitsList[i];
            
            if (((u.GetPosition().x > from.x && u.GetPosition().x < to.x) || (u.GetPosition().x < from.x && u.GetPosition().x > to.x)) &&
               ((u.GetPosition().z > from.z && u.GetPosition().z < to.z) || (u.GetPosition().z < from.z && u.GetPosition().z > to.z)))
            {
                AddSelection(u);   
            }

        }
        Debug.Log("Selection : " + selections.Count);
    }

    public void AddSelection(Unit u)
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


    public void AddUnitInList(Unit unit) {
    	unitsList.Add(unit);
    }




	// *** BUILDING METHODS *** //


	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
		
		// Destroy the ghost remnant (if the player does not rightclick, but directly click on a new Building, then, a ghost would remain without that fix)
		if (tempBuilding) {
			Destroy(tempBuilding.gameObject);
		}

    	GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
    	tempBuilding = newBuilding.GetComponent< Building >();
    	// Create a ghost building to show the player where the placement will be
    	if (tempBuilding) {
        	tempCreator = creator;
        	findingPlacement = true;
        	tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
        	tempBuilding.SetColliders(false);
        	tempBuilding.SetPlayingArea(playingArea);
   	 	} else {
   	 		Destroy(newBuilding);
   	 	}
	}

	public bool IsFindingBuildingLocation() {
    	return findingPlacement;
	}
 
	public void FindBuildingLocation() {
    	Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
    	//newLocation.y = 0;
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
	        if ( hitObject && hitObject.name != "Ground" ) {
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

	public void StartConstruction() {
	    // Stop the placement mode
	    findingPlacement = false;
	    
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
	    
	    // Reset it to null so that the building is not deleted by the CreateBuilding method
	    tempBuilding = null;
	}

	// Cancel the building placement. Triggered by right-clicking.
	public void CancelBuildingPlacement() {
	    findingPlacement = false;
	    Destroy(tempBuilding.gameObject);
	    tempBuilding = null;
	    tempCreator = null;
	}
}
