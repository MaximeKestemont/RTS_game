using UnityEngine;
using System.Collections;


// TODO do something
public class Core : Building {


	/*** Game Engine methods, all can be overridden by subclass ***/

	// Use this for initialization
    protected override void Start () {
        base.Start();
    }
 
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
    }
}