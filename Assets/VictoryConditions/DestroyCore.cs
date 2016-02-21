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
        return player && !player.IsDead() && player.GetResourceAmount(type) >= amount;
    }
}