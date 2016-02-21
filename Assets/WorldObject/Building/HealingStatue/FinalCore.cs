using UnityEngine;
using System.Collections;


// TODO change to CoreStatue, which gives something to the opponent when destroyed
public class FinalCore : Building {


	/*** Game Engine methods, all can be overridden by subclass ***/

	// Use this for initialization
    protected override void Start () {
        base.Start();
    }
 
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
    }
}