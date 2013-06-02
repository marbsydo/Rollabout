using UnityEngine;
using System.Collections;

public class BezierGenerator : MonoBehaviour {
	
	public Vector3[] BezierGenerate(BezierCubic bezier) {
		Vector3[] P = new Vector3[bezier.GetSegmentNum() + 1];
		
		float a = 1.0f;
		float d = (1 / (float) bezier.GetSegmentNum());
		
		for (int i = 0; i <= bezier.GetSegmentNum(); i++) {
			P[i] = bezier.CalculateCurvePoint(a);
			a -= d;
		}
		
		return P;
	}
	
	public void BezierPlatformGenerate(BezierCubic bezier) {
		Vector3[] P = BezierGenerate(bezier);
		
		CreateSphereAt(P[0]);
		for (int i = 1; i <= bezier.GetSegmentNum(); i++) {
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
}

/*
# BezierCubic
Contains data structure for storing the 4 points a cubic bezier requires,
and also how many segments the resulting curve should have. There is also a
function to adjust the number of segments according to how long you desire
each segment to be.

## Constructor
Requires the four points (A, B, C and D) for a cubic bezier. Also requires the
number of segments the resulting curve should have.

## Setting
After creation, each individual point (A, B, C and D) can be adjusted, using
the following functions:
* SetPointA()
* SetPointB()
* SetPointC()
* SetPointD()
The number of segments can be adjusted, according to how many you desire in
total, or how long you wish each segment to be (matched best as possible)
using the following functions:
* SetSegmentNum()
* SetSegmentLength()

## Getting
All the cubic curve data can be read using the following functions:
* GetPointA()
* GetPointB()
* GetPointC()
* GetPointD()
* GetSegmentNum()

## Calculating
Some more information can be calculated. These functions should be called
sparingly because the result has to be generated each time:
* CalculateCurvePoint()
* CalculateLength()
*/
public class BezierCubic {
	private Vector3[] p = new Vector3[4];
	private int segments = 1;
	
	// # Constructor
	
	public BezierCubic(Vector3 A, Vector3 B, Vector3 C, Vector3 D, int segments) {
		SetPointA(A);
		SetPointB(B);
		SetPointC(C);
		SetPointD(D);
		SetSegmentNum(segments);
	}
	
	// ## Setting
	
	public void SetPointA(Vector3 A) {
		this.p[0] = A;
	}
	
	public void SetPointB(Vector3 B) {
		this.p[1] = B;
	}
	
	public void SetPointC(Vector3 C) {
		this.p[2] = C;
	}
	
	public void SetPointD(Vector3 D) {
		this.p[3] = D;
	}
	
	public void SetSegmentNum(int segments) {
		if (segments >= 1) {
			this.segments = segments;
		} else {
			this.segments = 1;
			Debug.LogWarning("Attempted to assign an invalid number of segments to BezierCubic");
		}
	}
	
	public void SetSegmentLength(int segmentLength) {
		// Calculates how many segments are required based upon desired length
		int segments = (int) (CalculateLength() / segmentLength);
		if (segments < 1)
			segments = 1;
		SetSegmentNum(segments);
	}
	
	// ## Getting
	
	public Vector3 GetPointA() {
		return this.p[0];
	}
	
	public Vector3 GetPointB() {
		return this.p[1];
	}
	
	public Vector3 GetPointC() {
		return this.p[2];
	}
	
	public Vector3 GetPointD() {
		return this.p[3];
	}
	
	public int GetSegmentNum() {
		return segments;
	}
	
	// ## Calculating
	
	public Vector3 CalculateCurvePoint(float a) {
		float b = 1.0f - a;
		float a2 = a*a;
		float b2 = b*b;
		return p[0]*a2*a + p[1]*3*a2*b + p[2]*3*a*b2 + p[3]*b2*b;
	}
	
	public float CalculateLength() {
		return CalculateLength(segments);
	}
	
	public float CalculateLength(int segmentsToMeasure) {
		float a = 1.0f;
		float d = (1 / (float) segmentsToMeasure);
		
		// Get all points
		Vector3[] P = new Vector3[segmentsToMeasure + 1];
		for (int i = 0; i < segmentsToMeasure; i++) {
			P[i] = CalculateCurvePoint(a);
			a -= d;
		}
		
		// Mesure their length
		float l = 0;
		float x, y;
		for (int i = 1; i < segmentsToMeasure; i++) {
			x = P[i - 1].x - P[i].x;
			y = P[i - 1].y - P[i].y;
			l += Mathf.Sqrt(x * x + y * y);
		}
		
		return l;
	}
}