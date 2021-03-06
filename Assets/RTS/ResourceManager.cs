﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
namespace RTS {
	// This class stores the static global variables (not STATE variables !), that are re-used accross several classes.
    public static class ResourceManager {
		
    	// Weight (so that we can scale depending on the resolution and the screen size)
    	public const float weight = 1.0f;

    	// HUD related variables
    	public const float ORDERS_BAR_WIDTH = 150 * weight;
    	public const float RESOURCE_BAR_HEIGHT = 40 * weight;

    	// Tooltip related variables (for displaying the information about units, etc.)
    	public const float TOOLTIP_WIDTH = 100 * weight;
    	public const float TOOLTIP_HEIGHT = 120 * weight;
    	public const float TEXT_HEIGHT = 30 * weight; 			// height of the text in the tooltip
    	public const float MARGIN = 5 * weight;

    	// Resources related textures and variables
    	public const float RESOURCE_IMAGE_WIDTH = 64 * weight, RESOURCE_IMAGE_HEIGHT = 64 * weight;
    	private static Texture2D goldImage;
    	public static Texture2D GoldImage { get { return goldImage; } }

		// Menu related variables
		public static bool MenuOpen { get; set; }
		private static float buttonHeight = 40;
		private static float headerHeight = 32, headerWidth = 256;
		private static float textHeight = 25, padding = 10;
		public static float PauseMenuHeight { get { return headerHeight + 2 * buttonHeight + 4 * padding; } }
		public static float MenuWidth { get { return headerWidth + 2 * padding; } }
		public static float ButtonHeight { get { return buttonHeight; } }
		public static float ButtonWidth { get { return (MenuWidth - 3 * padding) / 2; } }
		public static float HeaderHeight { get { return headerHeight; } }
		public static float HeaderWidth { get { return headerWidth; } }
		public static float TextHeight { get { return textHeight; } }
		public static float Padding { get { return padding; } } 


		// Camera related variables
		public static float ScrollSpeed { get { return 25; } }
		public static float RotateSpeed { get { return 100; } }
		public static float RotateAmount { get { return 10; } }
		public static int ScrollWidth { get { return 30; } }			// Width from which the camera can be moved (starting from the edge of the screen)
		public static float MinCameraHeight { get { return 10; } }
		public static float MaxCameraHeight { get { return 100; } }


		// Speed to build units in queue
		public static int BuildSpeed { get { return 1; } }
		
		
		// Invalid related variables		
		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		private static Bounds invalidBounds = new Bounds(new Vector3(-99999, -99999, -99999), new Vector3(0, 0, 0));
		public static Vector3 InvalidPosition { get { return invalidPosition; } }
		public static Bounds InvalidBounds { get { return invalidBounds; } }


		// Texture for displaying the health
		private static Texture2D healthyTexture, damagedTexture, criticalTexture;
		public static Texture2D HealthyTexture { get { return healthyTexture; } }
		public static Texture2D DamagedTexture { get { return damagedTexture; } }
		public static Texture2D CriticalTexture { get { return criticalTexture; } }
		private static Dictionary< ResourceType, Texture2D > resourceHealthBarTextures; // for resources (ore,...)
		
		// Set the health bar texture for resources (ore,...)
		public static void SetResourceHealthBarTextures(Dictionary< ResourceType, Texture2D > images) {
    		resourceHealthBarTextures = images;
		}

		// Set the resource icon when displaying the tooltip or the resource bar
		public static void StoreResourcesTextures(Texture2D goldTexture) {
			goldImage = goldTexture; 
		}

		// Get the health bar texture for resources (ore,...)
		public static Texture2D GetResourceHealthBar(ResourceType resourceType) {
    		if(resourceHealthBarTextures != null && resourceHealthBarTextures.ContainsKey(resourceType)) {
    			return resourceHealthBarTextures[resourceType];
    		} else {
    			return null;
    		}
		}


		// Select box skin (stored here as it is the same for all objects)
		private static GUISkin selectBoxSkin;
		public static GUISkin SelectBoxSkin { get { return selectBoxSkin; }}

		public static void StoreSelectBoxItems(GUISkin skin, Texture2D healthy, Texture2D damaged, Texture2D critical) {
			selectBoxSkin = skin;
			healthyTexture = healthy;
    		damagedTexture = damaged;
    		criticalTexture = critical;
		}

		// Menu skin
		private static Texture2D menuSkin;
		public static Texture2D MenuSkin { get { return menuSkin; }}

		public static void StoreMenuSkin(Texture2D menuTexture) {
			menuSkin = menuTexture;
		}


		// GameObject list
		private static GameObjectList gameObjectList;
		public static void SetGameObjectList(GameObjectList objectList) {
    		gameObjectList = objectList;
		}

		// Wrapper method
		public static GameObject GetBuilding(string name) {
    		return gameObjectList.GetBuilding(name);
		}
 
		public static GameObject GetUnit(string name) {
    		return gameObjectList.GetUnit(name);
		}

		public static GameObject GetUnitOrBuilding(string name) {
			if ( gameObjectList.GetUnit(name) ) {
				return gameObjectList.GetUnit(name);
			} else {
				return gameObjectList.GetBuilding(name);
			}
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


		// Player related variables
		private static List<Player> listPlayers = new List<Player>();		// List of players

		public static void AddPlayer(Player player) {
			listPlayers.Add(player);
		}

		public static void RemovePlayer(Player player) {
			listPlayers.Remove(player);
		}

		public static List<Player> GetPlayers() {
			return listPlayers;
		}

		// Team related variables
		private static List<Team> teamList = new List<Team>();
		
		public static void AddTeam(Team team) { teamList.Add(team); }
		public static List<Team> GetTeams() { return teamList; }

    }
}