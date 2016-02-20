using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    	switch (tooltipName) {
    		case "Harvester" :
    			GUI.skin = null;
    			GUI.BeginGroup(new Rect(
        			Screen.width - ResourceManager.ORDERS_BAR_WIDTH - ResourceManager.TOOLTIP_WIDTH + ResourceManager.MARGIN, 
        			ResourceManager.RESOURCE_BAR_HEIGHT + ResourceManager.MARGIN, 
        			ResourceManager.TOOLTIP_WIDTH, 
        			ResourceManager.TOOLTIP_HEIGHT));
        		GUI.Box(new Rect(0, 0, Screen.width, Screen.height), ""); // put the texture here instead of ""

        		// WRITE THE TEXT HERE
        		GameObject obj = ResourceManager.GetUnit(tooltipName);
        		Unit unit = obj.GetComponent<Unit>();

        		// Display the name of the object
        		GUI.Label (new Rect( 
        			ResourceManager.MARGIN, 
        			ResourceManager.MARGIN, 
        			Screen.width, 
        			Screen.height), 
        		    "" + unit.objectName);

        		// Display the gold cost (below the object name)
        		GUI.Label (new Rect(
        			ResourceManager.MARGIN * 2, 
        			ResourceManager.MARGIN + ResourceManager.TEXT_HEIGHT, 
        			Screen.width, 
        			Screen.height), 
        		    "" + unit.costValue[0]);
        		
        		// Display the gold icon (right of the cost)
        		GUI.DrawTexture(new Rect(
        			ResourceManager.MARGIN * 2 + 30, 
        			ResourceManager.MARGIN + ResourceManager.TEXT_HEIGHT, 
        			ResourceManager.RESOURCE_IMAGE_WIDTH / 3, 
        			ResourceManager.RESOURCE_IMAGE_HEIGHT / 3), 
        			ResourceManager.GoldImage);

        		GUI.EndGroup();

    			break;
    		default: 
    			break;
    	}
    	
    }
}