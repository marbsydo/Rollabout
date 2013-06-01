using UnityEngine;
using System.Collections;

public class EditorController : BezierGenerator {
	
	// Camera
	Camera editorCamera;
	float editorCameraSpeed;
	float editorCameraSpeedNormal = 0.5f;
	float editorCameraSpeedShift = 2f;
	
	Vector3 pointA;
	Vector3 pointB;
	Vector3 pointC;
	Vector3 pointD;
	int detail = 10;
	int drawStage = 0;
	
	void Start() {
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
	}
	
	void Update() {
		UpdateCameraMovement();
		UpdateDrawBeziers();
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
				pointA = m;
				pointB = pointA + new Vector3(8, 0, 0);
				drawStage++;
			} else if (drawStage == 1) {
				pointD = m;
				pointC = pointD - new Vector3(8, 0, 0);
				drawStage++;
				
				BezierPlatformGenerate(pointA, pointB, pointC, pointD, detail);
			}
		}
	}
}
