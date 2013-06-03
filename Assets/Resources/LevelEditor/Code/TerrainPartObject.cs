using UnityEngine;
using System.Collections;

public class TerrainPartObject : MonoBehaviour {
	TerrainPart terrainPart;
	
	EditorNode[] nodes;
	
	GameObject prefabNode;
	
	void Awake() {
		prefabNode = Resources.Load("LevelEditor/EditorNode") as GameObject;
	}
	
	public void AssignBlueprint(BlueprintPart blueprintPart) {
		// Convert the blueprint into a physical thing
		this.terrainPart = new TerrainPart(blueprintPart);
		this.terrainPart.SetParent(transform);
		this.terrainPart.Regenerate();
		
		// Create the relevant nodes for this object
		Vector3[] p = blueprintPart.CalculatePoints();
		switch (blueprintPart.GetType()) {
		case BlueprintPartType.StraightLine:
			nodes = new EditorNode[2];
			nodes[0] = CreateNode(p[0], 0);
			nodes[1] = CreateNode(p[1], 0);
			break;
		case BlueprintPartType.CurveBezierCubic:
			nodes = new EditorNode[2];
			nodes[0] = CreateNode(p[0], 1);
			nodes[1] = CreateNode(p[p.Length - 1], 1);
			break;
		case BlueprintPartType.CurveCircularArc:
			nodes = new EditorNode[2];
			nodes[0] = CreateNode(p[0], 1);
			nodes[1] = CreateNode(p[1], 1);
			break;
		}
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
		switch (this.terrainPart.blueprintPart.GetType()) {
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
		
		// Then, regenerate the terrain
		this.terrainPart.Regenerate();
	}
}
