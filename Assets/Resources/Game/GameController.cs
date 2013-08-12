using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	
	MainController mainController;
	
	void Awake() {
		mainController = (GameObject.Find("MainController") as GameObject).GetComponent<MainController>() as MainController;
	}

	void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			mainController.LevelLoadToEdit(mainController.GetLevelFilenameLastLoaded());
		}
	}
}
