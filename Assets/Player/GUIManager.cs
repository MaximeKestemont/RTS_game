using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	private bool showRect;
    private Rect selectionRect;


    public void setSelectionBox(bool b, Rect rect) { showRect = b; selectionRect = rect; }
	
	void OnGUI () {
		// So that it does not draw a box at (0,0) when the input 1 is down
        if (showRect && selectionRect.width > 0 && selectionRect.height > 0) {
            GUI.Box(selectionRect, "");
        }
       		
	}
}
