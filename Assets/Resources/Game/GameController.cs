using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	void Awake() {
		LevelIO levelIO = new LevelIO();
		levelIO.Load("play" + levelIO.GetLevelFileExtension(), true, false);
	}

	void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			Application.LoadLevel("LevelEditor");
		}
	}
}
