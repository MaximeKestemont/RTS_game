using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;
using Photon;

public class WorldObject : Photon.MonoBehaviour {

	public string objectName;	// TODO currently not unique - may cause a problem later on
	public Texture2D buildImage;
	public int cost, sellValue;
	public int hitPoints, maxHitPoints;
	public float weaponRange = 10.0f;	// default range
	public float weaponRechargeTime = 1.0f;	// attack speed
	public float weaponAimSpeed = 1.0f;		// speed to aim
	private float currentWeaponChargeTime; // current progress of reloading

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	
	protected GUIStyle healthStyle = new GUIStyle(); // hold the healthy/damaged/critical texture
	protected float healthPercentage = 1.0f;


	// Combat related variables
	protected WorldObject target = null; 			// attack target
	protected bool attacking = false; 				// attack mode or not
	protected bool movingIntoPosition = false;		// moving into position to be able to attack
	protected bool aiming = false;					// aiming (once in position)

	private List< Material > oldMaterials = new List< Material >(); // Store the list of materials, to restore them later on

	// Audio related variables
	public AudioClip attackSound, selectSound, useWeaponSound;
	public float attackVolume = 1.0f, selectVolume = 1.0f, useWeaponVolume = 1.0f;
	protected AudioElement audioElement;

	// Network related variables
	public PhotonView myPhotonView;


	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();

		// Get back  the network view, to be able to call RPC method on it
		myPhotonView = this.GetComponent<PhotonView>();
	}
 
	protected virtual void Start () {

		// Initialize the player to which the object belong - if there is one
    	SetPlayer();

    	// Initialize the color of the object depending on the player owning it
    	if ( player ) {
    		SetTeamColor();
    	}

    	// Initialise audio settings for the object
    	InitialiseAudio();

	}
 
 	// TODO maybe the attack part should be moved to a subclass of world object being able to attack, as the non attacking building here will still
 	// have to handle those checks...
	protected virtual void Update () {
		// Progress towards the reloading of weapon
	    currentWeaponChargeTime += Time.deltaTime;

	    // If in attack mode, not moving into position or aiming -> attack
	    if (attacking && !movingIntoPosition && !aiming) {
	    	PerformAttack();
	    }
	}
 
	protected virtual void OnGUI() {
		if (currentlySelected) DrawSelection();
	}

	
    protected virtual void AnimationUpdate() {
        // to specifiy in children
    }

	public virtual void SetSelection(bool selected, Rect playingArea) {
    	currentlySelected = selected;
    	if (selected) {
    		if (audioElement != null) 
    			audioElement.Play(selectSound);
    		this.playingArea = playingArea;
    	}
	}

	public virtual void SetSelection(bool selected) {
		currentlySelected = selected;
	}

	public bool IsOwnedBy(Player owner) {
    	if(player && player.Equals(owner)) {
        	return true;
    	} else {
        	return false;
    	}
	}

	public string[] GetActions() {
    	return actions;
	}
 
	public virtual void PerformAction(string actionToPerform) {
		Debug.Log("Perform action Building");
    	//it is up to children with specific actions to determine what to do with each of those actions
	}

	public virtual void SetHoverState(GameObject hoverObject) {
	    //only handle input if owned by a human player and currently selected
	    if(player && player.human && currentlySelected) {
	        //something other than the ground is being hovered over
	        //Debug.Log("Object name : " + hoverObject.name);
	        if(hoverObject.name != "Ground" && hoverObject.name != "Bridge" ) {
	            Player owner = hoverObject.transform.root.GetComponent< Player >();
	            Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
	            Building building = hoverObject.transform.parent.GetComponent< Building >();
	            
				// the object is owned by a player
	            if ( owner ) {
	            	// player = current player  
	                if ( owner.username == player.username ) {
	                	player.hud.SetCursorState(CursorState.Select);
	                }
	                // other player
	                else if(CanAttack()) {
	                	player.hud.SetCursorState(CursorState.Attack);
	                }
	                else {
	                	player.hud.SetCursorState(CursorState.Select);
	                }
	            } else if (unit || building && CanAttack()) {
	            	player.hud.SetCursorState(CursorState.Attack);
	            } else {
	            	player.hud.SetCursorState(CursorState.Select);
	            }
	        }
	    }
	}


	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
	    bool eraseAttackMode = true;

	    //only handle input if currently selected
	    if (currentlySelected && hitObject && hitObject.name != "Ground" && hitObject.name != "Bridge") {
	        WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
	        
	        //clicked on another selectable object
	        if (worldObject) {
	            Resource resource = hitObject.transform.parent.GetComponent< Resource >();
	            // If the click is on a resource deposit and resource deposit is empty, do nothing
	            if(resource && resource.isEmpty()) {
	            	return;
	            } else {
		            Player owner = hitObject.transform.root.GetComponent< Player >();
		            //the object is controlled by a player
		            if (owner) { 
		            	// this object is controlled by a human player
		                if (player && player.human) { 
		                    //start attack if object is not owned by the same player and this object can attack
		                    Debug.Log("Player : " + player.username + "/ : " + player.name);
		                    Debug.Log("Target : " + owner.username);
		                    if ( (player.username != owner.username || player.name != owner.username) && CanAttack()) {
		                    	eraseAttackMode = false;
		                    	BeginAttack(worldObject);
		                    } 
		                } 
		            } 
		        }
	    	}
	    }

	    if (eraseAttackMode) {
	    	attacking = false;
	    	movingIntoPosition = false;
	    	aiming = false;
	    }
	}

	// Method that initiate an attack (i.e. launch the attach if is range, adjust position if not)
	protected virtual void BeginAttack(WorldObject target) {
		if (audioElement != null) 
			audioElement.Play(attackSound);
	    this.target = target;
	    if ( TargetInRange() ) {
	        attacking = true;
	        PerformAttack();
	    } else {
	    	AdjustPosition();
	    }
	}

	// Calculate the bounds of the object
	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren< Renderer >()) {
			selectionBounds.Encapsulate(r.bounds);
		}
	}


	//private void ChangeSelection(WorldObject worldObject, Player controller) {
    	//this should be called by the following line, but there is an outside chance it will not
    	/*SetSelection(false, playingArea);
    	foreach (WorldObject obj in controller.selections ) 
    		obj.SetSelection(false, playingArea);
    	controller.ResetSelection();
    	controller.selections.Add(worldObject);
   	 	worldObject.SetSelection(true, controller.hud.GetPlayingArea());*/
	//}

	protected virtual void DrawSelectionBox(Rect selectBox) {
	    GUI.Box(selectBox, "");
	    CalculateCurrentHealth(0.35f, 0.65f);
	    DrawHealthBar(selectBox, "");
	}

	protected void DrawHealthBar(Rect selectBox, string label) {
	    healthStyle.padding.top = -20;	// make sure that the text is drawn above the health bar
	    healthStyle.fontStyle = FontStyle.Bold;
	    GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
	}

	// This method finds all the child objects of this object with the script TeamColor.cs attached, and changed their color to 
	// the team color of the player.
	protected void SetTeamColor() {
    	TeamColor[] teamColors = GetComponentsInChildren< TeamColor >();
    	foreach(TeamColor teamColor in teamColors) {
    		teamColor.GetComponent<Renderer>().material.color = player.teamColor;
    	}
	}
	 

	// This will be called by the object when selected, so that it appears on the UI.
	private void DrawSelection() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the playing area
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	// Set the heathstyle background to healthy/damaged/critical, depending on the current health of the object
	//[PunRPC]
	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit) {
	    healthPercentage = (float)hitPoints / (float)maxHitPoints;
	    if ( healthPercentage > highSplit ) {
	    	healthStyle.normal.background = ResourceManager.HealthyTexture;
	    } else if ( healthPercentage > lowSplit ) {
	    	healthStyle.normal.background = ResourceManager.DamagedTexture;
	    } else { 
	    	healthStyle.normal.background = ResourceManager.CriticalTexture;
	    }
	}

	public void SetColliders(bool enabled) {
    	Collider[] colliders = GetComponentsInChildren< Collider >();
    	foreach (Collider collider in colliders) {
    		collider.enabled = enabled;
    	}
	}
 
	public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
    	if (storeExistingMaterial) {
    		oldMaterials.Clear();
    	}
    	Renderer[] renderers = GetComponentsInChildren< Renderer >();
    	foreach(Renderer renderer in renderers) {
        	if (storeExistingMaterial) {
        		oldMaterials.Add(renderer.material);
        	}
        	renderer.material = material;
    	}
	}
 
	public void RestoreMaterials() {
    	Renderer[] renderers = GetComponentsInChildren< Renderer >();
    	if (oldMaterials.Count == renderers.Length) {
        	for (int i = 0; i < renderers.Length; i++) {
            	renderers[i].material = oldMaterials[i];
        	}
    	}
	}
 
	public void SetPlayingArea(Rect playingArea) {
    	this.playingArea = playingArea;
	}

	public Bounds GetSelectionBounds() {
    	return selectionBounds;
	}

	// Set the player that owns the object
	public void SetPlayer() {
    	player = transform.root.GetComponentInChildren< Player >();
	}


	// *** Attack methods *** 

	public virtual bool CanAttack() {
    	//default behaviour needs to be overidden by children
    	return false;
	}

	private bool TargetInRange() {
	    Vector3 targetLocation = target.transform.position;
	    Vector3 direction = targetLocation - transform.position;
	    // distance = root of square of direction -> distance^2 so that we dont do the root computation (faster this way)
	    if (direction.sqrMagnitude < weaponRange * weaponRange) {
	        return true;
	    }
	    return false;
	}

	private void AdjustPosition() {
	    Unit self = this as Unit;
	    
	    // if unit, adjust. If not an unit, cant attack (currently).
	    if (self) {
	        movingIntoPosition = true;
	        Vector3 attackPosition = FindNearestAttackPosition();
	        self.StartMove(attackPosition);
	        attacking = true;
	    } else {
	    	attacking = false;
	    }
	}

	private Vector3 FindNearestAttackPosition() {
	    Vector3 targetLocation = target.transform.position;
	    Vector3 direction = targetLocation - transform.position;
	    float targetDistance = direction.magnitude;

	    // Note : taking 90% of the range, so that if the target directly move, the unit should not directly re-move.
	    float distanceToTravel = targetDistance - (0.9f * weaponRange);	

	    // Linear interpolation to find the position that is at the correct distance, along the direction vector.
	    return Vector3.Lerp(transform.position, targetLocation, distanceToTravel / targetDistance);
	}

	private void PerformAttack() {
		// Make sure that the target is not already destroyed
	    if (!target) {
	        attacking = false;
	        return;
	    }
	    // Check if still in range (as it could have managed to run away)
	    if (!TargetInRange()) {
	    	AdjustPosition();
	    // Check if the weapon is in front
	    } else if (!TargetInFrontOfWeapon()) {
	    	AimAtTarget();
	    // Check that it can fire (or is reloading)
	    } else if (ReadyToFire()) {
	    	UseWeapon();
	    }
	}

	// Check that the object is in the right direction to attack. If an unit can fire from backward, this need to be overriden !
	protected virtual bool TargetInFrontOfWeapon() {
	    Vector3 targetLocation = target.transform.position;
	    Vector3 direction = targetLocation - transform.position;
	    if (direction.normalized == transform.forward.normalized) {
	    	return true; 
	    } else {
	    	return false;
	    }
	}

	// Differ for each kind of unit (animation, time to set up, etc.)
	protected virtual void AimAtTarget() {
    	aiming = true;
    	//this behaviour needs to be specified by a specific object
	}

	// Method handling the attack speed of the weapong
	private bool ReadyToFire() {
	    if (currentWeaponChargeTime >= weaponRechargeTime) {
	    	return true;
	    } else {	
	    	return false;
		}
	}

	// Method handling the firing part of the attack (and resetting the attack load time)
	protected virtual void UseWeapon() {

		if(audioElement != null) 
			audioElement.Play(useWeaponSound);

		// Reset the loading time of the weapon, for the next attack
	    currentWeaponChargeTime = 0.0f;
	    //this behaviour needs to be specified by a specific object
	}

	[PunRPC]
	public void TakeDamage(int damage) {
	    hitPoints -= damage;
	    if ( hitPoints <= 0 ) {
	    	// TODO animation for the object being destroyed

	    	// Remove object from the selections of every player
	    	foreach ( Player player in ResourceManager.GetPlayers() ) {
	    		player.RemoveFromSelection(this);
	    	}
	    	Destroy(gameObject);
	    }
	}

	// Method to destroy an object on every Player 	// TODO move to another place
	[PunRPC]
	public static void RPCDestroy(GameObject obj) {
		Destroy(obj);
	}


	// Initialise the audio settings by creating the audio element containing the audio objects (containing the audio clip).
	protected virtual void InitialiseAudio() {
	    List< AudioClip > sounds = new List< AudioClip >();
	    List< float > volumes = new List< float >();
	    
	    // TODO refactor those 3 calls (need to check how to use pointers inside safe context)
	    if (attackVolume < 0.0f) attackVolume = 0.0f;
	    if (attackVolume > 1.0f) attackVolume = 1.0f;
	    sounds.Add(attackSound);
	    volumes.Add(attackVolume);
	    if (selectVolume < 0.0f) selectVolume = 0.0f;
	    if (selectVolume > 1.0f) selectVolume = 1.0f;
	    sounds.Add(selectSound);
	    volumes.Add(selectVolume);
	    if (useWeaponVolume < 0.0f) useWeaponVolume = 0.0f;
	    if (useWeaponVolume > 1.0f) useWeaponVolume = 1.0f;
	   	sounds.Add(useWeaponSound);
	    volumes.Add(useWeaponVolume);
	    audioElement = new AudioElement(sounds, volumes, objectName, this.transform);
	}




}
