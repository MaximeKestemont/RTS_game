using UnityEngine;
using RTS;
 
public class Resource : WorldObject {
 
    // max amount of the resource
    public float capacity;
 
    // amount left + type
    protected float amountLeft;
    protected ResourceType resourceType;
 
    
    /*** Game Engine methods, all can be overridden by subclass ***/
 
    protected override void Start () {
        base.Start();
        amountLeft = capacity;
        resourceType = ResourceType.Unknown;
    }

    // Health display differs from standard units - the color stays the same, but the bar still needs to decrease (done by HealthPercentage)
    [PunRPC]
    protected override void CalculateCurrentHealth (float lowSplit, float highSplit) {
    	healthPercentage = amountLeft / capacity;
    	healthStyle.normal.background = ResourceManager.GetResourceHealthBar(resourceType);
	}
 
    /*** Public methods ***/
 
    public void Remove(float amount) {
        amountLeft -= amount;
        if ( amountLeft < 0 ) { 
        	amountLeft = 0;
        }
    }
 
    public bool isEmpty() {
        return amountLeft <= 0;
    }
 

 	/*** Getters/setters ***/
    public ResourceType GetResourceType() {
        return resourceType;
    }
}