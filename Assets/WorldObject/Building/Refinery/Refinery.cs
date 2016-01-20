using UnityEngine;
 
public class Refinery : Building {

	/*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start () {
        base.Start();
        actions = new string[] {"Harvester"};
    }
 
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}