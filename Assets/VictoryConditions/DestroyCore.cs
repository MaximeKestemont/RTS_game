using UnityEngine;
using System.Collections;
using RTS;
 
public class DestroyCore : VictoryCondition {
     
    public int amount = 500;
     
    private ResourceType type = ResourceType.Money;
     
    public override string GetDescription () {
        return "Destroy Core";
    }
     
    public override bool PlayerMeetsConditions (Player player) {
    	// TODO need to check that the core of all other team is destroyed


        return player && !player.IsDead() && player.GetResourceAmount(type) >= amount;
    }
}