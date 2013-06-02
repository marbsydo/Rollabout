using UnityEngine;
using System.Collections;

public class EditorController : TerrainGenerator {
	
	// Camera
	Camera editorCamera;
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	Vector3[] bezierPoint = new Vector3[4];
	int detail = 10;
	int drawStage = 0;
	
	void Start() {
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
	}
	
	void Update() {
		UpdateCameraMovement();
		UpdateDrawBeziers();
		UpdatePlaceBall();
	}
	
	void UpdatePlaceBall() {
		if (Input.GetMouseButtonDown(1)) {
			Vector3 m = editorCamera.ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			Object ball = Resources.Load("Test/TestBall");
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
	}
}
