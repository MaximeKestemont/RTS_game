using UnityEngine;
 
public class Worker : Unit {
 
    public int buildSpeed;
 
    private Building currentProject;
    private bool building = false;
    private float amountBuilt = 0.0f;
 
    /*** Game Engine methods, all can be overridden by subclass ***/
 
    protected override void Start () {
        base.Start();
        actions = new string[] {"Refinery", "WarFactory"};
    }
 
    protected override void Update () {
        base.Update();
    }
 
    /*** Public Methods ***/
 
    public override void SetBuilding (Building project) {
        base.SetBuilding (project);
        currentProject = project;
        StartMove(currentProject.transform.position, currentProject.gameObject);
        building = true;
    }
 
    public override void PerformAction (string actionToPerform) {
        base.PerformAction (actionToPerform);
        CreateBuilding(actionToPerform);
    }


 
    public override void StartMove(Vector3 destination) {
        base.StartMove(destination);
        amountBuilt = 0.0f;
    }
 
	private void CreateBuilding(string buildingName) {
    	Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
    	if (player) {
    		player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    	}
	}
}