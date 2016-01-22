using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class WorldObject : MonoBehaviour {

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue;
	public int hitPoints, maxHitPoints;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	
	protected GUIStyle healthStyle = new GUIStyle(); // hold the healthy/damaged/critical texture
	protected float healthPercentage = 1.0f;

	private List< Material > oldMaterials = new List< Material >(); // Store the list of materials, to restore them later on


	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}
 
	protected virtual void Start () {
    	SetPlayer();
	}
 
	protected virtual void Update () {
	 
	}
 
	protected virtual void OnGUI() {
		if (currentlySelected) DrawSelection();
	}

	public virtual void SetSelection(bool selected, Rect playingArea) {
    	currentlySelected = selected;
    	if(selected) this.playingArea = playingArea;
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
        	if(hoverObject.name != "Ground") player.hud.SetCursorState(CursorState.Select);
    	}
	}

	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		Debug.Log("THERE IS A CLICK " + this);
    	//only handle input if currently selected
    	if(currentlySelected && hitObject && hitObject.name != "Ground") {
        	WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
        	//clicked on another selectable object
        	if (worldObject) {
        		Resource resource = hitObject.transform.parent.GetComponent< Resource >();
            	// If the resource deposit is empty, do nothing
            	if(resource && resource.isEmpty()) {
            		return;
            	} else {
					ChangeSelection(worldObject, controller);
            	}
        	}
    	}
	}

	// Calculate the bounds of the object
	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren< Renderer >()) {
			selectionBounds.Encapsulate(r.bounds);
		}
	}


	private void ChangeSelection(WorldObject worldObject, Player controller) {
    	//this should be called by the following line, but there is an outside chance it will not
    	SetSelection(false, playingArea);
    	if(controller.SelectedObject) controller.SelectedObject.SetSelection(false, playingArea);
    	controller.SelectedObject = worldObject;
   	 	worldObject.SetSelection(true, controller.hud.GetPlayingArea());
	}

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

}
