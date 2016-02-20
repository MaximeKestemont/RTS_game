using UnityEngine;
using System.Collections;
using RTS;
 
/**
 * Singleton that handles the management of game state. This includes
 * detecting when a game has been finished and what to do from there.
 */
 
public class GameManager : MonoBehaviour {
     
    private static bool created = false;
    private bool initialised = false;
    private VictoryCondition[] victoryConditions;
    private HUD hud;
     
    void Awake() {
    	// singleton
        if(!created) {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
            initialised = true;
        } else {
            Destroy(this.gameObject);
        }
    }

    void Start() {
    	if(initialised) {
            LoadDetails();
        }
    }
     
    void OnLevelWasLoaded() {
        if(initialised) {
            LoadDetails();
        }
    }
     
    private void LoadDetails() {
        victoryConditions = GameObject.FindObjectsOfType(typeof(VictoryCondition)) as VictoryCondition[];

    }
     
    void Update() {
        if (victoryConditions != null) {
            foreach (VictoryCondition victoryCondition in victoryConditions) {
                if (victoryCondition.GameFinished()) {
                	Player[] players = GameObject.FindObjectsOfType(typeof(Player)) as Player[];

        			foreach (Player player in players) {
            			if (player.human) hud = player.GetComponentInChildren< HUD >(); // TODO change that so that the check is done on the player which owns the session
        			}

                    ResultsScreen resultsScreen = hud.GetComponent< ResultsScreen >();
                    resultsScreen.SetMetVictoryCondition(victoryCondition);
                    resultsScreen.enabled = true;
                    Time.timeScale = 0.0f;
                    Cursor.visible = true;
                    ResourceManager.MenuOpen = true;
                    hud.enabled = false;
                }
            }
        }
    }
     
}