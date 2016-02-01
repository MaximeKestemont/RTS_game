using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
 
// General class for menu. Override the SetButtons method to add specific buttons to child menu. 
public class Menu : MonoBehaviour {
 
    // Variables related to the clickSound in the menu
    public AudioClip clickSound;
    public float clickVolume = 1.0f;
    protected AudioElement audioElement;

    public GUISkin mySkin;
    public Texture2D header;
 
    protected string[] buttons;
 
    protected virtual void Start () {
        SetButtons();

        InitialiseAudio();
    }
 
    protected virtual void OnGUI() {
        DrawMenu();
    }
 
// TODO draw a general box for the image, another for the menu

 	// default implementation for a menu consisting of a vertical list of buttons
    protected virtual void DrawMenu() {
        // Reset the skin
        GUI.skin = null;
        float menuHeight = GetMenuHeight();
 
		 // Part of the screen on which it will draw the menu
        float groupLeft = Screen.width / 2 - ResourceManager.MenuWidth / 2;
        float groupTop = Screen.height / 2 - menuHeight / 2;
        GUI.BeginGroup(new Rect(groupLeft, groupTop, ResourceManager.MenuWidth, menuHeight));
 
        // background box
        GUI.Box(new Rect(0, 0, ResourceManager.MenuWidth, menuHeight), "");
        // header image
        GUI.DrawTexture(new Rect(ResourceManager.Padding, ResourceManager.Padding, ResourceManager.HeaderWidth, ResourceManager.HeaderHeight), header);
 
        // menu buttons
        if(buttons != null) {
            float leftPos = ResourceManager.MenuWidth / 2 - ResourceManager.ButtonWidth / 2;
            float topPos = 2 * ResourceManager.Padding + header.height;
            for(int i = 0; i < buttons.Length; i++) {                if(i > 0) topPos += ResourceManager.ButtonHeight + ResourceManager.Padding;
                if(GUI.Button(new Rect(leftPos, topPos, ResourceManager.ButtonWidth, ResourceManager.ButtonHeight), buttons[i])) {
                    HandleButton(buttons[i]);
                }
            }
        }
 
        GUI.EndGroup();
    }
 
    protected virtual void SetButtons() {
        //a child class needs to set this for buttons to appear
    }
 
    protected virtual void HandleButton(string text) {
        if (audioElement != null) 
            audioElement.Play(clickSound);
        
        //a child class needs to set this to handle button clicks
    }
 
    protected virtual float GetMenuHeight() {
        float buttonHeight = 0;
        if (buttons != null) {
        	buttonHeight = buttons.Length * ResourceManager.ButtonHeight;
        }
        float paddingHeight = 2 * ResourceManager.Padding;
        if (buttons != null) {
        	paddingHeight += buttons.Length * ResourceManager.Padding;
        }
        return ResourceManager.HeaderHeight + buttonHeight + paddingHeight;
    }
 
    protected void ExitGame() {
        Application.Quit();
    }

    // Initialise the audio settings by creating the audio element containing the audio objects (containing the audio clip).
    protected virtual void InitialiseAudio() {
        List< AudioClip > sounds = new List< AudioClip >();
        List< float > volumes = new List< float >();
        
        // TODO refactor those 3 calls (need to check how to use pointers inside safe context)
        if (clickVolume < 0.0f) clickVolume = 0.0f;
        if (clickVolume > 1.0f) clickVolume = 1.0f;
        sounds.Add(clickSound);
        volumes.Add(clickVolume);
        audioElement = new AudioElement(sounds, volumes, "Menu", this.transform);
    }
}