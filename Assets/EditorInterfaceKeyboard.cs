using UnityEngine;
using System.Collections;

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	GUIText guiText;
	
	enum TextMenu{Main, Terrain, Scenery, Objects};
	
	TextMenu menu = TextMenu.Main;
	
	string menuMain = 	"T - Terrain\n" +
						"S - Scenery\n" +
						"O - Objects";
	string menuTerrain = "";
	string menuScenery = "";
	string menuObjects = "";
	/*
	 * Main menu:
	 * T - Terrain
	 * S - Scenery
	 * O - Objects
	 */
	
	void Awake() {
		transform.position = new Vector3(0.01f, 0.99f, 0);
		guiText = GetComponent<GUIText>();
	}
	
	void Start() {
		SetMenu(TextMenu.Main);
	}
	
	void Update() {
		
	}
	
	void SetMenu(TextMenu menu) {
		this.menu = menu;
		switch (this.menu) {
		case TextMenu.Main:
			SetText(menuMain);
			break;
		case TextMenu.Terrain:
			SetText(menuTerrain);
			break;
		case TextMenu.Scenery:
			SetText(menuScenery);
			break;
		case TextMenu.Objects:
			SetText(menuObjects);
			break;
		default:
			Debug.LogError("Unknown menu: " + menu);
			break;
		}
	}
	
	void SetText(string t) {
		guiText.text = t;
	}
}