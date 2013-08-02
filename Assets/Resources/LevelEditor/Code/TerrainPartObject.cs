using UnityEngine;
using System.Collections;

public class TerrainPartObject : MonoBehaviour {
	public TerrainPart terrainPart;
	
	public EditorNode[] nodes;
	
	GameObject prefabNode;

	private float segmentLength = 3f;
	
	void Awake() {
		prefabNode = Resources.Load("LevelEditor/EditorNode") as GameObject;
	}
	
	public void AssignBlueprint(BlueprintPart blueprintPart) {
		// Convert the blueprint into a physical thing
		this.terrainPart = new TerrainPart(blueprintPart);
		this.terrainPart.SetParent(transform);

		// Create the relevant nodes for this object
		//Vector3[] p = blueprintPart.CalculatePoints();
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

		// Create the terrain with the correct segment lengths
		this.Regenerate();
	}
	
	EditorNode CreateNode(Vector3 pos, int numControls) {
		GameObject g = (GameObject.Instantiate(prefabNode, Vector3.zero, Quaternion.identity) as GameObject);
		EditorNode node = g.GetComponent<EditorNode>();
		node.SetPosition(pos);
		node.CreateHandles(numControls);
		node.SetTerrainPartObject(this);
		return node;
	}
	
	// Called by EditorNode.cs when the nodes have changed
	public void Regenerate() {
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

			this.terrainPart.blueprintPart.SetSegmentLength(segmentLength);
			break;
		case BlueprintPartType.CurveCircularArc:
			this.terrainPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
			this.terrainPart.blueprintPart.SetNodePosition(1, nodes[0].GetControlPosition(0));
			this.terrainPart.blueprintPart.SetNodePosition(2, nodes[1].GetVertexPosition());

			this.terrainPart.blueprintPart.SetSegmentLength(segmentLength);
			break;
		default:
			Debug.LogError("BlueprintPartType " + this.terrainPart.blueprintPart.GetType() + " does not exist.");
			break;
		}
		
		// Then, regenerate the terrain
		this.terrainPart.Regenerate();
	}

	public void SegmentLengthIncrease() {
		segmentLength += 0.5f;
		if (segmentLength > 10f)
			segmentLength = 10f;
		//Debug.Log(segmentLength);
	}

	public void SegmentLengthDecrease() {
		segmentLength -= 0.5f;
		if (segmentLength < 1f)
			segmentLength = 1f;
		//Debug.Log(segmentLength);
	}
}

public class TerrainPartMaker {
	BlueprintPart part;
	Vector3[] nodes;
	int nodeCurrent = 0;

	GameObject prefabTerrainPartObject;

	public TerrainPartMaker(BlueprintPartType type) {

		// Load the template terrain prefab
		prefabTerrainPartObject = Resources.Load("LevelEditor/TerrainPartObject") as GameObject;

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

	public TerrainPartObject CreateTerrain() {

		// Add all nodes to the blueprint
		this.part.SetNodePositions(nodes);

		// Create the terrain obejct
		TerrainPartObject terrain = (GameObject.Instantiate(prefabTerrainPartObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<TerrainPartObject>();
		terrain.AssignBlueprint(part);

		// Return it
		return terrain;
	}
}