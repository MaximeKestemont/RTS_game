using UnityEngine;
using RTS;
 
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

    public override void DisplayActionTooltip(string tooltipName) {
    	base.DisplayActionTooltip(tooltipName);

    	// TODO
    	GUI.DrawTexture(new Rect(2 * 50, 20, 100, 100), ResourceManager.HealthyTexture);
    }
}