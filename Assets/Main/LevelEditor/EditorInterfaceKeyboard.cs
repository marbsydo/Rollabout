using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Super important note:
// This script (EditorInterfaceKeyboard.cs) must execute after EditorNode.cs
// So if it stops working (i.e. cannot draw terrain), go to:
// Edit -> Project Settings -> Script Execution Order
// and ensure that EditorInterfaceKeyboard.cs is listed after EditorNode.cs

// The last item is set to __Length to so then the length can be read by doing (int)TerrainStyle.__Length
public enum TerrainStyle {GroundGrass, RollersGeneral, __Length};
public enum TerrainTool {StraightLine, CurveBezierCubic, CurveCircularArc, __Length};
public enum TextMenu {Main, Navigation, Terrain, Scenery, Objects, Options, LevelSave, LevelLoad};

[RequireComponent (typeof(GUIText))]
public class EditorInterfaceKeyboard : MonoBehaviour {
	
	MainController mainController;
	EditorController editorController;
	
	TextMenu menuCurrent = TextMenu.Main;
	List<MenuAbstract> menus;
	
	void Awake() {
		// Move text to top right corner
		transform.position = new Vector3(0.01f, 0.99f, 0);
		
		// Get references
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>() as EditorController;
		mainController = (GameObject.Find("MainController") as GameObject).GetComponent<MainController>() as MainController;
		
		// Create all objects for the various menus
		// NOTE:
		// Menu objects are accessed by casting `menuCurrent` to int for the array index
		// As a result, the following Add()s must be in the same order as enum TextMenu
		menus = new List<MenuAbstract>();
		menus.Add(new MenuMain());
		menus.Add(new MenuNavigation());
		menus.Add(new MenuTerrain());
		menus.Add(new MenuScenery());
		menus.Add(new MenuObjects());
		menus.Add(new MenuOptions());
		menus.Add(new MenuLevelSave());
		menus.Add(new MenuLevelLoad());
		menus.Add(new MenuError());
		
		// Give each menu object required references
		foreach (MenuAbstract menu in menus) {
			menu.SetReferances(this, mainController, editorController);
		}
	}
	
	void Start() {
		SetMenu(TextMenu.Main);
	}
	
	void Update() {
		guiText.text = menus[(int)menuCurrent].Text();
	}
	
	public void SetMenu(TextMenu menu) {
		menus[(int)menuCurrent].End();
		menuCurrent = menu;
		menus[(int)menuCurrent].Begin();
	}
}

abstract class MenuAbstract {
	
	protected EditorInterfaceKeyboard editorInterfaceKeyboard;
	protected MainController mainController;
	protected EditorController editorController;
	
	abstract public void Begin();
	abstract public string Text();
	abstract public void End();
	
	public void SetReferances(EditorInterfaceKeyboard editorInterfaceKeyboard, MainController mainController, EditorController editorController) {
		this.editorInterfaceKeyboard = editorInterfaceKeyboard;
		this.mainController = mainController;
		this.editorController = editorController;
	}
	
	protected void SetMenu(TextMenu menu) {
		editorInterfaceKeyboard.SetMenu(menu);
	}
}

class MenuMain : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
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
		
		if (Input.GetKeyDown(KeyCode.O)) {
			SetMenu(TextMenu.Objects);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Options);
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuNavigation : MenuAbstract {
	
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "WASD/Arrows - Move\n" +
			"Shift       - Move faster\n" +
			"Esc         - Back";
		
		// Move camera
		editorCameraSpeed = Input.GetKey(KeyCode.LeftShift) ? editorCameraSpeedShift : editorCameraSpeedNormal;
		editorController.GetCamera().gameObject.transform.position += new Vector3(Input.GetAxis("Horizontal") * editorCameraSpeed, Input.GetAxis ("Vertical") * editorCameraSpeed, 0);
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuTerrain : MenuAbstract {
	
	TerrainStyle terrainStyle = TerrainStyle.GroundGrass;

	int terrainStyleMax = (int)TerrainStyle.__Length;
	int terrainToolMax = (int)TerrainTool.__Length;

	TerrainTool terrainTool = TerrainTool.StraightLine;
	int drawStage = 0;
	Vector3[] drawPoints;
	
	override public void Begin() {
		editorController.NodesActivate();
	}
	
	override public string Text() {
		string t;
		
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
			if ((int)terrainStyle >= terrainStyleMax)
				terrainStyle = (TerrainStyle)0;
		}
		if (Input.GetKeyDown(KeyCode.D)) {
			terrainStyle--;
			if ((int)terrainStyle < 0)
				terrainStyle = (TerrainStyle)terrainStyleMax;
		}
		
		if (Input.GetKeyDown(KeyCode.T)) {
			terrainTool++;
			if ((int)terrainTool >= terrainToolMax)
				terrainTool = (TerrainTool)0;
		}
		if (Input.GetKeyDown(KeyCode.Y)) {
			terrainTool--;
			if ((int)terrainTool < 0)
				terrainTool = (TerrainTool)terrainToolMax;
		}
		
		// Drawing
		// Each new point is attached to the end of the last point
		
		if (Input.GetMouseButtonDown(0)) {
			if (editorController.MouseClaim(editorInterfaceKeyboard.gameObject)) {
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

				TerrainObjectMaker terrainObjectMaker = new TerrainObjectMaker(type);

				Vector3 partPointsDiff = drawPoints[1] - drawPoints[0];
				switch (type) {
				case BlueprintPartType.CurveBezierCubic:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.25f);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.75f);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				case BlueprintPartType.CurveCircularArc:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[0] + partPointsDiff * 0.5f);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				case BlueprintPartType.StraightLine:
					terrainObjectMaker.AddNode(drawPoints[0]);
					terrainObjectMaker.AddNode(drawPoints[1]);
					break;
				}

				terrainObjectMaker.SetSegmentLength(2f);
				terrainObjectMaker.SetIsEditable(true);

				//TerrainPartObject terrain = terrainObjectMaker.CreateTerrain();
				terrainObjectMaker.CreateTerrain();

				editorController.MouseReleaseNextFrame(editorInterfaceKeyboard.gameObject);
			}
		}
		
		return t;
	}
	
	override public void End() {
		editorController.NodesDeactivate();
	}
	
	string TerrainStyleToText(TerrainStyle t) {
		string s = "";
		switch (t) {
		case TerrainStyle.GroundGrass:
			s = "Grass";
			break;
		case TerrainStyle.RollersGeneral:
			s = "Rollers";
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
}

class MenuScenery : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		return "Scenery menu is not done yet";
	}
	
	override public void End() {
	}
}

class MenuObjects : MenuAbstract {
	
	enum EditorObject {Player, Rock};
	
	EditorObject currentObject = EditorObject.Player;
	
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
	
		t = "Current object: " + EditorObjectToString(currentObject) + 
			"\n" +
			"P   - Player\n" +
			"R   - Rock\n" +
			"\n" +
			"Esc - Back\n";
		
		if (Input.GetKeyDown(KeyCode.P)) {
			SetCurrentObject(EditorObject.Player);
		}
		
		if (Input.GetKeyDown(KeyCode.R)) {
			SetCurrentObject(EditorObject.Rock);
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Main);
		}
		
		return t;
	}
	
	override public void End() {
	}
	
	void SetCurrentObject(EditorObject currentObject) {
		this.currentObject = currentObject;
	}
	
	string EditorObjectToString(EditorObject editorObject) {
		string t = "";
		switch (editorObject) {
		case EditorObject.Player:
			t = "Player";
			break;
		case EditorObject.Rock:
			t = "Rock";
			break;
		}
		return t;
	}
}

class MenuOptions : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
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
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuLevelSave : MenuAbstract {
	
	string levelSaveLevelName = "";
	
	override public void Begin() {
		levelSaveLevelName = "";
	}
	
	override public string Text() {
		string t;
		
		// Show all files in directory
		DirectoryInfo info = new DirectoryInfo(mainController.GetLevelFilePath());
		FileInfo[] fileInfo = info.GetFiles();
		List<string> files = new List<string>();
		foreach (FileInfo file in fileInfo) {
			if (file.Extension == mainController.GetLevelFileExtension())
				files.Add(Path.GetFileNameWithoutExtension(file.Name));
		}
		
		t = "";
		
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
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetMenu(TextMenu.Options);
		}
		
		t += "\nDirectory contents:\n";
		
		foreach (string file in files) {
			t += "\n" + file;
		}
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuLevelLoad : MenuAbstract {
	
	int levelLoadLevelNum = 0;
	
	override public void Begin() {
		levelLoadLevelNum = 0;
	}
	
	override public string Text() {
		string t;
		
		// Show all files in directory
		DirectoryInfo info = new DirectoryInfo(mainController.GetLevelFilePath());
		FileInfo[] fileInfo = info.GetFiles();
		List<string> files = new List<string>();
		foreach (FileInfo file in fileInfo) {
			if (file.Extension == mainController.GetLevelFileExtension())
				files.Add(Path.GetFileNameWithoutExtension(file.Name));
		}
		
		t = "";
		
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
		
		if (files.Count > 0) {
			t = files[levelLoadLevelNum];
		} else {
			// No level files in directory so show blank
			t = "";
		}
		t += "\n";
		
		if (Input.GetKeyDown(KeyCode.Return)) {
			// Check the array of levels is not empty
			if (files.Count > 0) {
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
		
		return t;
	}
	
	override public void End() {
	}
}

class MenuError : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
		string t;
		
		t = "Esc - Main menu\n" +
			"\n" +
			"Error! This menu does not exist.";
		
		return t;
	}
	
	override public void End() {
	}
}

/*
class MenuTemplate : MenuAbstract {
	override public void Begin() {
	}
	
	override public string Text() {
	}
	
	override public void End() {
	}
}
*/