using UnityEngine;
using System.Collections;

public abstract class TerrainBase : MonoBehaviour {

	protected bool requireNodes;
	protected ObjectNode[] nodes;
	private Color partColor = Color.white;

	public float segmentLength = 1f;
	public bool physicsEnabled;
	public BlueprintPart blueprintPart;

	// Init() is used instead of a constructor because parameters cannot be passed through AddComponent<>()
	// and this class is created in the CreateTerrain() function of class TerrainObjectMaker using AddComponent<>()
	// It is necessary that Init() is called before any other functions in this class
	public void Init(bool requireNodes, bool physicsEnabled) {
		// If set to true, nodes will be created when AssignBlueprint() is called
		// If set to false, nodes will not be created
		this.requireNodes = requireNodes;
		this.physicsEnabled = physicsEnabled;
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

	protected void ApplyColor() {
		exSprite[] sprites = GetComponentsInChildren<exSprite>();
		foreach (exSprite sprite in sprites) {
			sprite.color = partColor;
		}
	}

	protected ObjectNode CreateNode(Vector3 pos, int numControls) {
		GameObject g = new GameObject();
		g.name = "Node";
		ObjectNode node = g.AddComponent<ObjectNode>();
		node.SetPosition(pos);
		node.CreateHandles(numControls);
		node.SetTerrain(this);

		// Make the node be a child of the terrain object
		g.transform.parent = this.transform;

		return node;
	}

	public abstract void AssignBlueprint(BlueprintPart blueprintPart);
	public abstract void Regenerate();
}
