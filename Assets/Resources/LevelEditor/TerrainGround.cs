using UnityEngine;
using System.Collections;

public class TerrainGround : TerrainBase {

	public GroundPart groundPart;
	private TerrainGroundStyle terrainGroundStyle;

	public void SetTerrainGroundStyle(TerrainGroundStyle terrainGroundStyle) {
		this.terrainGroundStyle = terrainGroundStyle;
	}

	public override void AssignBlueprint(BlueprintPart blueprintPart) {
		// Convert the blueprint into a physical thing
		this.groundPart = new GroundPart(blueprintPart, this.terrainGroundStyle);
		this.groundPart.SetParent(transform);

		if (requireNodes) {
			// Create the relevant nodes for this object
			switch (blueprintPart.GetTerrainBlueprintType()) {
			case TerrainBlueprintType.StraightLine:
				nodes = new ObjectNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 0);
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(1), 0);
				break;
			case TerrainBlueprintType.CurveBezierCubic:
				nodes = new ObjectNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 1);
				nodes[0].MoveControl(0, blueprintPart.GetNodePosition(1));
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(3), 1);
				nodes[1].MoveControl(0, blueprintPart.GetNodePosition(2));
				break;
			case TerrainBlueprintType.CurveCircularArc:
				nodes = new ObjectNode[2];
				nodes[0] = CreateNode(blueprintPart.GetNodePosition(0), 1);
				nodes[0].MoveControl(0, blueprintPart.GetNodePosition(1));
				nodes[1] = CreateNode(blueprintPart.GetNodePosition(2), 0);
				break;
			}
		}

		// Create the terrain with the correct segment lengths
		this.Regenerate();
	}
	
	// Called by ObjectNode.cs when the nodes have changed
	public override void Regenerate() {
		if (requireNodes) {
			// First, modify the blueprint
			switch (this.groundPart.blueprintPart.GetTerrainBlueprintType()) {
			case TerrainBlueprintType.StraightLine:
				this.groundPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.groundPart.blueprintPart.SetNodePosition(1, nodes[1].GetVertexPosition());
				break;
			case TerrainBlueprintType.CurveBezierCubic:
				this.groundPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.groundPart.blueprintPart.SetNodePosition(1, nodes[0].GetControlPosition(0));
				this.groundPart.blueprintPart.SetNodePosition(2, nodes[1].GetControlPosition(0));
				this.groundPart.blueprintPart.SetNodePosition(3, nodes[1].GetVertexPosition());
				break;
			case TerrainBlueprintType.CurveCircularArc:
				this.groundPart.blueprintPart.SetNodePosition(0, nodes[0].GetVertexPosition());
				this.groundPart.blueprintPart.SetNodePosition(1, nodes[0].GetControlPosition(0));
				this.groundPart.blueprintPart.SetNodePosition(2, nodes[1].GetVertexPosition());
				break;
			default:
				Debug.LogError("TerrainBlueprintType " + this.groundPart.blueprintPart.GetType() + " does not exist.");
				break;
			}
		}

		// Set segment length
		this.groundPart.blueprintPart.SetSegmentLength(segmentLength);

		// Then, regenerate the terrain
		this.groundPart.Regenerate();

		// Finally, apply the colour
		ApplyColor();
	}
}