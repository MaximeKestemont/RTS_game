using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour {

	public HUD hud;
	public string username;
	public bool human;

	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	private Dictionary< ResourceType, int > resources, resourceLimits;

	public WorldObject SelectedObject { get; set; }
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

	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
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
    	newLocation.y = 0;
    	tempBuilding.transform.position = newLocation;
	}
}
