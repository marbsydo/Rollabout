using UnityEngine;
using System.Collections;

public class EditorController : TerrainGenerator {
	
	// Camera
	Camera editorCamera;
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	// Prefabs
	GameObject prefabNode;
	
	// Who has claimed to mouse?
	public bool mouseClaimed = false;
	public GameObject mouseClaimant;
	public bool mouseReleaseNextFrame = false;
	public GameObject mouseReleaseNextFrameClaimant;
	
	// Drawing
	Vector3[] bezierPoint = new Vector3[4];
	int detail = 10;
	int drawStage = 0;
	GameObject[] currentNodes;
	
	void Start() {
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
		
		prefabNode = Resources.Load("LevelEditor/BezierNode") as GameObject;
	}
	
	void Update() {
		
		UpdateMouseRelease();
		
		UpdateCameraMovement();
		UpdatePlaceBall();
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
				currentNodes = new GameObject[2];
				currentNodes[0] = GameObject.Instantiate(prefabNode, m, Quaternion.identity) as GameObject;
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			if (drawStage == 1) {
				drawStage = 0;
				Vector3 m = GetMousePos();
				currentNodes[1] = GameObject.Instantiate(prefabNode, m, Quaternion.identity) as GameObject;
				
				// Create the thing between the two points
				// Calling GenericPart constructor
				
				Vector3 A = currentNodes[0].transform.position;
				Vector3 B = currentNodes[1].transform.position;
				TerrainPart part = new TerrainPart(new GenericPart(GenericPartType.StraightLine, A, B));
				part.Generate();
				MouseReleaseNextFrame(gameObject);
			}
		}
		
		/*
		if (Input.GetMouseButtonDown(0)) {
			
			Vector3 m = editorCamera.ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			
			if (drawStage == 0) {
				bezierPoint[0] = m;
				drawStage++;
			} else if (drawStage == 1) {
				bezierPoint[1] = m;
				drawStage++;
			} else if (drawStage == 2) {
				bezierPoint[2] = m;
				drawStage++;
			} else if (drawStage == 3) {
				bezierPoint[3] = m;
				drawStage = 0;
				
				BezierCubic bezier = new BezierCubic(bezierPoint[0], bezierPoint[1], bezierPoint[2], bezierPoint[3], detail);
				bezier.SetSegmentLength(2);
				BezierPlatformGenerate(bezier);
			}
		}
		*/
	}
	
	Vector3 GetMousePos() {
		Vector3 m = editorCamera.ScreenToWorldPoint(Input.mousePosition);
		m.z = 0;
		return m;
	}
}
