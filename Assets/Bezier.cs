using UnityEngine;
using System.Collections;

public class Bezier : MonoBehaviour {
	void Start() {
		
	}
	
	void OnDrawGizmos() {
		
		Vector3 A = new Vector3(0 , 0 , 0 );
		Vector3 B = new Vector3(10, 0 , 0 );
		Vector3 C = new Vector3(10, 40, 0 );
		Vector3 D = new Vector3(20, 40, 0 );
		
		int detail = 3;
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
