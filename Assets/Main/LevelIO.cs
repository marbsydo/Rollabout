using UnityEngine;
using System.Collections;

public class LevelIO {
	
	public string GetLevelFilePath() {
		return Application.dataPath + "/levels/";
	}
	
	public string GetLevelFileExtension() {
		return ".ewfg";
	}
	
	public void Save(string filename) {
		string levelString = ConvertLevelToString();
		
		string fullPath = GetLevelFilePath() + filename + GetLevelFileExtension();
		
		Debug.Log("Saving: " + fullPath);
		
		// Write levelString to file
		System.IO.File.WriteAllText(fullPath, levelString);
	}

	public void Load(string filename, bool edit) {
		string levelString;
		
		string fullPath = GetLevelFilePath() + filename + GetLevelFileExtension();
		
		Debug.Log("Loading: " + fullPath);
		
		// Check the file exists
		if (System.IO.File.Exists(fullPath)) {
			// Read levelString from file
			levelString = System.IO.File.ReadAllText(fullPath);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
			GenerateLevelFromString(levelString, edit);
		} else {
			Debug.LogWarning("Could not open file [" + filename + "]. Does not exist!");
		}
	}

	public void LoadFromResources(string filename, bool edit) {
		// Read levelString from Resources
		string levelString = (Resources.Load(filename, typeof(TextAsset)) as TextAsset).text;

		GenerateLevelFromString(levelString, edit);
	}

	string ConvertLevelToString() {
		LevelDataWrite levelData = new LevelDataWrite();

		//TODO: Write level data to levelData

		// 1) Write version
		levelData.WriteString("v0.1");

		// Find all TerrainGrounds
		TerrainGround[] grounds = GameObject.FindObjectsOfType(typeof(TerrainGround)) as TerrainGround[];

		// 2) Write how many grounds there are
		levelData.WriteInt(grounds.Length);

		foreach (TerrainGround ground in grounds) {

			// Loop through each ground
			TerrainBlueprintType type = ground.groundPart.blueprintPart.GetTerrainBlueprintType();
			levelData.WriteInt((int) type);

			levelData.WriteInt((int) ground.style);

			// Write segment length
			levelData.WriteFloat(ground.groundPart.blueprintPart.GetSegmentLength());

			// Write points
			for (int i = 0; i < ground.groundPart.blueprintPart.GetNodeAmount(); i++) {
				levelData.WriteVector2((Vector2) ground.groundPart.blueprintPart.GetNodePosition(i));
			}
		}

		// Find all TerrainRollers
		TerrainRoller[] rollers = GameObject.FindObjectsOfType(typeof(TerrainRoller)) as TerrainRoller[];

		// 3) Write how many rollers there are
		levelData.WriteInt(rollers.Length);

		foreach (TerrainRoller roller in rollers) {

			// Loop through each roller
			TerrainBlueprintType type = roller.rollerPart.blueprintPart.GetTerrainBlueprintType();
			levelData.WriteInt((int) type);

			levelData.WriteInt((int) roller.style);
			levelData.WriteFloat(roller.spacing);
			levelData.WriteBool(roller.isFixed);
			levelData.WriteFloat(roller.speed);

			// Write points
			for (int i = 0; i < roller.rollerPart.blueprintPart.GetNodeAmount(); i++) {
				levelData.WriteVector2((Vector2) roller.rollerPart.blueprintPart.GetNodePosition(i));
			}
		}

		return levelData.ReadAll();
	}

	void GenerateLevelFromString(string levelString, bool edit) {
		Debug.Log("Generating level from levelString: " + levelString);

		LevelDataRead levelData = new LevelDataRead(levelString);

		//TODO: Generate level from levelData

		// 1) Read version

		string version = levelData.ReadString();

		Debug.Log("Version is: " + version);

		// 2) Read how many terrains there are
		int numTerrains = levelData.ReadInt();

		for (int t = 0; t < numTerrains; t++) {
			
			// Read type
			TerrainBlueprintType type = (TerrainBlueprintType) levelData.ReadInt();

			// Read style
			TerrainGroundStyle style = (TerrainGroundStyle) levelData.ReadInt();

			// Read segment length
			float segmentLength = levelData.ReadFloat();

			TerrainInfo terrainInfo = new TerrainInfo(type, style, segmentLength);
			TerrainObjectMaker terrainObjectMaker = new TerrainObjectMaker(terrainInfo);

			for (int i = 0; i < terrainObjectMaker.GetNodeAmount(); i++) {
				terrainObjectMaker.AddNode((Vector3) levelData.ReadVector2());
			}

			terrainObjectMaker.SetIsEditable(edit);
			terrainObjectMaker.CreateTerrain();
		}

		// 3) Read how many rollers there are
		int numRollers = levelData.ReadInt();

		for (int t = 0; t < numRollers; t++) {

			// Read type
			TerrainBlueprintType type = (TerrainBlueprintType) levelData.ReadInt();

			// Read style
			TerrainRollerStyle style = (TerrainRollerStyle) levelData.ReadInt();
			float spacing = levelData.ReadFloat();
			bool isFixed = levelData.ReadBool();
			float speed = levelData.ReadFloat();

			TerrainInfo terrainInfo = new TerrainInfo(type, style, spacing, isFixed, speed);
			TerrainObjectMaker terrainObjectMaker = new TerrainObjectMaker(terrainInfo);

			for (int i = 0; i < terrainObjectMaker.GetNodeAmount(); i++) {
				terrainObjectMaker.AddNode((Vector3) levelData.ReadVector2());
			}

			terrainObjectMaker.SetIsEditable(edit);
			terrainObjectMaker.CreateTerrain();
		}
	}
}

abstract public class LevelData {
	protected string levelString;

	public string ReadAll() {
		return levelString;
	}

	// Conversion functions
	protected int StringToInt(string s) {
		int i = -1;
		if (!int.TryParse(s, out i))
			Debug.LogWarning("Could not convert integer [" + i + "] to string!");
		return i;
	}

	protected string IntToString(int i) {
		return i.ToString();
	}

	protected float StringToFloat(string s) {
		float f = -1.0f;
		if (!float.TryParse(s, out f))
			Debug.LogWarning("Could not convert float [" + f + "] to string!");
		return f;
	}

	protected string FloatToString(float f) {
		return f.ToString();
	}
}

public class LevelDataRead : LevelData {
	int pos = 0;

	public LevelDataRead(string levelString) {
		this.levelString = levelString;
	}

	//TODO: Functions for reading from level data sequentially in sections

	public string ReadUntilSpace() {
		string r = "";
		char lastChar = '?';
		while (pos < levelString.Length && lastChar != ' ') {
			lastChar = levelString[pos];
			r += lastChar;
			pos++;
		}
		return r;
	}

	public string ReadNumChars(int num) {
		pos += num;
		return levelString.Substring(pos - num, num);
	}

	public int ReadInt() {
		string i_s = ReadUntilSpace();
		return StringToInt(i_s);
	}

	public float ReadFloat() {
		string f_s = ReadUntilSpace();
		return StringToFloat(f_s);
	}

	public bool ReadBool() {
		return (ReadInt() == 1 ? true : false);
	}

	public string ReadString() {
		int l = ReadInt();
		return ReadNumChars(l);
	}

	public Vector2 ReadVector2() {
		float vx, vy;
		vx = ReadFloat();
		vy = ReadFloat();
		return new Vector2(vx, vy);
	}
}

public class LevelDataWrite : LevelData {
	public LevelDataWrite() {
		this.levelString = "";
	}

	//TODO: Functions for writing to level data sequentially in sections

	public void WriteRaw(string r) {
		this.levelString += r;
	}

	public void WriteString(string s) {
		// Write string length as integer, then space, then the string itself
		WriteRaw(IntToString(s.Length));
		WriteRaw(" ");
		WriteRaw(s);
	}

	public void WriteInt(int i) {
		// Convert integer to string
		WriteRaw(IntToString(i));
		WriteRaw(" ");
	}

	public void WriteFloat(float f) {
		// Convert float to string
		WriteRaw(FloatToString(f));
		WriteRaw(" ");
	}

	public void WriteBool(bool b) {
		WriteInt(b ? 1 : 0);
	}

	public void WriteVector2(Vector2 v) {
		WriteFloat(v.x);
		WriteFloat(v.y);
	}
}