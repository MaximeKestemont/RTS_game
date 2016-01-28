using UnityEngine;
using System.Collections;
using RTS;
 
// General class for menu. Override the SetButtons method to add specific buttons to child menu. 
public class Menu : MonoBehaviour {
 
    public GUISkin mySkin;
    public Texture2D header;
 
    protected string[] buttons;
 
    protected virtual void Start () {
        SetButtons();
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
}