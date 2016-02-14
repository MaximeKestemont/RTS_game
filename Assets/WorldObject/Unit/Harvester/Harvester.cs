using UnityEngine;
using RTS;
using System.Collections.Generic;
 
public class Harvester : Unit {
 
    public float capacity;
    public Building resourceStore;
    public float collectionAmount, depositAmount;
 

	private float currentDeposit = 0.0f;
    private bool harvesting = false, emptying = false;
    private float currentLoad = 0.0f;
    private ResourceType harvestType;
    private Resource resourceDeposit;		

    // Audio related variables
    public AudioClip emptyHarvestSound, harvestSound, startHarvestSound;
    public float emptyHarvestVolume = 0.5f, harvestVolume = 0.5f, startHarvestVolume = 1.0f;		
 
    /*** Game Engine methods, all can be overridden by subclass ***/
 
    protected override void Start () {
        base.Start();
        harvestType = ResourceType.Unknown;
    }
 
    protected override void Update () {
    	base.Update();
    	
    	if(!rotating && !moving) {
        	if (harvesting || emptying) {
            	Arms[] arms = GetComponentsInChildren< Arms >();
            	foreach(Arms arm in arms) {
            		arm.GetComponent<Renderer>().enabled = true;
            	}
            	if ( harvesting ) {
                	Collect();
                	if ( currentLoad >= capacity || resourceDeposit.isEmpty() ) {
                    	//make sure that we have a whole number to avoid bugs
                    	//caused by floating point numbers
                    	currentLoad = Mathf.Floor(currentLoad);
                    	harvesting = false;
                    	emptying = true;
                    	foreach ( Arms arm in arms ) { 
                    		arm.GetComponent<Renderer>().enabled = false;
                    	}
                    	StartMove (resourceStore.transform.position, resourceStore.gameObject);
                	}
            	} else {
                	Deposit();
                	if ( currentLoad <= 0 ) {
                    	emptying = false;
                    	foreach(Arms arm in arms) {
                    		arm.GetComponent<Renderer>().enabled = false;
                    	}
                    	if (!resourceDeposit.isEmpty() ) {
                        	harvesting = true;
                        	StartMove (resourceDeposit.transform.position, resourceDeposit.gameObject);
                    	} else {
                            // TODO no need to check each player as only the player with the view matter + seems to work fine without that part...
                            /*foreach ( Player player in ResourceManager.GetPlayers() ) {
                                player.RemoveFromSelection(resourceDeposit);
                            }*/
                            Destroy(resourceDeposit);
                        }
                	}
            	}
        	}
    	}
	}

    // Link the harverster to the building having created it (the refinery)
    public override void SetBuilding (Building creator) {
        base.SetBuilding (creator);
        resourceStore = creator;
    }

	// Draw the current amount of resources collected by the harverster
	protected override void DrawSelectionBox (Rect selectBox) {
    	base.DrawSelectionBox(selectBox);
    	float percentFull = currentLoad / capacity;
    	float maxHeight = selectBox.height - 4;
    	float height = maxHeight * percentFull;
    	float leftPos = selectBox.x + selectBox.width - 7;
    	float topPos = selectBox.y + 2 + (maxHeight - height);
    	float width = 5;
    	Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
    	if(resourceBar) {
    		GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
    	}
	}

    protected override void InitialiseAudio () {
        base.InitialiseAudio ();
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        if(emptyHarvestVolume < 0.0f) emptyHarvestVolume = 0.0f;
        if(emptyHarvestVolume > 1.0f) emptyHarvestVolume = 1.0f;
        sounds.Add(emptyHarvestSound);
        volumes.Add(emptyHarvestVolume);
        if(harvestVolume < 0.0f) harvestVolume = 0.0f;
        if(harvestVolume > 1.0f) harvestVolume = 1.0f;
        sounds.Add(harvestSound);
        volumes.Add (harvestVolume);
        if(startHarvestVolume < 0.0f) startHarvestVolume = 0.0f;
        if(startHarvestVolume > 1.0f) startHarvestVolume = 1.0f;
        sounds.Add(startHarvestSound);
        volumes.Add(startHarvestVolume);
        audioElement.Add(sounds, volumes);
    }

 
    /* Public Methods */
 
    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);

        //only handle input if owned by a human player and currently selected
        if(player && player.human && currentlySelected) {
            if(hoverObject.name != "Ground" && hoverObject.name != "Bridge") {
                Resource resource = hoverObject.transform.parent.GetComponent< Resource >();
                if (resource && !resource.isEmpty()) { 
                	player.hud.SetCursorState(CursorState.Harvest);
                }
            }
        }
    }
 
    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle input if owned by a human player
        if(player && player.human) {
            if(hitObject.name != "Ground" && hitObject.name != "Bridge") {
                Resource resource = hitObject.transform.parent.GetComponent< Resource >();
                if(resource && !resource.isEmpty()) {
                    //make sure that we select harvester remains selected
                    if(player.selections.Count > 0) { 
                    	//player.ResetSelection();
                        //player.SelectedObject.SetSelection(false, playingArea);
                    }
                    //SetSelection(true, playingArea);
                    //player.selections.Add(this);
                    StartHarvest(resource);
                }
            } else StopHarvest();
        }
    }
 
    /* Private Methods */
 
    private void StartHarvest(Resource resource) {
        if(audioElement != null) audioElement.Play(startHarvestSound);

    	resourceDeposit = resource;
    	StartMove(resource.transform.position, resource.gameObject);
    	
    	//we can only collect one resource at a time, other resources are lost
    	if(harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType()) {
        	harvestType = resource.GetResourceType();
        	currentLoad = 0.0f;
    	}
    	harvesting = true;
    	emptying = false;
	}
 
    private void StopHarvest() {
 
    }

    private void Collect() {
        if(audioElement != null) audioElement.Play(harvestSound);

    	float collect = collectionAmount * Time.deltaTime;
    	//make sure that the harvester cannot collect more than it can carry
    	if (currentLoad + collect > capacity) {
    		collect = capacity - currentLoad;
    	}
    	resourceDeposit.Remove(collect);
    	currentLoad += collect;
	}
 
	private void Deposit() {
        if(audioElement != null) audioElement.Play(emptyHarvestSound);

    	currentDeposit += depositAmount * Time.deltaTime;
    	int deposit = Mathf.FloorToInt(currentDeposit);
    	if(deposit >= 1) {
        	if(deposit > currentLoad) {
        		deposit = Mathf.FloorToInt(currentLoad);
        	}
        	currentDeposit -= deposit;
        	currentLoad -= deposit;
        	ResourceType depositType = harvestType;
        	if(harvestType == ResourceType.Ore) {
        		depositType = ResourceType.Money;
        	}
        	player.AddResource(depositType, deposit);
    	}
	}

}