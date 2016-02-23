using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTS;
 
public class Refinery : Building {

	/*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start () {
        base.Start();
        actions = new string[] {"Harvester"};
    }

    protected override bool ShouldMakeDecision () {
        return false;
    }
 

    // TODO remove Refinery here, only for the sake of testing the display
    public override List<Type> GetBuildingRequirements() {
        return new List<Type>() { typeof(WarFactory), typeof(Refinery) };
    }

    // TODO remove Refinery here, only for the sake of testing the display
    public override List<string> GetBuildingNameRequirements() {
        return new List<string>() { "WarFactory", "Refinery" };
    }

    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}