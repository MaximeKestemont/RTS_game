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

	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation) {
    	Debug.Log ("add " + unitName + " to player");

		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		
		// parent of unit = Units (list of all units of the player)
		Units units = GetComponentInChildren< Units >();
		newUnit.transform.parent = units.transform;

		Unit unitObject = newUnit.GetComponent< Unit >();
    	if(unitObject && spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
	}
}
