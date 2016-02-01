using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
 
public class MainMenu : Menu {
 
    // Audio related variables
    public AudioClip ambientMusicSound;
    public float ambientMusicVolume = 1.0f;

    protected override void Start() {
        base.Start();

        if (audioElement != null) 
            audioElement.Play(ambientMusicSound);        
    }

    protected override void OnGUI() {
        DrawBackground();
        base.OnGUI();
    }

    protected override void SetButtons () {
        buttons = new string[] {"New Game", "Quit Game"};
    }
 
    protected override void HandleButton (string text) {
        base.HandleButton(text);

        switch(text) {
            case "New Game": 
                if (audioElement != null) audioElement.Stop(ambientMusicSound);
            	NewGame(); 
            	break;
            case "Quit Game": 
                if (audioElement != null) audioElement.Stop(ambientMusicSound);
            	ExitGame(); 
            	break;
            default: 
            	break;
        }
    }

    protected override void InitialiseAudio() {
        base.InitialiseAudio ();
        if (ambientMusicVolume < 0.0f) ambientMusicVolume = 0.0f;
        if (ambientMusicVolume > 1.0f) ambientMusicVolume = 1.0f;
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        sounds.Add(ambientMusicSound);
        volumes.Add (ambientMusicVolume);
        audioElement.Add(sounds, volumes);
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