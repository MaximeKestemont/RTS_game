using UnityEngine;
using System.Collections;
using RTS;

public abstract class VictoryCondition : MonoBehaviour {
     
     /*
    protected Player[] players;
     
    public void SetPlayers(Player[] players) {
        this.players = players;
    }
     
    public Player[] GetPlayers() {
        return players;
    }*/
     
    public virtual bool GameFinished() {
        if ( ResourceManager.GetPlayers() == null ) return true;
        foreach ( Player player in ResourceManager.GetPlayers() ) {
            if (PlayerMeetsConditions(player)) return true;
        }
        return false;
    }
     
    public Player GetWinner() {
        if ( ResourceManager.GetPlayers() == null ) return null;
        foreach ( Player player in ResourceManager.GetPlayers() ) {
            if ( PlayerMeetsConditions(player) ) return player;
        }
        return null;
    }
     
    public abstract string GetDescription();
     
    public abstract bool PlayerMeetsConditions(Player player);
}