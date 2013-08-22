using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	
	MainController mainController;
	
	Camera gameCamera;

	void Awake() {
		mainController = (GameObject.Find("MainController") as GameObject).GetComponent<MainController>() as MainController;

		gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();
	}

	void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			mainController.LevelLoadToEdit(mainController.GetLevelFilenameLastLoaded());
		}

		if (Input.GetMouseButtonDown(0)) {
			Object domino = Resources.Load("Objects/Domino");
			GameObject.Instantiate(domino, GetMousePos(), Quaternion.identity);
		}
	}

	public Vector3 GetMousePos() {
		Vector3 m = gameCamera.ScreenToWorldPoint(Input.mousePosition);
		m.z = 0;
		return m;
	}
}
