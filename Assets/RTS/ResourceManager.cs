﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
namespace RTS {
	// TODO Whats the goal of this class again??
    public static class ResourceManager {
 
		public static float ScrollSpeed { get { return 25; } }
		public static float RotateSpeed { get { return 100; } }
		public static float RotateAmount { get { return 10; } }

		// Speed to build units in queue
		public static int BuildSpeed { get { return 1; } }
		
		// Width from which the camera can be moved (starting from the edge of the screen)
		public static int ScrollWidth { get { return 15; } }

		public static float MinCameraHeight { get { return 10; } }
		public static float MaxCameraHeight { get { return 40; } }

		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		public static Vector3 InvalidPosition { get { return invalidPosition; } }

		private static Bounds invalidBounds = new Bounds(new Vector3(-99999, -99999, -99999), new Vector3(0, 0, 0));
		public static Bounds InvalidBounds { get { return invalidBounds; } }


		// Texture for displaying the health
		private static Texture2D healthyTexture, damagedTexture, criticalTexture;
		public static Texture2D HealthyTexture { get { return healthyTexture; } }
		public static Texture2D DamagedTexture { get { return damagedTexture; } }
		public static Texture2D CriticalTexture { get { return criticalTexture; } }
		private static Dictionary< ResourceType, Texture2D > resourceHealthBarTextures; // for resources (ore,...)

		// Resource box skin (stored here as it is the same for all objects)
		private static GUISkin selectBoxSkin;
		public static GUISkin SelectBoxSkin { get { return selectBoxSkin; }}

		// GameObject list
		private static GameObjectList gameObjectList;
		public static void SetGameObjectList(GameObjectList objectList) {
    		gameObjectList = objectList;
		}

		// Wrapper method // TODO are those really useful?
		public static GameObject GetBuilding(string name) {
    		return gameObjectList.GetBuilding(name);
		}
 
		public static GameObject GetUnit(string name) {
    		return gameObjectList.GetUnit(name);
		}
 
		public static GameObject GetWorldObject(string name) {
    		return gameObjectList.GetWorldObject(name);
		}
 
		public static GameObject GetPlayerObject() {
    		return gameObjectList.GetPlayerObject();
		}
 
		public static Texture2D GetBuildImage(string name) {
    		return gameObjectList.GetBuildImage(name);
		}

		// Set the health bar texture for resources (ore,...)
		public static void SetResourceHealthBarTextures(Dictionary< ResourceType, Texture2D > images) {
    		resourceHealthBarTextures = images;
		}

		// Get the health bar texture for resources (ore,...)
		public static Texture2D GetResourceHealthBar(ResourceType resourceType) {
    		if(resourceHealthBarTextures != null && resourceHealthBarTextures.ContainsKey(resourceType)) {
    			return resourceHealthBarTextures[resourceType];
    		} else {
    			return null;
    		}
		}

		public static void StoreSelectBoxItems(GUISkin skin, Texture2D healthy, Texture2D damaged, Texture2D critical) {
			selectBoxSkin = skin;
			healthyTexture = healthy;
    		damagedTexture = damaged;
    		criticalTexture = critical;
		}

    }
}