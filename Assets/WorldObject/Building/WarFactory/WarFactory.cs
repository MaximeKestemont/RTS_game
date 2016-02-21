using UnityEngine;
using System.Collections;

public class WarFactory : Building {

	// Use this for initialization
    protected override void Start () {
        base.Start();
        actions = new string[] { "Tank" };
    }

    protected override bool ShouldMakeDecision () {
        return false;
    }
	
	public override void PerformAction(string actionToPerform) {
    	base.PerformAction(actionToPerform);
    	CreateUnit(actionToPerform);
	}
}
