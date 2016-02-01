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

	protected override void Awake() {
    	base.Awake();

    	buildQueue = new Queue< string >();
        SetSpawnPoint();
	}
 
	protected override void Start () {
    	base.Start();
	}
 
	protected override void Update () {
  	  	base.Update();

  	  	ProcessBuildQueue();
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
            	if(flag && player.human) flag.Disable();
        	}
    	}
	}

	public override void SetHoverState(GameObject hoverObject) {
    	base.SetHoverState(hoverObject);
    	
    	//only handle input if owned by a human player and currently selected
    	if(player && player.human && currentlySelected) {
        	if(hoverObject.name == "Ground") {
            	if(player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
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
    	buildQueue.Enqueue(unitName);
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
    	
    	//only handle iput if owned by a human player and currently selected
    	if ( player && player.human && currentlySelected ) {
        	if(hitObject.name == "Ground") {
            	if((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition) {
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
        		flag.transform.localPosition = rallyPoint;
    	}
	}

	public void Sell() {
    	if ( player ) 
    		player.AddResource(ResourceType.Money, sellValue);
    	if ( currentlySelected ) 
    		SetSelection(false, playingArea);
    	
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
        CalculateCurrentHealth(0.5f, 0.99f);
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
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
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
