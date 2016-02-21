using UnityEngine;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject {

	public float maxBuildProgress;
	public Texture2D rallyPointImage;
	public Texture2D sellImage;

	protected Queue< string > buildQueue;
	protected Vector3 rallyPoint;

	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
    private bool needsBuilding = false;

    // Audio related variables
    public AudioClip finishedJobSound;
    public float finishedJobVolume = 1.0f;

    // Dirty flag to update only once the bounds, as we need to update the bounds when the object has been created by another player
    // and is received through the network (only the position is passed, not the bounds)
    private bool dirtyFlag = true;


    // Method to initialize the building, make the name corresponding to the player and sending the parent object name to the network
    // so that other players can re-create the object hierarchy
    public static GameObject InstantiateBuilding(GameObject gameObject, string prefabName, Vector3 buildingPosition, Quaternion buildingQuaternion) 
    {
        GameObject buildingObject = null;

        int playerID = PhotonNetwork.player.ID;

        if (gameObject.GetComponent<Player>()) {

            // Get the Units object
            Buildings buildings = gameObject.GetComponentInChildren< Buildings >();

            // Store the name of the parent 
            object[] data = new object[1];
            data[0] = buildings.name;           // parent name

            // Create the unit, and send the parent name so that it can be retrieved from other player views to re-build the hierarchy
            buildingObject = PhotonNetwork.Instantiate(prefabName, buildingPosition, buildingQuaternion, 0, data);
            
            // Modify the name so that it is unique (corresponding to the playerID)
            buildingObject.name = prefabName + playerID;
            buildingObject.transform.parent = buildings.transform;
        }

        if (gameObject.GetComponent<Team>()) {
            Team team = gameObject.GetComponent<Team>();

            // Store the name of the parent 
            object[] data = new object[1];
            data[0] = team.name;           // parent name

            // TODO send data?
            buildingObject = PhotonNetwork.Instantiate(prefabName, buildingPosition, buildingQuaternion, 0, data);
            team.AddBuildingInList(buildingObject.GetComponent<Building>());

            // Modify the name so that it is unique (corresponding to the team)
            buildingObject.name = prefabName + team.teamName;
            buildingObject.transform.parent = team.transform;
        }
        return buildingObject;
    }


	protected override void Awake() {
    	base.Awake();

    	buildQueue = new Queue< string >();
        SetSpawnPoint();

        CalculateBounds();
	}
 
	protected override void Start () {
    	base.Start();

        if (transform.root.GetComponent< Player >()) {
            Debug.Log("Add building to player ");
            transform.root.GetComponent< Player >().AddBuildingInList(this);
        }
	}
 
	protected override void Update () {
  	  	base.Update();

  	  	ProcessBuildQueue();


        if ( dirtyFlag ) {
            dirtyFlag = false;
            CalculateBounds();
        }

        CalculateBounds();
        
	}

    public Vector3 GetPosition() {
        return transform.position;
    }

	public override void SetSelection(bool selected, Rect playingArea) {
    	base.SetSelection(selected, playingArea);
    	if ( player ) {
        	RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
        	if ( selected ) {
            	if ( flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition ) {
                	flag.transform.localPosition = rallyPoint;
                	flag.transform.forward = transform.forward;
                	flag.Enable();
           	 	}
        	} else {
            	if ( flag && player.human ) flag.Disable();
        	}
    	}
	}

	public override void SetHoverState(GameObject hoverObject) {
    	base.SetHoverState(hoverObject);
    	
    	//only handle input if owned by a human player and currently selected
    	if (player && player.human && currentlySelected) {
        	if (hoverObject.name == "Ground" || hoverObject.name == "Bridge" ) {
            	if (player.hud.GetPreviousCursorState() == CursorState.RallyPoint) 
                    player.hud.SetCursorState(CursorState.RallyPoint);
        	}
    	}
	}


 
	protected override void OnGUI() {
 		base.OnGUI();

        // check if the building is currently being built
        if (needsBuilding) {
            DrawBuildProgress();
        }
	}


	// Put unit in the queue
	protected void CreateUnit(string unitName) {
        // If the unit is a valid unit
        if ( ResourceManager.GetUnit(unitName) && ResourceManager.GetUnit(unitName).GetComponent< Unit >() ) {
            // Check that the player has enough resources to create the unit
            if ( player.CanRemoveResources(ResourceManager.GetUnit(unitName).GetComponent< Unit >().GetCostFromArray()) ) {
                // Substract the cost of the unit, and queue it in the build queue
                player.RemoveResources(ResourceManager.GetUnit(unitName).GetComponent< Unit >().GetCostFromArray());
                buildQueue.Enqueue(unitName);
            }      
        }
	}

	// Process the queue. When the build progress is finished for a unit, add this unit to the field.
	protected void ProcessBuildQueue() {
    	if(buildQueue.Count > 0) {		
        	currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
        	if(currentBuildProgress > maxBuildProgress) {
            	if (player) {
                    if (audioElement != null) 
                        audioElement.Play(finishedJobSound);
            		player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);

            	}
            	currentBuildProgress = 0.0f;
        	}
    	}
	}

	// Return list of names of units in the build queue
	public string[] getBuildQueueValues() {
    	string[] values = new string[buildQueue.Count];
    	int pos=0;
    	foreach(string unit in buildQueue) 
    		values[pos++] = unit;
    	return values;
	}
 
	public float getBuildPercentage() {
    	return currentBuildProgress / maxBuildProgress;
	}

	public bool hasSpawnPoint() {
    	return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
    	base.MouseClick(hitObject, hitPoint, controller);

    	//only handle input if owned by a human player and currently selected
    	if ( player && player.human && currentlySelected ) {
        	if ( hitObject.name == "Ground" ) {
            	if (hitPoint != ResourceManager.InvalidPosition) {
                	SetRallyPoint(hitPoint);
            	}
        	}
    	}
	}

	public void SetRallyPoint(Vector3 position) {
    	rallyPoint = position;
    	if(player && player.human && currentlySelected) {
        	RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
        	if ( flag ) 
        		flag.transform.position = rallyPoint;
    	}
	}

	public void Sell() {
    	if ( player ) 
    		player.AddResource(ResourceType.Money, sellValue);
    	if ( currentlySelected ) 
    		SetSelection(false, playingArea);
            player.selections.Remove(this);
    	
    	Destroy(this.gameObject);
	}

    // Start the construction
    public void StartConstruction() {
        CalculateBounds();  // make sure that the bounds are correct once construction has started
        needsBuilding = true;
        hitPoints = 0;      // hitpoints allow to track the progress
    }

    private void DrawBuildProgress() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
        //Draw the selection box around the currently selected object, within the bounds of the main draw area
        GUI.BeginGroup(playingArea);
        myPhotonView.RPC("CalculateCurrentHealth", PhotonTargets.All, 0.5f, 0.99f); 
        DrawHealthBar(selectBox, "Building ...");
        GUI.EndGroup();
    }

    public bool UnderConstruction() {
        return needsBuilding;
    }
 
    // Construct the building. If finished, then restore the initial materials.    
    public void Construct(int amount) {
        hitPoints += amount;
        if(hitPoints >= maxHitPoints) {
            hitPoints = maxHitPoints;
            needsBuilding = false;
            RestoreMaterials();
            SetSpawnPoint();
            SetTeamColor();
        }
    }

    private void SetSpawnPoint() {
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 5;
        float spawnY = transform.position.y;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 5;
        spawnPoint = new Vector3(spawnX, spawnY, spawnZ);
        rallyPoint = spawnPoint;
    }

    protected override void InitialiseAudio () {
        base.InitialiseAudio ();
        if (finishedJobVolume < 0.0f) finishedJobVolume = 0.0f;
        if (finishedJobVolume > 1.0f) finishedJobVolume = 1.0f;
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        sounds.Add(finishedJobSound);
        volumes.Add (finishedJobVolume);
        audioElement.Add(sounds, volumes);
    }

}
