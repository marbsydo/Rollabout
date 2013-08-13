using UnityEngine;
using System.Collections;

public class EditorController : TerrainGenerator {
	
	// Main controller
	MainController mainController;
	
	// Input
	const KeyCode inputLevelSave = KeyCode.P;
	const KeyCode inputLevelLoad = KeyCode.O;
	const KeyCode inputLevelPlay = KeyCode.T;

	// Camera
	Camera editorCamera;
	
	// Who has claimed to mouse?
	public bool mouseClaimed = false;
	public GameObject mouseClaimant;
	public bool mouseReleaseNextFrame = false;
	public GameObject mouseReleaseNextFrameClaimant;
	
	// Terrain nodes
	EditorNode[] nodes;
	
	void Awake() {
		mainController = GameObject.Find("MainController").GetComponent<MainController>();
	}
	
	void Start() {
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
	}
	
	void Update() {
		UpdateMouseRelease();
		UpdatePlaceBall();
	}
	
	public void LevelSave(string filename) {
		mainController.LevelSave(filename);
	}
	
	public void LevelLoad(string filename) {
		mainController.LevelLoadToEditAdditive(filename);
		PostLevelLoad();
	}
	
	// Should be called after a level is loaded
	public void PostLevelLoad() {
		// Activate and deactivate all nodes to ensure all nodes from loaded level are deactivated
		NodesActivate();
		NodesDeactivate();
	}
	
	public void LevelPlay() {
		mainController.LevelSave("__temp_level");
		mainController.LevelLoadToPlay("__temp_level");
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

	public void NodesActivate() {
		// Reactivate all nodes
		if (nodes != null) {
			foreach (EditorNode node in nodes) {
				node.gameObject.SetActive(true);
			}
		}
	}
	
	public void NodesDeactivate() {
		// Deactivate all nodes
		nodes = FindObjectsOfType(typeof(EditorNode)) as EditorNode[];
		foreach (EditorNode node in nodes) {
			node.gameObject.SetActive(false);
		}
	}
	
	public Vector3 GetMousePos() {
		Vector3 m = editorCamera.ScreenToWorldPoint(Input.mousePosition);
		m.z = 0;
		return m;
	}
}