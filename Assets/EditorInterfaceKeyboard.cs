using UnityEngine;
using System.Collections;

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	GUIText guiText;
	
	enum TextMenu {Main, Terrain, Scenery, Objects};
	TextMenu menu = TextMenu.Main;
	
	enum TerrainStyle {Grass, Snow, Desert};
	TerrainStyle terrainStyle = TerrainStyle.Grass;
	
	enum TerrainTool {StraightLine, CurveBezierCubic, CurveCircularArc};
	TerrainTool terrainTool = TerrainTool.StraightLine;
	
	/*
	 * Main menu:
	 * 
	 * ?   - Help
	 * T   - Terrain
	 * S   - Scenery
	 * O   - Objects
	 */
	
	/*
	 * Terrain menu:
	 * 
	 * S   - Select style
	 * T   - Select tool
	 * Esc - Back
	 * 
	 * Current style: Grass
	 * Current tool:  Curve Bezier Cubic
	 */
	
	void Awake() {
		transform.position = new Vector3(0.01f, 0.99f, 0);
		guiText = GetComponent<GUIText>();
	}
	
	void Start() {
		SetMenu(TextMenu.Main);
	}
	
	void Update() {
		UpdateMenu();
	}
	
	void UpdateMenu() {
		string t;
		TextMenu menu = this.menu;
		switch (menu) {
		case TextMenu.Main:
			t = "?   - Help\n" +
				"T   - Terrain\n" +
				"S   - Scenery\n" +
				"O   - Objects";
			if (Input.GetKey(KeyCode.T)) {
				SetMenu(TextMenu.Terrain);
			}
			break;
		case TextMenu.Terrain:
			t = "S/D - Select style\n" +
				"T/Y - Select tool\n" +
				"Esc - Back\n" +
				"\n" +
				"Current style: " + TerrainStyleToText(terrainStyle) + "\n" +
				"Current tool: " + TerrainToolToText(terrainTool) + "\n" +
				"\n" +
				"Use mouse to draw and modify terrain.";
			
			if (Input.GetKey(KeyCode.Escape)) {
				SetMenu(TextMenu.Main);
			}
			
			if (Input.GetKeyDown(KeyCode.S)) {
				terrainStyle++;
				if ((int)terrainStyle > 2)
					terrainStyle = (TerrainStyle)0;
			}
			if (Input.GetKeyDown(KeyCode.D)) {
				terrainStyle--;
				if ((int)terrainStyle < 0)
					terrainStyle = (TerrainStyle)2;
			}
			
			if (Input.GetKeyDown(KeyCode.T)) {
				terrainTool++;
				if ((int)terrainTool > 2)
					terrainTool = (TerrainTool)0;
			}
			if (Input.GetKeyDown(KeyCode.Y)) {
				terrainTool--;
				if ((int)terrainTool < 0)
					terrainTool = (TerrainTool)2;
			}
			break;
		default:
			t = "ERROR\n" +
				"Unknown menu: " + menu;
			break;
		}
		
		SetText(t);
	}
	
	string TerrainStyleToText(TerrainStyle t) {
		string s = "";
		switch (t) {
		case TerrainStyle.Grass:
			s = "Grass";
			break;
		case TerrainStyle.Snow:
			s = "Snow (NOT IMPLEMENTED)";
			break;
		case TerrainStyle.Desert:
			s = "Desert (NOT IMPLEMENTED)";
			break;
		default:
			s = "???";
			break;
		}
		return s;
	}

	string TerrainToolToText(TerrainTool t) {
		string s = "";
		switch (t) {
		case TerrainTool.StraightLine:
			s = "Straight line";
			break;
		case TerrainTool.CurveBezierCubic:
			s = "Curve bezier cubic";
			break;
		case TerrainTool.CurveCircularArc:
			s = "Curve circular arc";
			break;
		default:
			s = "???";
			break;
		}
		return s;
	}
	
	void SetMenu(TextMenu menu) {
		this.menu = menu;
	}
	
	void SetText(string t) {
		guiText.text = t;
	}
}