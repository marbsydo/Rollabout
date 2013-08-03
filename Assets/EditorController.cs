using UnityEngine;
using System.Collections;

public class EditorController : TerrainGenerator {
	
	// Input
	const KeyCode inputLevelSave = KeyCode.P;
	const KeyCode inputLevelLoad = KeyCode.O;

	// LevelIO
	LevelIO levelIO = new LevelIO();

	// Camera
	Camera editorCamera;
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	// Prefabs
	GameObject prefabTerrainPartObject;
	
	// Who has claimed to mouse?
	public bool mouseClaimed = false;
	public GameObject mouseClaimant;
	public bool mouseReleaseNextFrame = false;
	public GameObject mouseReleaseNextFrameClaimant;
	
	// Drawing
	Vector3[] bezierPoint = new Vector3[4];
	int detail = 10;
	int drawStage = 0;
	Vector3[] drawPoints;
	
	void Start() {
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
		
		prefabTerrainPartObject = Resources.Load("LevelEditor/TerrainPartObject") as GameObject;
	}
	
	void Update() {
		
		UpdateMouseRelease();
		
		UpdateCameraMovement();
		UpdatePlaceBall();

		if (Input.GetKeyDown(inputLevelSave)) {
			levelIO.Save("test.txt");
		} else if (Input.GetKeyDown(inputLevelLoad)) {
			levelIO.Load("test.txt", true);
		}
	}
	
	void LateUpdate() {
		/* UpdateDrawBeziers() must be in LateUpdate because it takes second
		 * priority after BezierNode.cs, who also wants to claim the mouse
		 */
		UpdateDrawBeziers();
	}
	
	void UpdateMouseRelease() {
		if (mouseReleaseNextFrame) {
			MouseRelease(mouseReleaseNextFrameClaimant);
			mouseReleaseNextFrameClaimant = null;
			mouseReleaseNextFrame = false;
		}
	}
	
	public GameObject MouseClaimant() {
		return this.mouseClaimant;
	}

	// Called by other scripts that want to use the mouse
	// They will be denied if the mouse is already in use
	public bool MouseClaim(GameObject mouseClaimant) {
		if (!mouseClaimed) {
			// Mouse is not in use. It can be claimed!
			this.mouseClaimant = mouseClaimant;
			mouseClaimed = true;
			return true;
		} else {
			// Mouse is in use. It cannot be claimed!
			return false;
		}
	}
	
	// Called by other scripts when they are done with the mouse
	// Claimant is required for debugging
	public void MouseRelease(GameObject mouseClaimant) {
		// Mouse is no longer claimed
		mouseClaimed = false;
		this.mouseClaimant = null;
	}
	
	// Called by other scripts when they will be done with the mouse next frame
	// Claimant is required for debugging
	public void MouseReleaseNextFrame(GameObject mouseClaimant) {
		mouseReleaseNextFrame = true;
		mouseReleaseNextFrameClaimant = mouseClaimant;
	}
	
	// Called by other scripts to work out what the current camera is
	public Camera GetCamera() {
		return editorCamera;
	}
	
	void UpdatePlaceBall() {
		if (Input.GetMouseButtonDown(2)) {
			Vector3 m = GetMousePos();
			Object ball = Resources.Load("LevelEditor/TestBall");
			GameObject.Instantiate(ball, m, Quaternion.identity);
		}
	}
	
	void UpdateCameraMovement() {
		// Camera movement
		editorCameraSpeed = Input.GetKey(KeyCode.LeftShift) ? editorCameraSpeedShift : editorCameraSpeedNormal;
		editorCamera.gameObject.transform.position += new Vector3(Input.GetAxis("Horizontal") * editorCameraSpeed, Input.GetAxis ("Vertical") * editorCameraSpeed, 0);
	}
	
	void UpdateDrawBeziers() {
		// Each new point is attached to the end of the last point
		
		if (Input.GetMouseButtonDown(0)) {
			if (MouseClaim(gameObject)) {
				drawStage = 1;
				Vector3 m = GetMousePos();
				drawPoints = new Vector3[2];
				drawPoints[0] = m;
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			if (drawStage == 1) {
				drawStage = 0;
				Vector3 m = GetMousePos();
				drawPoints[1] = m;
				
				// Create the desired blueprint
				BlueprintPartType type;

				if (Input.GetKey(KeyCode.Z)) {
					type = BlueprintPartType.CurveBezierCubic;
				} else if (Input.GetKey(KeyCode.X)) {
					type = BlueprintPartType.CurveCircularArc;
				} else {
					type = BlueprintPartType.StraightLine;
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

				TerrainPartObject terrain = terrainPartMaker.CreateTerrain(true);

				MouseReleaseNextFrame(gameObject);
			}
		}
	}
	
	Vector3 GetMousePos() {
		Vector3 m = editorCamera.ScreenToWorldPoint(Input.mousePosition);
		m.z = 0;
		return m;
	}
}

public class LevelIO {

	private const string filepath = "Levels/";

	public void Save(string filename) {
		string levelString = ConvertLevelToString();

		// Write levelString to file
		System.IO.File.WriteAllText(filepath + filename, levelString);
	}

	public void Load(string filename, bool edit) {
		string levelString;

		// Check the file exists
		if (System.IO.File.Exists(filepath + filename)) {
			// Read levelString from file
			levelString = System.IO.File.ReadAllText(filepath + filename);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
			GenerateLevelFromString(levelString, edit);
		} else {
			Debug.LogWarning("Could not open file [" + filename + "]. Does not exist!");
		}
	}

	public void LoadFromResources(string filename, bool edit) {
		// Read levelString from Resources
		string levelString = (Resources.Load(filename, typeof(TextAsset)) as TextAsset).text;

		GenerateLevelFromString(levelString, edit);
	}

	string ConvertLevelToString() {
		LevelDataWrite levelData = new LevelDataWrite();

		//TODO: Write level data to levelData

		// 1) Write version
		levelData.WriteString("v0.1");

		// Find all TerrainPartObjects
		TerrainPartObject[] terrains = GameObject.FindObjectsOfType(typeof(TerrainPartObject)) as TerrainPartObject[];

		// 2) Write how many terrains there are
		levelData.WriteInt(terrains.Length);

		foreach (TerrainPartObject terrain in terrains) {
			// Loop through each terrain
			BlueprintPartType type = terrain.terrainPart.blueprintPart.GetPartType();
			levelData.WriteInt((int) type);

			// Write points
			for (int i = 0; i < terrain.terrainPart.blueprintPart.GetNodeAmount(); i++) {
				levelData.WriteVector2((Vector2) terrain.terrainPart.blueprintPart.GetNodePosition(i));
			}
		}

		return levelData.ReadAll();
	}

	void GenerateLevelFromString(string levelString, bool edit) {
		Debug.Log("Generating level from levelString: " + levelString);

		LevelDataRead levelData = new LevelDataRead(levelString);

		//TODO: Generate level from levelData

		// 1) Read version

		string version = levelData.ReadString();

		Debug.Log("Version is: " + version);

		// 2) Read how many terrains there are
		int numTerrains = levelData.ReadInt();

		for (int t = 0; t < numTerrains; t++) {

			// Create each terrain
			BlueprintPartType type = (BlueprintPartType) levelData.ReadInt();
			TerrainPartMaker terrainPartMaker = new TerrainPartMaker(type);
			for (int i = 0; i < terrainPartMaker.GetNodeAmount(); i++) {
				terrainPartMaker.AddNode(new Vector3(levelData.ReadFloat(), levelData.ReadFloat(), 0.0f));
			}
			TerrainPartObject terrain = terrainPartMaker.CreateTerrain(edit);
		}
	}
}

abstract public class LevelData {
	protected string levelString;

	public string ReadAll() {
		return levelString;
	}

	// Conversion functions
	protected int StringToInt(string s) {
		int i = -1;
		if (!int.TryParse(s, out i))
			Debug.LogWarning("Could not convert integer [" + i + "] to string!");
		return i;
	}

	protected string IntToString(int i) {
		return i.ToString();
	}

	protected float StringToFloat(string s) {
		float f = -1.0f;
		if (!float.TryParse(s, out f))
			Debug.LogWarning("Could not convert float [" + f + "] to string!");
		return f;
	}

	protected string FloatToString(float f) {
		return f.ToString();
	}
}

public class LevelDataRead : LevelData {
	int pos = 0;

	public LevelDataRead(string levelString) {
		this.levelString = levelString;
	}

	//TODO: Functions for reading from level data sequentially in sections

	public string ReadUntilSpace() {
		string r = "";
		char lastChar = '?';
		while (pos < levelString.Length && lastChar != ' ') {
			lastChar = levelString[pos];
			r += lastChar;
			pos++;
		}
		return r;
	}

	public string ReadNumChars(int num) {
		pos += num;
		return levelString.Substring(pos - num, num);
	}

	public int ReadInt() {
		string i_s = ReadUntilSpace();
		return StringToInt(i_s);
	}

	public float ReadFloat() {
		string f_s = ReadUntilSpace();
		return StringToFloat(f_s);
	}

	public string ReadString() {
		int l = ReadInt();
		return ReadNumChars(l);
	}

	public Vector2 ReadVector2() {
		float vx, vy;
		vx = ReadFloat();
		vy = ReadFloat();
		return new Vector2(vx, vy);
	}
}

public class LevelDataWrite : LevelData {
	public LevelDataWrite() {
		this.levelString = "";
	}

	//TODO: Functions for writing to level data sequentially in sections

	public void WriteRaw(string r) {
		this.levelString += r;
	}

	public void WriteString(string s) {
		// Write string length as integer, then space, then the string itself
		WriteRaw(IntToString(s.Length));
		WriteRaw(" ");
		WriteRaw(s);
	}

	public void WriteInt(int i) {
		// Convert integer to string
		WriteRaw(IntToString(i));
		WriteRaw(" ");
	}

	public void WriteFloat(float f) {
		// Convert float to string
		WriteRaw(FloatToString(f));
		WriteRaw(" ");
	}

	public void WriteVector2(Vector2 v) {
		WriteFloat(v.x);
		WriteFloat(v.y);
	}
}