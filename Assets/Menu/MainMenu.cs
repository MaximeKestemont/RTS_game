using UnityEngine;
using System.Collections;
using RTS;
 
public class MainMenu : Menu {
 

    protected override void OnGUI() {
        DrawBackground();
        base.OnGUI();
    }

    protected override void SetButtons () {
        buttons = new string[] {"New Game", "Quit Game"};
    }
 
    protected override void HandleButton (string text) {
        switch(text) {
            case "New Game": 
            	NewGame(); 
            	break;
            case "Quit Game": 
            	ExitGame(); 
            	break;
            default: 
            	break;
        }
    }
 
    private void NewGame() {
        ResourceManager.MenuOpen = false;

        // Load the starting map.
        Application.LoadLevel("Map");
        
        // makes sure that the loaded level runs at normal speed
        Time.timeScale = 1.0f;
    }

    // Draw the background on the full screen
    private void DrawBackground() {
        GUI.skin = mySkin;

        GUI.BeginGroup(new Rect(0, 0, Screen.width , Screen.height));
        GUI.Box(new Rect(0, 0, Screen.width , Screen.height), "");

        GUI.EndGroup();
    }
}