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
			levelIO.Load("test.txt");
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
				
				BlueprintPart part;

				if (Input.GetKey(KeyCode.Z)) {
					// Curve bezier cubic

					// Calculate where the two nodes should be: they go part way inbetween each end point
					Vector3 drawPointDiff = drawPoints[1] - drawPoints[0];

					part = new BlueprintPart(BlueprintPartType.CurveBezierCubic, drawPoints[0], drawPoints[0] + drawPointDiff * 0.25f, drawPoints[0] + drawPointDiff * 0.75f, drawPoints[0] + drawPointDiff * 1);
					//part = new BlueprintPart(BlueprintPartType.CurveBezierCubic, drawPoints[0], drawPoints[0] + new Vector3(8, 0, 0), drawPoints[1] - new Vector3(8, 0, 0) ,drawPoints[1]);
				} else if (Input.GetKey(KeyCode.X)) {
					// Circular arc
					part = new BlueprintPart(BlueprintPartType.CurveCircularArc, drawPoints[0], drawPoints[0] + (drawPoints[1] - drawPoints[0]) / 2, drawPoints[1]);
				} else {
					// Straight line
					part = new BlueprintPart(BlueprintPartType.StraightLine, drawPoints[0], drawPoints[1]);
				}

				// Assign the blueprint to a terrain object
				TerrainPartObject terrain = (GameObject.Instantiate(prefabTerrainPartObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<TerrainPartObject>();
				//terrain.gameObject.name = "TerrainPartObject";
				terrain.AssignBlueprint(part);
				
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
	public void Save(string filename) {
		string levelString = ConvertLevelToString();

		// Write levelString to file
		System.IO.File.WriteAllText("Levels/" + filename, levelString);
	}

	public void Load(string filename) {
		string levelString;

		// Read levelString from file
		levelString = System.IO.File.ReadAllText("Levels/" + filename);

		GenerateLevelFromString(levelString);
	}

	public void LoadFromResources(string filename) {
		// Read levelString from Resources
		string levelString = (Resources.Load(filename, typeof(TextAsset)) as TextAsset).text;

		GenerateLevelFromString(levelString);
	}

	string ConvertLevelToString() {
		LevelData levelData = new LevelData();

		//TODO: Write level data to levelData

		// Find all TerrainPartObjects
		TerrainPartObject[] terrains = GameObject.FindObjectsOfType(typeof(TerrainPartObject)) as TerrainPartObject[];
		foreach (TerrainPartObject terrain in terrains) {

		}

		return levelData.ReadAll();
	}

	void GenerateLevelFromString(string levelString) {
		Debug.Log("Generating level from levelString: " + levelString);

		LevelData levelData = new LevelData(levelString);

		//TODO: Generate level from levelData
	}
}

public class LevelData {
	string levelString;
	bool read;

	// If constructed with a string, you can:
	// * Read from string in sections
	// * Read the entire string
	public LevelData(string levelString) {
		this.levelString = levelString;
		read = true;
	}

	// If constructed without a string, you can:
	// * Write to string in sections
	// * Read the entire string
	public LevelData() {
		this.levelString = "";
		read = false;
	}

	public string ReadAll() {
		return levelString;
	}

	//TODO: Functions for writing to level data sequentially in sections

	//TODO: Functions for reading from level data sequentially in sections

	// Conversion functions
	int StringToInt(string s) {
		int i = -1;
		if (!int.TryParse(s, out i))
			Debug.LogWarning("Could not convert integer [" + i + "] to string!");
		return i;
	}

	string IntToString(int i) {
		return i.ToString();
	}
}