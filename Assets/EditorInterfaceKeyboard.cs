using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// TODO:
// Look at UpdateMenu()
// Add section for TextMenu.Options
// Within these options, add options for save, load and test
// Link up these options to EditorController.cs, in the Update() function
// The code there has been commented out but should be invoked from here instead

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	EditorController editorController;
	
	GUIText guiText;
	
	enum TextMenu {Main, Terrain, Scenery, Objects, Options, LevelLoad};
	TextMenu menu = TextMenu.Main;
	
	enum TerrainStyle {Grass, Snow, Desert};
	TerrainStyle terrainStyle = TerrainStyle.Grass;
	
	enum TerrainTool {StraightLine, CurveBezierCubic, CurveCircularArc};
	TerrainTool terrainTool = TerrainTool.StraightLine;
	
	int levelLoadLevelNum = 0;
	
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
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>() as EditorController;
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
				"O   - Objects\n" +
				"Esc - Options";
			if (Input.GetKeyDown(KeyCode.T)) {
				SetMenu(TextMenu.Terrain);
			}
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Options);
			}
			break;
		case TextMenu.Options:
			t = "S   - Save\n" +
				"L   - Load\n" +
				"P   - Play\n" +
				"Esc - Back";
			
			if (Input.GetKeyDown(KeyCode.S)) {
				editorController.LevelSave("test_save");
			}
			
			if (Input.GetKeyDown(KeyCode.L)) {
				SetMenu(TextMenu.LevelLoad);
			}
			
			if (Input.GetKeyDown(KeyCode.P)) {
				editorController.LevelPlay();
			}
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Main);
			}
			break;
		case TextMenu.LevelLoad:

			
			// Show all files in directory
			DirectoryInfo info = new DirectoryInfo(editorController.GetLevelFilepath());
			FileInfo[] fileInfo = info.GetFiles();
			List<string> files = new List<string>();
			foreach (FileInfo file in fileInfo) {
				if (file.Extension == editorController.GetFileExtension())
					files.Add(Path.GetFileNameWithoutExtension(file.Name));
					//t += "\n" + Path.GetFileNameWithoutExtension(file.Name);
			}
			
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				levelLoadLevelNum++;
			}
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				levelLoadLevelNum--;
			}
			if (levelLoadLevelNum < 0)
				levelLoadLevelNum = 0;
			if (levelLoadLevelNum > (files.Count - 1))
				levelLoadLevelNum = (files.Count - 1);
			
			t = files[levelLoadLevelNum];
			
			t += "\nDirectory contents:\n";
			
			foreach (string file in files) {
				t += "\n" + file;
			}
			
			if (Input.GetKeyDown(KeyCode.Return)) {
				editorController.LevelLoad(files[levelLoadLevelNum]);
			}
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Options);
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
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
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