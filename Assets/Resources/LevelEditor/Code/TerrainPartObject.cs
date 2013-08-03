using UnityEngine;
using System.Collections;

public class TerrainPartObject : MonoBehaviour {
	public TerrainPart terrainPart;
	
	private bool requireNodes;
	public EditorNode[] nodes;
	
	GameObject prefabNode;

	private float segmentLength = 1f;
	private Color partColor = Color.white;
	
	// Init() is used instead of a constructor because parameters cannot be passed through AddComponent<>()
	// and this class is created in the CreateTerrain() function of class TerrainPartMaker using AddComponent<>()
	// It is necessary that Init() is called before any other functions in this class
	public void Init(bool requireNodes) {
		// If set to true, nodes will be created when AssignBlueprint() is called
		// If set to false, nodes will not be created
		this.requireNodes = requireNodes;
	}

	void Awake() {
		prefabNode = Resources.Load("LevelEditor/EditorNode") as GameObject;
	}

	public void AssignBlueprint(BlueprintPart blueprintPart) {
		// Convert the blueprint into a physical thing
		this.terrainPart = new TerrainPart(blueprintPart);
		this.terrainPart.SetParent(transform);

		if (requireNodes) {
			// Create the relevant nodes for this object
			switch (blueprintPart.GetPartType()) {
			case BlueprintPartType.StraightLine:
				nodes = new EditorNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 0);
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(1), 0);
				break;
			case BlueprintPartType.CurveBezierCubic:
				nodes = new EditorNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 1);
				nodes[0].MoveControl(0, blueprintPart.GetNodePosition(1));
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(3), 1);
				nodes[1].MoveControl(0, blueprintPart.GetNodePosition(2));
				break;
			case BlueprintPartType.CurveCircularArc:
				nodes = new EditorNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 1);
				nodes[0].MoveControl(0, blueprintPart.GetNodePosition(1));
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(2), 0);
				break;
			}
		}

		// Create the terrain with the correct segment lengths
		this.Regenerate();
	}
	
	EditorNode CreateNode(Vector3 pos, int numControls) {
		GameObject g = (GameObject.Instantiate(prefabNode, Vector3.zero, Quaternion.identity) as GameObject);
		EditorNode node = g.GetComponent<EditorNode>();
		node.SetPosition(pos);
		node.CreateHandles(numControls);
		node.SetTerrainPartObject(this);

		// Make the node be a child of the terrain object
		g.transform.parent = this.transform;

		return node;
	}
	
	// Called by EditorNode.cs when the nodes have changed
	public void Regenerate() {
		if (requireNodes) {
			// First, modify the blueprint
			switch (this.terrainPart.blueprintPart.GetPartType()) {
			case BlueprintPartType.StraightLine:
				this.terrainPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.terrainPart.blueprintPart.SetNodePosition(1, nodes[1].GetVertexPosition());
				break;
			case BlueprintPartType.CurveBezierCubic:
				this.terrainPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.terrainPart.blueprintPart.SetNodePosition(1, nodes[0].GetControlPosition(0));
				this.terrainPart.blueprintPart.SetNodePosition(2, nodes[1].GetControlPosition(0));
				this.terrainPart.blueprintPart.SetNodePosition(3, nodes[1].GetVertexPosition());
				break;
			case BlueprintPartType.CurveCircularArc:
				this.terrainPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.terrainPart.blueprintPart.SetNodePosition(1, nodes[0].GetControlPosition(0));
				this.terrainPart.blueprintPart.SetNodePosition(2, nodes[1].GetVertexPosition());
				break;
			default:
				Debug.LogError("BlueprintPartType " + this.terrainPart.blueprintPart.GetType() + " does not exist.");
				break;
			}
		}

		// Set segment length
		this.terrainPart.blueprintPart.SetSegmentLength(segmentLength);

		// Then, regenerate the terrain
		this.terrainPart.Regenerate();

		// Finally, apply the colour
		ApplyColor();
	}

	public void SetSegmentLength(float segmentLength) {
		this.segmentLength = segmentLength;
	}

	public void SegmentLengthIncrease() {
		segmentLength += 0.5f;
		if (segmentLength > 10f)
			segmentLength = 10f;
	}

	public void SegmentLengthDecrease() {
		segmentLength -= 0.5f;
		if (segmentLength < 1f)
			segmentLength = 1f;
	}

	public void SetColor(Color c) {
		this.partColor = c;
	}

	private void ApplyColor() {
		exSprite[] sprites = GetComponentsInChildren<exSprite>();
		foreach (exSprite sprite in sprites) {
			sprite.color = partColor;
		}
	}
}

/*
Example usage:

// Set up inputs
BlueprintPartType blueprintPartType = BlueprintPartType.CurveCircularArc;
float segmentLength = 4f;
bool edit = false;
Vector3 p1 = new Vector3(1, 2, 3);
Vector3 p2 = new Vector3(4, 5, 6);
Vector3 p3 = new Vector3(7, 8, 9);

// Pass inputs to the maker
TerrainPartMaker terrainPartMaker = new TerrainPartMaker(blueprintPartType);
terrainPartMaker.AddNode(p1);
terrainPartMaker.AddNode(p2);
terrainPartMaker.AddNode(p3);
terrainPartMaker.SetSegmentLength(segmentLength);
terrainPartMaker.SetIsEditable(edit);

// Maker returns finished part
TerrainPartObject part = terrainPartMaker.CreateTerrain();

*/
public class TerrainPartMaker {
	BlueprintPart part;
	Vector3[] nodes;
	int nodeCurrent = 0;

	float segmentLength = 1f;
	bool edit;

	public TerrainPartMaker(BlueprintPartType type) {

		// Create the blank blueprint
		switch (type) {
		case BlueprintPartType.StraightLine:
			part = new StraightLine();
			break;
		case BlueprintPartType.CurveBezierCubic:
			part = new CurveBezierCubic();
			break;
		case BlueprintPartType.CurveCircularArc:
			part = new CurveCircularArc();
			break;
		default:
			Debug.LogError("Unknown part [" + part + "]. Defaulting to StraightLine.");
			part = new StraightLine();
			break;
		}

		nodes = new Vector3[part.GetNodeAmount()];
	}

	public int GetNodeAmount() {

		// Get how many nodes the blueprint requires in total
		return nodes.Length;
	}

	public void AddNode(Vector3 v) {
		
		// Add a node to the list of nodes for the blueprint
		if (nodeCurrent >= nodes.Length) {
			Debug.LogError("Trying to add too many nodes!");
		} else {
			nodes[nodeCurrent] = v;
			nodeCurrent++;
		}
	}

	public void SetSegmentLength(float segmentLength) {
		this.segmentLength = segmentLength;
	}

	public void SetIsEditable(bool edit) {
		this.edit = edit;
	}

	public TerrainPartObject CreateTerrain() {

		// Add all nodes to the blueprint
		this.part.SetNodePositions(nodes);

		// Create the terrain object
		GameObject obj = new GameObject() as GameObject;
		TerrainPartObject terrain = obj.AddComponent<TerrainPartObject>();
		terrain.Init(edit);

		terrain.AssignBlueprint(part);

		terrain.SetSegmentLength(segmentLength);

		// Return it
		return terrain;
	}
}