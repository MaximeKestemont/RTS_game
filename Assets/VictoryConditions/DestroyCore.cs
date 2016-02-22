using UnityEngine;
using System.Collections;
using RTS;
 
public class DestroyCore : VictoryCondition {
     
     
    public override string GetDescription () {
        return "Destroying the Core";
    }
     
    public override bool PlayerMeetsConditions (Player player) {
    	Team[] teams = GameObject.FindObjectsOfType(typeof(Team)) as Team[];

    	// If an enemy team still has its core, then the game is not over
    	foreach ( Team team in teams ) {
    		if ( player.GetTeam() != team ) {

    			if ( team.GetComponentInChildren<FinalCore>() ) {
    				return false;
    			}
    		}
    	}

    	// If all enemy teams do not have their core anymore, then the game is over
    	return true;
    }
}