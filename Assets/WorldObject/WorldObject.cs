﻿using UnityEngine;
using System.Collections;

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


	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}
 
	protected virtual void Start () {
    	player = transform.root.GetComponentInChildren< Player >();
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
    	//it is up to children with specific actions to determine what to do with each of those actions
	}

	public virtual void SetHoverState(GameObject hoverObject) {
    	//only handle input if owned by a human player and currently selected
    	if(player && player.human && currentlySelected) {
        	if(hoverObject.name != "Ground") player.hud.SetCursorState(CursorState.Select);
    	}
	}

	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
    	//only handle input if currently selected
    	if(currentlySelected && hitObject && hitObject.name != "Ground") {
        	WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
        	//clicked on another selectable object
        	if(worldObject) ChangeSelection(worldObject, controller);
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
    	CalculateCurrentHealth();
    	GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), "", healthStyle);
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

	protected virtual void CalculateCurrentHealth() {
    	healthPercentage = (float)hitPoints / (float)maxHitPoints;
    	if(healthPercentage > 0.65f) {
    		healthStyle.normal.background = ResourceManager.HealthyTexture;
    	} else if (healthPercentage > 0.35f) { 
    		healthStyle.normal.background = ResourceManager.DamagedTexture;
    	} else { 
    		healthStyle.normal.background = ResourceManager.CriticalTexture;
    	}
	}

	public Bounds GetSelectionBounds() {
    	return selectionBounds;
	}

}
