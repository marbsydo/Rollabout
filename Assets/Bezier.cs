using UnityEngine;
using System.Collections;

public class Bezier : MonoBehaviour {
	
	Vector3 A = new Vector3(-4, 0, 0);
	Vector3 B = new Vector3(2, 0, 0);
	Vector3 C = new Vector3(4, 8, 0);
	Vector3 D = new Vector3(6, 8, 0);
	
	int detail = 6;
	
	void Start() {
		Vector3[] P = BezierGenerate(A, B, C, D, detail);
		
		CreateSphereAt(P[0]);
		for (int i = 1; i <= detail; i++) {
			CreateBlockBetween(P[i-1], P[i]);
			CreateSphereAt(P[i]);
		}
	}
	
	void CreateSphereAt(Vector3 pos) {
		GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		s.transform.position = pos;
	}
	
	void CreateBlockBetween(Vector3 pos1, Vector3 pos2) {
		GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
		b.transform.position = (pos1 + pos2) / 2;
		
		// Rotate block
		float a = Mathf.Rad2Deg * Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x);
		b.transform.eulerAngles = new Vector3(0, 0, a);
		
		// Change its length
		float d = (pos1 - pos2).magnitude;
		b.transform.localScale = new Vector3(d, 1, 1);
	}
	
	void OnDrawGizmos() {
		Vector3[] P = BezierGenerate(A, B, C, D, detail);
		for (int i = 1; i <= detail; i++)
			Gizmos.DrawLine(P[i-1], P[i]);
		
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(A, 0.1f);
		
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(D, 0.1f);
	}
	
	Vector3[] BezierGenerate(Vector3 A, Vector3 B, Vector3 C, Vector3 D, int detail) {
		Vector3[] P = new Vector3[201];
		
		float a = 1.0f;
		float b = 1.0f - a;
		float d = (1 / (float) detail);
		
		for (int i = 0; i <= detail; i++) {
			P[i] = A*a*a*a + B*3*a*a*b + C*3*a*b*b + D*b*b*b;
			a -= d;
			b = 1.0f - a;
		}
		
		return P;
	}
}
