using UnityEngine;
using System.Collections;


// TODO do something? maybe house for population??
public class Tavern : Building {


	/*** Game Engine methods, all can be overridden by subclass ***/

	// Use this for initialization
    protected override void Start () {
        base.Start();
    }
 
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
    }
}