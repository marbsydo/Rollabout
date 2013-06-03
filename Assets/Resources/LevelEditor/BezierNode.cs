using UnityEngine;
using System.Collections;

public class BezierNode : MonoBehaviour {
	
	EditorController editorController;
	
	GameObject spriteBezierControl;
	GameObject spriteBezierVertex;
	GameObject spriteBezierLine;
	
	GameObject nodeVertex;
	GameObject[] nodeControl = new GameObject[2];
	GameObject[] nodeLine = new GameObject[2];
	
	// Mouse clicking stuff
	// 0 = nothing
	// 1 = nodeVertex
	// 2 = nodeControl[0]
	// 3 = nodeControl[1];
	int mouseHolding = 0;
	
	void Start() {
		
		editorController = (GameObject.Find("EditorController") as GameObject).GetComponent<EditorController>();
		
		spriteBezierControl = Resources.Load("LevelEditor/Sprites/SpriteBezierControl") as GameObject;
		spriteBezierVertex = Resources.Load("LevelEditor/Sprites/SpriteBezierVertex") as GameObject;
		spriteBezierLine = Resources.Load("LevelEditor/Sprites/SpriteBezierLine") as GameObject;
		
		// Create one control, two vertices and two lines
		nodeVertex = GameObject.Instantiate(spriteBezierVertex, transform.position, Quaternion.identity) as GameObject;
		nodeControl[0] = GameObject.Instantiate(spriteBezierControl, transform.position, Quaternion.identity) as GameObject;
		nodeControl[1] = GameObject.Instantiate(spriteBezierControl, transform.position, Quaternion.identity) as GameObject;
		nodeLine[0] = GameObject.Instantiate(spriteBezierLine, transform.position, Quaternion.identity) as GameObject;
		nodeLine[1] = GameObject.Instantiate(spriteBezierLine, transform.position, Quaternion.identity) as GameObject;
		
		nodeControl[0].transform.position = transform.position - new Vector3(2, 0, 0);
		nodeControl[1].transform.position = transform.position + new Vector3(2, 0, 0);
		
		UpdateLine(0);
		UpdateLine(1);
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			// Clicked somewhere, so if it clicked on a vertex or control
			
			Vector3 m = editorController.GetCamera().ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			
			if ((m - nodeVertex.transform.position).magnitude < 1) {
				// Clicked on vertex
				mouseHolding = 1;
			} else if ((m - nodeControl[0].transform.position).magnitude < 1) {
				// Clicked on control 0
				mouseHolding = 2;
			} else if ((m - nodeControl[1].transform.position).magnitude < 1) {
				// Clicked on control 1
				mouseHolding = 3;
			} else {
				mouseHolding = 0;
			}
		}
		
		if (!Input.GetMouseButton(0)) {
			// If release button, drop whatever is being moved
			mouseHolding = 0;
		}
		
		if (mouseHolding > 0) {
			// Something is being held, so move it
			
			Vector3 m = editorController.GetCamera().ScreenToWorldPoint(Input.mousePosition);
			m.z = 0;
			
			if (mouseHolding == 1) {
				//nodeVertex.transform.position = m;
				MoveVertex(m);
			} else {
				MoveControl(mouseHolding == 2 ? 0 : 1, m);
			}
		}
	}
	
	void MoveVertex(Vector3 pos) {
		Vector3 d = pos - nodeVertex.transform.position;
		nodeVertex.transform.position += d;
		nodeControl[0].transform.position += d;
		nodeControl[1].transform.position += d;
		nodeLine[0].transform.position += d;
		nodeLine[1].transform.position += d;
	}
	
	void MoveControl(int control, Vector3 pos) {
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
	
	void UpdateLine(int line) {
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
