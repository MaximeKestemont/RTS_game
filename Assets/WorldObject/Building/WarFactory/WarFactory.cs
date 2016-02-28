using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WarFactory : Building {

	// Use this for initialization
    protected override void Start () {
        base.Start();
        actions = new string[] { "Tank" };
    }

    protected override bool ShouldMakeDecision () {
        return false;
    }

    public override List<string> GetBuildingNameRequirements() {
        return new List<string>() { "Refinery" };
    }

    public override List<Type> GetBuildingRequirements() {
        return new List<Type>() { typeof(Refinery) };
    }
	
	public override void PerformAction(string actionToPerform) {
    	base.PerformAction(actionToPerform);
    	CreateUnit(actionToPerform);
	}
}
