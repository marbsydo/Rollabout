using UnityEngine;
using System.Collections;

public class LevelIO {
	
	public string GetFilepath(bool temp) {
		string p = Application.dataPath;
		if (!temp)
			p += "/levels/";
		else
			p += "/levels/temp/";
		return p;
	}
	
	public string GetLevelFileExtension() {
		return ".ewfg";
	}
	
	public void Save(string filename, bool temp) {
		string levelString = ConvertLevelToString();

		// Write levelString to file
		System.IO.File.WriteAllText(GetFilepath(temp) + filename, levelString);
	}

	public void Load(string filename, bool temp, bool edit) {
		string levelString;

		// Check the file exists
		if (System.IO.File.Exists(GetFilepath(temp) + filename)) {
			// Read levelString from file
			levelString = System.IO.File.ReadAllText(GetFilepath(temp) + filename);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
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

		// Find all TerrainPartObjects
		TerrainPartObject[] terrains = GameObject.FindObjectsOfType(typeof(TerrainPartObject)) as TerrainPartObject[];

		// 2) Write how many terrains there are
		levelData.WriteInt(terrains.Length);

		foreach (TerrainPartObject terrain in terrains) {
			// Loop through each terrain
			BlueprintPartType type = terrain.terrainPart.blueprintPart.GetPartType();
			levelData.WriteInt((int) type);

			// Write points
			for (int i = 0; i < terrain.terrainPart.blueprintPart.GetNodeAmount(); i++) {
				levelData.WriteVector2((Vector2) terrain.terrainPart.blueprintPart.GetNodePosition(i));
			}

			// Write segment length
			levelData.WriteFloat(terrain.terrainPart.blueprintPart.GetSegmentLength());
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

			// Create each terrain
			BlueprintPartType type = (BlueprintPartType) levelData.ReadInt();
			TerrainPartMaker terrainPartMaker = new TerrainPartMaker(type);
			for (int i = 0; i < terrainPartMaker.GetNodeAmount(); i++) {
				terrainPartMaker.AddNode(new Vector3(levelData.ReadFloat(), levelData.ReadFloat(), 0.0f));
			}

			// Read segments length
			float segmentLength = levelData.ReadFloat();
			terrainPartMaker.SetSegmentLength(segmentLength);

			terrainPartMaker.SetIsEditable(edit);

			TerrainPartObject terrain = terrainPartMaker.CreateTerrain();
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

	public void WriteVector2(Vector2 v) {
		WriteFloat(v.x);
		WriteFloat(v.y);
	}
}