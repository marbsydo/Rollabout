using UnityEngine;
using System.Collections;

public enum EditorNodeControlRestriction{None, PerpendicularToMiddleOfAC};

public class EditorNode : MonoBehaviour {
	
	const KeyCode inputNodeModify = KeyCode.Mouse0;              // Moving and reshaping a node
	const KeyCode inputNodeSnapGrid = KeyCode.LeftAlt;           // Snapping a node to the grid (overridden by spoke snap)
	const KeyCode inputNodeSnapNode = KeyCode.LeftShift;         // Snapping a node to another node
	const KeyCode inputNodeSnapSpokeAngle = KeyCode.LeftControl; // Snapping a node to a spoke (the angle of the spoke)
	const KeyCode inputNodeSnapSpokeLinear = KeyCode.LeftAlt;    // Snapping a node to a spoke (the distance along the spoke)
	const KeyCode inputNodeSelectIndividual = KeyCode.LeftShift; // Selecting multiple nodes in one go
	const KeyCode inputNodeSegmentsIncrease = KeyCode.X;
	const KeyCode inputNodeSegmentsDecrease = KeyCode.Z;
	const KeyCode inputDelete = KeyCode.Delete;

	Color colorModifyControl = new Color(1, 0.6f, 0.1f, 1);
	Color colorModifyVertex = new Color(0.6f, 0.6f, 0.6f, 1);

	EditorController editorController;
	
	// The object that we should tell when we update
	TerrainPartObject terrainPartObject;
	
	GameObject spriteBezierControl;
	GameObject spriteBezierVertex;
	GameObject spriteBezierLine;
	
	bool handlesHaveBeenCreated = false;
	GameObject nodeVertex;
	GameObject[] nodeControl;
	EditorNodeControlRestriction[] nodeControlRestriction;
	GameObject[] nodeLine;
	int numControls = 3;
	
	// Snapping
	bool snapByDefault = true;
	float snapMinDist = 0.5f;
	
	// grid resolutions
	float gridMainResolution = 2f;  // Resolution of the main grid
	float gridSpokeResolution = 2f; // Resolution of the spoke length

	// For moving multiple nodes
	EditorNode[] additionalNodes;
	int additionalNodesLength;
	const int additionalNodesMax = 3; // Max number of nodes that can be moved together

	// Mouse clicking stuff
	// -2 = nothing
	// -1 = nodeVertex
	//  x = nodeControl[x]
	int mouseHolding = -2;

	// The offset between a node and the mouse when the mouse first selects the node
	Vector3 mousePosOffset;
	
	void Awake() {
		
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>();
		
		spriteBezierControl = Resources.Load("LevelEditor/Sprites/SpriteBezierControl") as GameObject;
		spriteBezierVertex = Resources.Load("LevelEditor/Sprites/SpriteBezierVertex") as GameObject;
		spriteBezierLine = Resources.Load("LevelEditor/Sprites/SpriteBezierLine") as GameObject;
	}
	
	// Should only be called once
	// (unless you feel like writing a function to destroy all handles)
	public void CreateHandles(int numControls) {
		nodeControl = new GameObject[numControls];
		nodeControlRestriction = new EditorNodeControlRestriction[numControls];
		nodeLine = new GameObject[numControls];
		
		this.numControls = numControls;
		
		nodeVertex = GameObject.Instantiate(spriteBezierVertex, transform.position, Quaternion.identity) as GameObject;
		nodeVertex.transform.parent = transform;
		
		for (int i = 0; i < numControls; i++) {
			nodeControlRestriction[i] = EditorNodeControlRestriction.None;
			nodeControl[i] = GameObject.Instantiate(spriteBezierControl, transform.position, Quaternion.identity) as GameObject;
			nodeLine[i] = GameObject.Instantiate(spriteBezierLine, transform.position, Quaternion.identity) as GameObject;
			
			nodeControl[i].transform.parent = transform;
			nodeLine[i].transform.parent = transform;
			
			Vector3 controlOffset;
			
			switch (i) {
			case 0:
				controlOffset = new Vector3(-2, 0, 0);
				break;
			case 1:
				controlOffset = new Vector3(+2, 0, 0);
				break;
			case 2:
				controlOffset = new Vector3(0, -2, 0);
				break;
			case 3:
				controlOffset = new Vector3(0, +2, 0);
				break;
			default:
				Debug.LogError("I'm not sure what position to create this control at.");
				controlOffset = new Vector3(+2, +2, 0);
				break;
			}
			
			nodeControl[i].transform.position = transform.position + controlOffset;
			
			handlesHaveBeenCreated = true;
			
			// UpdateLine requires handlesHaveBeenCreated to be true
			UpdateLine(i);
		}
		
		handlesHaveBeenCreated = true;
	}
	
	public void SetPosition(Vector3 pos) {
		// Keep nodes in the z=-5 plane so they are draw on front of stuff
		pos.z = -5;
		transform.position = pos;
		//MoveVertex(pos);
	}
	
	public void SetTerrainPartObject(TerrainPartObject terrainPartObject) {
		this.terrainPartObject = terrainPartObject;
	}
	
	public void SetControlRestriction(int control, EditorNodeControlRestriction r) {
		nodeControlRestriction[control] = r;
	}
	
	public Vector3 GetVertexPosition() {
		return nodeVertex.transform.position;
	}
	
	public Vector3 GetControlPosition(int control) {
		return nodeControl[control].transform.position;
	}
	
	void Update() {
		if (handlesHaveBeenCreated) {
			UpdateHandles();
		}
	}
	
	void UpdateHandles() {
		if (Input.GetKeyDown(inputNodeModify)) {
			// Clicked somewhere, so if it clicked on a vertex or control
			
			Vector3 mousePos = GetMousePosition(false, Vector3.zero);
			
			// First check if it clicked any of the control points
			for (int i = 0; i < numControls; i++) {
				if ((mousePos - nodeControl[i].transform.position).magnitude < 1) {
					mousePosOffset = nodeControl[i].transform.position - mousePos;
					mouseHolding = i;

					// Highlight terrainPartObject to show it is being modified
					terrainPartObject.SetColor(colorModifyControl);

					break;
				}
			}
			
			// Next check if it clicked the vertex
			if (mouseHolding < 0) {
				if ((mousePos - nodeVertex.transform.position).magnitude < 1) {
					mousePosOffset = nodeVertex.transform.position - mousePos;
					// Did click on the vertex
					mouseHolding = -1;

					// Highlight terrainPartObject to show it is being modified
					terrainPartObject.SetColor(colorModifyVertex);

					// If moving multiple vertices, check for others at same position
					if (!Input.GetKey(inputNodeSelectIndividual)) {

						additionalNodes = new EditorNode[additionalNodesMax];
						additionalNodesLength = 0;

						EditorNode[] nodes = GameObject.FindObjectsOfType(typeof(EditorNode)) as EditorNode[];
						foreach (EditorNode node in nodes) {
							if (node.gameObject.GetInstanceID() != gameObject.GetInstanceID()) { // Don't select ourselves
								Vector3 p1 = node.GetVertexPosition();
								Vector3 p2 = this.GetVertexPosition();//GetMousePosition();
								p1.z = p2.z = 0;
								if ((p1 - p2).magnitude < 0.1f) {
									if (additionalNodesLength < additionalNodesMax) {
										// This other node is at our position, so select it too
										additionalNodes[additionalNodesLength] = node;
										additionalNodesLength++;
									}
								}
							}
						}
					}
				} else {
					// Did not click on the vertex
					mouseHolding = -2;
					additionalNodesLength = 0;
				}
			}
			
			if (mouseHolding > -2) {
				// If we cannot claim the mouse, drop whatever is being moved
				if (!editorController.MouseClaim(gameObject)) {
					mouseHolding = -2;
					additionalNodesLength = 0;

					// If the thing that claimed the mouse is NOT controlling the same object, return its colour to normal
					if (editorController.MouseClaimant().transform.parent.GetInstanceID() != this.transform.parent.GetInstanceID()) {
						terrainPartObject.SetColor(Color.white);
						terrainPartObject.Regenerate();
					}
				}
			}
		}
		
		if (!Input.GetKey(inputNodeModify)) {
			if (mouseHolding > -2) {
				// If release button, drop whatever is being moved
				mouseHolding = -2;
				additionalNodesLength = 0;
				terrainPartObject.SetColor(Color.white);
				terrainPartObject.Regenerate();
				
				editorController.MouseRelease(gameObject);
			}
		}
		
		if (mouseHolding > -2) {
			// Something is being held, so move it
			
			bool snappingToGrid = Input.GetKey(inputNodeSnapGrid);
			bool snappingToSpokeAngle = Input.GetKey(inputNodeSnapSpokeAngle);
			
			Vector3 mousePosWithOffset;
			
			// Do not snap to grid if snapping to spoke
			if (!snappingToSpokeAngle) {
				mousePosWithOffset = GetMousePosition(snappingToGrid, mousePosOffset);
			} else {
				mousePosWithOffset = GetMousePosition(false, mousePosOffset);
			}

			if (Input.GetKeyDown(inputDelete)) {
				Destroy(this.transform.parent.gameObject);

				editorController.MouseRelease(gameObject);
			}

			// Being held, so also update segment length if necessary
			if (Input.GetKeyDown(inputNodeSegmentsIncrease)) {
				terrainPartObject.SegmentLengthIncrease();
			}

			if (Input.GetKeyDown(inputNodeSegmentsDecrease)) {
				terrainPartObject.SegmentLengthDecrease();
			}
			
			// Snap to nearby thingies
			bool snappedToNode = false;
			if (Input.GetKey(inputNodeSnapNode) ^ snapByDefault) {

				// Find all nodes
				EditorNode[] nodes = GameObject.FindObjectsOfType(typeof(EditorNode)) as EditorNode[];
				foreach (EditorNode node in nodes) {
					// Compare GetInstanceID() to ensure we don't snap to ourselves
					if (!snappedToNode && node.gameObject.GetInstanceID() != gameObject.GetInstanceID()) {

						// Compare GetInstanceID() to ensure we don't snap with other nodes we are moving
						bool isOtherNodeWeAreMoving = false;
						for (int i = 0; i < additionalNodesLength; i++) {
							if (additionalNodes[i].gameObject.GetInstanceID() == node.gameObject.GetInstanceID()) {
								isOtherNodeWeAreMoving = true;
							}
						}

						if (!isOtherNodeWeAreMoving) {
							Vector3 p1 = node.GetVertexPosition();
							
							// Make p2 be the center of the vertex
							// simply using this.GetVertexPosition() will not work right
							// because that will result in the vertex getting locked in place once a snap occurs
							Vector3 p2 = GetMousePosition(false, mousePosOffset);
							p1.z = p2.z = 0;
							if ((p1 - p2).magnitude < snapMinDist) {
								snappedToNode = true;
								
								// Snap to thingy
								if (mouseHolding == -1) {
									MoveVertex(p1);

									// If moving multiple nodes, move them too
									for (int i = 0; i < additionalNodesLength; i++) {
										additionalNodes[i].MoveVertex(p1);
										additionalNodes[i].Regenerate();
									}
								} else {
									MoveControl(mouseHolding, p1);
								}
							}
						}
					}
				}
			}
			
			// Snap to spoke
			if (snappingToSpokeAngle) {
				// Find all nodes
				EditorNode[] nodes = this.transform.parent.GetComponentsInChildren<EditorNode>() as EditorNode[];
				if (nodes.Length == 2) {
					// There are exactly two nodes
					Vector3 p2 = mousePosWithOffset;//this.GetVertexPosition();
					Vector3 p1;
					
					if (mouseHolding == -1) {
						// If holding vertex, find the other vertex
						// In this way, we snap relative to the other vertex
						
						// Could be replaced with tertiary thing, but this is easier to read and debug
						if (nodes[0].GetInstanceID() == this.GetInstanceID()) {
							p1 = nodes[1].GetVertexPosition();
						} else {
							p1 = nodes[0].GetVertexPosition();
						}
					} else {
						// If holding control, find our vertex
						// In this way, we snap relative to our own vertex
						p1 = this.GetVertexPosition();
					}
						
					Vector3 diff = p2 - p1; // so p2 = p1 + diff
					
					float biggestAngle = 0f;
					float biggestAngleMagnitude = 0f;
					float step = 15f;
					for (float i = 0; i < 360f; i+= step) {
						Vector3 diffRotated = RotateVector3AroundZ(diff, i * Mathf.Deg2Rad);
						if (Mathf.Abs(diffRotated.x) > biggestAngleMagnitude) {
							biggestAngle = i;
							biggestAngleMagnitude = Mathf.Abs(diffRotated.x);
						}
					}
					
					diff = RotateVector3AroundZ(diff, biggestAngle * Mathf.Deg2Rad);
					diff = new Vector3(diff.x, 0, 0);
					diff = RotateVector3AroundZ(diff, biggestAngle * Mathf.Deg2Rad * -1);
					
					// Snapping spoke linearly
					if (Input.GetKey(inputNodeSnapSpokeLinear)) {
						diff = diff.normalized * Mathf.Round(diff.magnitude / gridSpokeResolution) * gridSpokeResolution;
					}
					
					mousePosWithOffset = p1 + diff;
				}
			}

			// Didn't snap to anything, so move it normally
			if (!snappedToNode) {
				// Move without snapping
				if (mouseHolding == -1) {
					MoveVertex(mousePosWithOffset);

					// If moving multiple nodes, move them too
					for (int i = 0; i < additionalNodesLength; i++) {
						additionalNodes[i].MoveVertex(mousePosWithOffset);
						additionalNodes[i].Regenerate();
					}
				} else {
					MoveControl(mouseHolding, mousePosWithOffset);
				}
			}

			// Now tell our TerrainPartObject to update
			Regenerate();
		}
	}
	
	// Rotate Vector3 v around Z axis by angle a (radians)
	private Vector3 RotateVector3AroundZ(Vector3 v, float a) {
		return new Vector3(v.x * Mathf.Cos(a) - v.y * Mathf.Sin(a), v.x * Mathf.Sin(a) + v.y * Mathf.Cos(a), v.z);
	}

	public void Regenerate() {
			if (terrainPartObject != null)
				terrainPartObject.Regenerate();
	}
	
	public void MoveVertex(Vector3 pos) {
		if (handlesHaveBeenCreated) {
			Vector3 d = pos - nodeVertex.transform.position;
			d.z = 0;
			nodeVertex.transform.position += d;
			for (int i = 0; i < numControls; i++) {
				nodeControl[i].transform.position += d;
				nodeLine[i].transform.position += d;
			}
		}
	}
	
	public void MoveControl(int control, Vector3 pos) {
		if (handlesHaveBeenCreated) {
			nodeControl[control].transform.position = pos;
			
			float nearDis = 1.0f;
			float farDis = 12.0f;
			
			// Ensure control does not come too near to vertex or too far
			Vector3 d = nodeControl[control].transform.position - nodeVertex.transform.position;
			d.z = 0;
			if (d.magnitude < nearDis)
				d = d.normalized * nearDis;
			else if (d.magnitude > farDis)
				d = d.normalized * farDis;
			
			nodeControl[control].transform.position = nodeVertex.transform.position + d;
			
			// Apply restriction
			
			//Todo: Find AC line
			// Find perpendicular line
			// Find closest point on the line
			
			// or perhaps move this node back from the circle?
			
			//Vector3 perp = new Vector3(nodeControl[control].transform.position.x, nodeControl[control].transform.position.y * -1, 0);
			
			UpdateLine(control);
		}
	}
	
	void UpdateLine(int line) {
		if (handlesHaveBeenCreated) {
			// Move line between vertex and node
			
			Vector3 pos1 = nodeVertex.transform.position;
			Vector3 pos2 = nodeControl[line].transform.position;
			
			nodeLine[line].transform.position = (pos1 + pos2) / 2;
			
			// Rotate block
			float a = Mathf.Rad2Deg * Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x);
			nodeLine[line].transform.eulerAngles = new Vector3(0, 0, a);
			
			// Change its length
			float d = (pos1 - pos2).magnitude;
			nodeLine[line].transform.localScale = new Vector3(d, 1, 1);
		}
	}
	
	Vector3 ClosestPointOnLine(Vector3 A, Vector3 B, Vector3 point) {
		Vector3 v1 = point - A;
		Vector3 v2 = (B - A).normalized;
		
		float d = Vector3.Distance(A, B);
		float t = Vector3.Dot(v2, v1);
		
		if (t <= 0)
			return A;
		
		if (t >= d)
			return B;
		
		Vector3 v3 = v2 * t;
		Vector3 vClosest = A + v3;
		
		return vClosest;
	}

	Vector3 GetMousePosition() {
		return GetMousePosition(false, Vector3.zero);
	}

	Vector3 GetMousePosition(bool snapToGrid, Vector3 offsetBeforeSnap) {
		Vector3 m = editorController.GetCamera().ScreenToWorldPoint(Input.mousePosition);
		m.z = transform.position.z;
		m += offsetBeforeSnap;
		
		if (snapToGrid) {
			m.x = Mathf.Round(m.x / gridMainResolution) * gridMainResolution;
			m.y = Mathf.Round(m.y / gridMainResolution) * gridMainResolution;
		}

		return m;
	}
}
