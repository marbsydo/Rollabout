using UnityEngine;
using System.Collections;

public class EditorNode : MonoBehaviour {
	
	EditorController editorController;
	
	// The object that we should tell when we update
	TerrainPartObject terrainPartObject;
	
	GameObject spriteBezierControl;
	GameObject spriteBezierVertex;
	GameObject spriteBezierLine;
	
	bool handlesHaveBeenCreated = false;
	GameObject nodeVertex;
	GameObject[] nodeControl;
	GameObject[] nodeLine;
	int numControls = 3;
	
	
	// Mouse clicking stuff
	// -2 = nothing
	// -1 = nodeVertex
	//  x = nodeControl[x]
	int mouseHolding = -2;
	
	void Awake() {
		
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>();
		
		spriteBezierControl = Resources.Load("LevelEditor/Sprites/SpriteBezierControl") as GameObject;
		spriteBezierVertex = Resources.Load("LevelEditor/Sprites/SpriteBezierVertex") as GameObject;
		spriteBezierLine = Resources.Load("LevelEditor/Sprites/SpriteBezierLine") as GameObject;
		
		nodeControl = new GameObject[numControls];
		nodeLine = new GameObject[numControls];
	}
	
	// Should only be called once
	// (unless you feel like writing a function to destroy all handles)
	public void CreateHandles(int numControls) {
		this.numControls = numControls;
		
		nodeVertex = GameObject.Instantiate(spriteBezierVertex, transform.position, Quaternion.identity) as GameObject;
		nodeVertex.transform.parent = transform;
		
		for (int i = 0; i < numControls; i++) {
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
		transform.position = pos;
		//MoveVertex(pos);
	}
	
	public void SetTerrainPartObject(TerrainPartObject terrainPartObject) {
		this.terrainPartObject = terrainPartObject;
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
		if (Input.GetMouseButtonDown(0)) {
			// Clicked somewhere, so if it clicked on a vertex or control
			
			Vector3 m = editorController.GetCamera().ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			
			// First check if it clicked any of the control points
			for (int i = 0; i < numControls; i++) {
				if ((m - nodeControl[i].transform.position).magnitude < 1) {
					mouseHolding = i;
					break;
				}
			}
			
			// Next check if it clicked the vertex
			if (mouseHolding < 0) {
				if ((m - nodeVertex.transform.position).magnitude < 1) {
					mouseHolding = -1;
				} else {
					mouseHolding = -2;
				}
			}
			
			if (mouseHolding > -2) {
				// If we cannot claim the mouse, do not being holding
				if (!editorController.MouseClaim(gameObject)) {
					mouseHolding = -2;
				}
			}
		}
		
		if (!Input.GetMouseButton(0)) {
			if (mouseHolding > -2) {
				// If release button, drop whatever is being moved
				mouseHolding = -2;
				
				editorController.MouseRelease(gameObject);
			}
		}
		
		if (mouseHolding > -2) {
			// Something is being held, so move it
			
			Vector3 m = editorController.GetCamera().ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			
			if (mouseHolding == -1) {
				//nodeVertex.transform.position = m;
				MoveVertex(m);
			} else {
				MoveControl(mouseHolding, m);
			}
			
			// Now tell our TerrainPartObject to update
			if (terrainPartObject != null)
				terrainPartObject.Regenerate();
		}
	}
	
	void MoveVertex(Vector3 pos) {
		if (handlesHaveBeenCreated) {
			Vector3 d = pos - nodeVertex.transform.position;
			nodeVertex.transform.position += d;
			for (int i = 0; i < numControls; i++) {
				nodeControl[i].transform.position += d;
				nodeLine[i].transform.position += d;
			}
		}
	}
	
	void MoveControl(int control, Vector3 pos) {
		if (handlesHaveBeenCreated) {
			nodeControl[control].transform.position = pos;
			
			float nearDis = 1.0f;
			float farDis = 12.0f;
			
			// Ensure control does not come too near to vertex or too far
			Vector3 d = nodeControl[control].transform.position - nodeVertex.transform.position;
			if (d.magnitude < nearDis)
				d = d.normalized * nearDis;
			else if (d.magnitude > farDis)
				d = d.normalized * farDis;
			
			nodeControl[control].transform.position = nodeVertex.transform.position + d;
			
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
}
