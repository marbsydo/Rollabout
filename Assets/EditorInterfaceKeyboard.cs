using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Super important note:
// This script (EditorInterfaceKeyboard.cs) must execute after EditorNode.cs
// So if it stops working (i.e. cannot draw terrain), go to:
// Edit -> Project Settings -> Script Execution Order
// and ensure that EditorInterfaceKeyboard.cs is listed after EditorNode.cs

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	EditorController editorController;
	
	//GUIText guiText;
	
	enum TextMenu {Main, Navigation, Terrain, Scenery, Objects, Options, LevelSave, LevelLoad};
	TextMenu menu = TextMenu.Main;
	
	enum TerrainStyle {Grass, Snow, Desert};
	TerrainStyle terrainStyle = TerrainStyle.Grass;
	
	// Terrain
	enum TerrainTool {StraightLine, CurveBezierCubic, CurveCircularArc};
	TerrainTool terrainTool = TerrainTool.StraightLine;
	int drawStage = 0;
	Vector3[] drawPoints;
	
	//Options (save/load)
	int levelLoadLevelNum = 0;
	string levelSaveLevelName = "";
	
	// Navigation
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
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
		//guiText = GetComponent<GUIText>(); // Redundant. guiText is defined by Unity
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
				"N   - Navigation\n" +
				"T   - Terrain\n" +
				"S   - Scenery\n" +
				"O   - Objects\n" +
				"Esc - Options";
			
			if (Input.GetKeyDown(KeyCode.N)) {
				SetMenu(TextMenu.Navigation);
			}
			
			if (Input.GetKeyDown(KeyCode.T)) {
				SetMenu(TextMenu.Terrain);
			}
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Options);
			}
			break;
		case TextMenu.Navigation:
			t = "WASD/Arrows - Move\n" +
				"Shift       - Move faster\n" +
				"Esc         - Back";
			
			// Move camera
			editorCameraSpeed = Input.GetKey(KeyCode.LeftShift) ? editorCameraSpeedShift : editorCameraSpeedNormal;
			editorController.GetCamera().gameObject.transform.position += new Vector3(Input.GetAxis("Horizontal") * editorCameraSpeed, Input.GetAxis ("Vertical") * editorCameraSpeed, 0);
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Main);
			}
			
			break;
		case TextMenu.Options:
			t = "S   - Save\n" +
				"L   - Load\n" +
				"P   - Play\n" +
				"Esc - Back";
			
			if (Input.GetKeyDown(KeyCode.S)) {
				SetMenu(TextMenu.LevelSave);
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
		case TextMenu.LevelSave:
			// Show all files in directory
			DirectoryInfo info = new DirectoryInfo(editorController.GetLevelFilepath());
			FileInfo[] fileInfo = info.GetFiles();
			List<string> files = new List<string>();
			foreach (FileInfo file in fileInfo) {
				if (file.Extension == editorController.GetFileExtension())
					files.Add(Path.GetFileNameWithoutExtension(file.Name));
					//t += "\n" + Path.GetFileNameWithoutExtension(file.Name);
			}
			
			t = "";
			
			if (menu == TextMenu.LevelSave) {
				
				foreach (char c in Input.inputString) {
					if (c == "\b"[0]) {
						// backspace
						if (levelSaveLevelName.Length >= 1) {
							levelSaveLevelName = levelSaveLevelName.Substring(0, levelSaveLevelName.Length - 1);
						}
					} else if (c == "\n"[0] || c == "\r"[0]) {
						// return
					} else {
						levelSaveLevelName += c;
					}
				}
				
				t = levelSaveLevelName + "\n";
				
				if (Input.GetKeyDown(KeyCode.Return)) {
					editorController.LevelSave(levelSaveLevelName);
					SetMenu(TextMenu.Main);
				}
			}
			
			if (menu == TextMenu.LevelLoad) {
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
				
				t = files[levelLoadLevelNum] + "\n";
				
				if (Input.GetKeyDown(KeyCode.Return)) {
					editorController.LevelLoad(files[levelLoadLevelNum]);
					SetMenu(TextMenu.Main);
				}
			}
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetMenu(TextMenu.Options);
			}
			
			t += "\nDirectory contents:\n";
			
			foreach (string file in files) {
				t += "\n" + file;
			}
			break;
		case TextMenu.Terrain:
			t = "S/D - Select style\n" +
				"T/Y - Select tool\n" +
				"Del - Delete selected terrain\n" +
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
			
			// Drawing
			// Each new point is attached to the end of the last point
			
			if (Input.GetMouseButtonDown(0)) {
				if (editorController.MouseClaim(gameObject)) {
					drawStage = 1;
					Vector3 m = editorController.GetMousePos();
					drawPoints = new Vector3[2];
					drawPoints[0] = m;
				}
			}
			
			if (Input.GetMouseButtonUp(0)) {
				if (drawStage == 1) {
					drawStage = 0;
					Vector3 m = editorController.GetMousePos();
					drawPoints[1] = m;
					
					//TODO: Use terrainStyle to select style e.g. grass, snow, etc.
					
					// Create the desired blueprint
					BlueprintPartType type;
					
					switch (terrainTool) {
					case TerrainTool.StraightLine:
						type = BlueprintPartType.StraightLine;
						break;
					case TerrainTool.CurveBezierCubic:
						type = BlueprintPartType.CurveBezierCubic;
						break;
					case TerrainTool.CurveCircularArc:
						type = BlueprintPartType.CurveCircularArc;
						break;
					default:
						type = BlueprintPartType.StraightLine;
						Debug.LogWarning("Unknown terrainTool [" + terrainTool + "]. Defaulting to StraightLine");
						break;
					}
	
					TerrainPartMaker terrainPartMaker = new TerrainPartMaker(type);
	
					Vector3 partPointsDiff = drawPoints[1] - drawPoints[0];
					switch (type) {
					case BlueprintPartType.CurveBezierCubic:
						terrainPartMaker.AddNode(drawPoints[0]);
						terrainPartMaker.AddNode(drawPoints[0] + partPointsDiff * 0.25f);
						terrainPartMaker.AddNode(drawPoints[0] + partPointsDiff * 0.75f);
						terrainPartMaker.AddNode(drawPoints[1]);
						break;
					case BlueprintPartType.CurveCircularArc:
						terrainPartMaker.AddNode(drawPoints[0]);
						terrainPartMaker.AddNode(drawPoints[0] + partPointsDiff * 0.5f);
						terrainPartMaker.AddNode(drawPoints[1]);
						break;
					case BlueprintPartType.StraightLine:
						terrainPartMaker.AddNode(drawPoints[0]);
						terrainPartMaker.AddNode(drawPoints[1]);
						break;
					}
	
					terrainPartMaker.SetSegmentLength(2f);
					terrainPartMaker.SetIsEditable(true);
	
					//TerrainPartObject terrain = terrainPartMaker.CreateTerrain();
					terrainPartMaker.CreateTerrain();
	
					editorController.MouseReleaseNextFrame(gameObject);
				}
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
		// Called when a menu ends
		switch (this.menu) {
		case TextMenu.Terrain:
			editorController.NodesDeactivate();
			break;
		}
		
		this.menu = menu;
		
		// Called when a menu starts
		switch (this.menu) {
		case TextMenu.Terrain:
			editorController.NodesActivate();
			break;
		case TextMenu.LevelLoad:
			levelLoadLevelNum = 0;
			break;
		case TextMenu.LevelSave:
			levelSaveLevelName = "";
			break;
		}
	}
	
	void SetText(string t) {
		guiText.text = t;
	}
}