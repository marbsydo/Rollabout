using UnityEngine;
using System.Collections;

public class EditorController : MonoBehaviour {
	
	Camera camera;
	float cameraSpeed;
	float cameraSpeedNormal = 0.5f;
	float cameraSpeedShift = 2f;
	
	void Start() {
		camera = GameObject.Find("EditorCamera").GetComponent<Camera>();
	}
	
	void Update() {
		// Camera movement
		cameraSpeed = Input.GetKey(KeyCode.LeftShift) ? cameraSpeedShift : cameraSpeedNormal;
		camera.gameObject.transform.position += new Vector3(Input.GetAxis("Horizontal") * cameraSpeed, Input.GetAxis ("Vertical") * cameraSpeed, 0);
	}
}
