using UnityEngine;
using System.Collections;


// TODO tuto part 22 to implement here
public class Turret : Building {


	/*** Game Engine methods, all can be overridden by subclass ***/

	// Use this for initialization
    protected override void Start () {
        base.Start();
    }
 
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
    }
}