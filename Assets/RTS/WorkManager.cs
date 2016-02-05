﻿using UnityEngine;
using System.Collections.Generic;
 
namespace RTS {
    public static class WorkManager {

        // Calculate the selection box when an object is selected (differs from the selection box to select units !). 
        // Return a Rect that is the the area on which the resource skin should be displayed.
    	public static Rect CalculateSelectionBox(Bounds selectionBounds, Rect playingArea) 
        {
    		// shorthand for the coordinates of the centre of the selection bounds
    		float cx = selectionBounds.center.x;
    		float cy = selectionBounds.center.y;
    		float cz = selectionBounds.center.z;
    		
            // shorthand for the coordinates of the extents of the selection bounds
    		float ex = selectionBounds.extents.x;
    		float ey = selectionBounds.extents.y;
    		float ez = selectionBounds.extents.z;
             
    		// Determine the screen coordinates for the corners of the selection bounds (8 corners because 3D)
    		List< Vector3 > corners = new List< Vector3 >();
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex, cy+ey, cz+ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex, cy+ey, cz-ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex, cy-ey, cz+ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex, cy+ey, cz+ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex, cy-ey, cz-ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex, cy-ey, cz+ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex, cy+ey, cz-ez)));
    		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex, cy-ey, cz-ez)));
             
    		// Determine the bounds on screen for the selection bounds
    		Bounds screenBounds = new Bounds(corners[0], Vector3.zero);
    		for ( int i = 1; i < corners.Count; i++ ) {
    		    screenBounds.Encapsulate(corners[i]);
    		}
             
    		// Screen coordinates start in the bottom left corner, rather than the top left corner
    		// this correction is needed to make sure the selection box is drawn in the correct place
    		float selectBoxTop = playingArea.height - (screenBounds.center.y + screenBounds.extents.y);
    		float selectBoxLeft = screenBounds.center.x - screenBounds.extents.x;
    		float selectBoxWidth = 2 * screenBounds.extents.x;
    		float selectBoxHeight = 2 * screenBounds.extents.y;
             
    		return new Rect( selectBoxLeft, selectBoxTop, selectBoxWidth, selectBoxHeight );
		}


        // Send the first object hitten
        public static GameObject FindHitObject( Vector3 origin ) {
            Ray ray = Camera.main.ScreenPointToRay( origin );
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
            else return null;
        }

        // Send the position of the first object hitten
        public static Vector3 FindHitPoint(Vector3 origin) {
            Ray ray = Camera.main.ScreenPointToRay(origin);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) return hit.point;
            else return ResourceManager.InvalidPosition;
        }

    }
}