using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour {
	
	public enum Scene{Edit, Play};
	
	LevelIO levelIO = new LevelIO();
	
	private string levelFilenameToLoad = "";
	private string levelFilenameLastLoaded = "";
	
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}
	
	public void SceneLoad(Scene scene) {
		switch (scene) {
		case Scene.Edit:
			Application.LoadLevel("LevelEditor");
			break;
		case Scene.Play:
			Application.LoadLevel("LevelPlay");
			break;
		}
	}
	
	public string GetLevelFilenameLastLoaded() {
		return levelFilenameLastLoaded;
	}
	
	public void LevelLoadToEdit(string filename) {
		LevelLoad(0, filename);
	}
	
	public void LevelLoadToPlay(string filename) {
		LevelLoad(1, filename);
	}
	
	public void LevelLoadToEditAdditive(string filename) {
		LevelLoad(2, filename);
	}
	
	private void LevelLoad(int mode, string filename) {
		this.levelFilenameToLoad = filename;
		
		switch (mode) {
		case 0:
			SceneLoad(Scene.Edit);
			break;
		case 1:
			SceneLoad(Scene.Play);
			break;
		case 2:
			levelIO.Load(this.levelFilenameToLoad, true);
			break;
		}
	}
	
	public void LevelSave(string filename) {
		levelIO.Save(filename);
	}
	
	public string GetLevelFilePath() {
		return levelIO.GetLevelFilePath();
	}
	
	public string GetLevelFileExtension() {
		return levelIO.GetLevelFileExtension();
	}
	
	void OnLevelWasLoaded(int level) {
		// Load level if necessary
		if (levelFilenameToLoad.Length > 0) {
			if (Application.loadedLevelName == "LevelEditor") {
				// Call EditorController's load function because it has some post processing
				EditorController editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>();
				editorController.LevelLoad(levelFilenameToLoad);
			} else if (Application.loadedLevelName == "LevelPlay") {
				levelIO.Load(levelFilenameToLoad, false);
			} else {
				Debug.LogWarning("Tried to load level in scene that cannot load level");
			}
			
			// Reset the file
			levelFilenameLastLoaded = levelFilenameToLoad;
			levelFilenameToLoad = "";
		}
	}
}
