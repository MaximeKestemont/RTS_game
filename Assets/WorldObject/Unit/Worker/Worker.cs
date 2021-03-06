﻿using UnityEngine;
using System.Collections.Generic;
 
public class Worker : Unit {
 
    public int buildSpeed;
 
    private Building currentProject;
    private bool building = false;
    private float amountBuilt = 0.0f;

    // Audio related variables
    public AudioClip finishedJobSound;
    public float finishedJobVolume = 1.0f;
 
    /*** Game Engine methods, all can be overridden by subclass ***/
 
    protected override void Start () {
        base.Start();
        actions = new string[] {"Refinery", "WarFactory", "FinalCore", "Turret"};
    }
 
    protected override void Update () {
        base.Update();

        // TODO probably to consuming to call it here...
        AnimationUpdate();

        // If not moving or rotating, check if there is a building to construct, and progress in the construction
        if (!moving && !rotating) {
            if(building && currentProject && currentProject.UnderConstruction()) {
                amountBuilt += buildSpeed * Time.deltaTime;
                int amount = Mathf.FloorToInt(amountBuilt);
                if (amount > 0) {
                    amountBuilt -= amount;
                    currentProject.Construct(amount);
                    if (!currentProject.UnderConstruction()) { 
                        if (audioElement != null) audioElement.Play(finishedJobSound);
                        building = false;
                    }
                }
            }
        }
    }

    // No decision to take if in building phase
    protected override bool ShouldMakeDecision () {
        if (building) 
            return false;
        else 
            return base.ShouldMakeDecision();
    }


    // TODO move this to Unit class and make it generic ?
    // TODO is it not too consuming to call this in the Update method?
    protected override void AnimationUpdate() {
        base.AnimationUpdate();
        Animation anim = this.GetComponentInChildren<Animation>();

        if ( moving ) {
            // If moving and the waking animation is not played, play it
            if ( anim && !anim.IsPlaying("Walk")) {
                anim.Play("Walk");
            }
        } else {
            if ( anim && !anim.IsPlaying("Idle")) {
                anim.Play("Idle");
            }
        }
    }

    protected override void InitialiseAudio () {
        base.InitialiseAudio ();
        if(finishedJobVolume < 0.0f) finishedJobVolume = 0.0f;
        if(finishedJobVolume > 1.0f) finishedJobVolume = 1.0f;
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        sounds.Add(finishedJobSound);
        volumes.Add (finishedJobVolume);
        audioElement.Add(sounds, volumes);
    }

 
    /*** Public Methods ***/
 
    public override void SetBuilding (Building project) {
        base.SetBuilding (project);
        currentProject = project;
        StartMove(currentProject.transform.position, currentProject.gameObject);
        building = true;
    }
 
    public override void PerformAction (string actionToPerform) {
        base.PerformAction (actionToPerform);
        CreateBuilding(actionToPerform);
    }
 
    public override void StartMove(Vector3 destination) {
        base.StartMove(destination);
        amountBuilt = 0.0f;
        building = false;
    }

    // Override so that when clicking on a building under construction, the worker goes to build it. Else, standard behaviour.
    public override void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller) {
        bool doBase = true;
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected && hitObject && hitObject.name!="Ground" && hitObject.name!="Bridge" ) {
            Building building = hitObject.transform.parent.GetComponent< Building >();
            if(building) {
                if(building.UnderConstruction()) {
                    SetBuilding(building);
                    doBase = false;
                }
            }
        }
        if (doBase) {
            base.MouseClick(hitObject, hitPoint, controller);
        }
    }



    /*** Private Methods ***/
 
	private void CreateBuilding(string buildingName) {
    	Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
    	if (player) {
    		player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    	}
	}
}