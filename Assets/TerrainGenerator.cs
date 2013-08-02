using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

}

public class TerrainPart {
	public BlueprintPart blueprintPart;
	Transform parent;
	GameObject[] objs;
	int objPos = 0;

	GameObject terrainLine;
	GameObject terrainCircle;
	
	public TerrainPart(BlueprintPart blueprintPart) {
		this.blueprintPart = blueprintPart;

		terrainLine = Resources.Load("Terrain/Sprites/TerrainGrassLine") as GameObject;
		terrainCircle = Resources.Load("Terrain/Sprites/TerrainGrassCircle") as GameObject;
	}
	
	public void SetParent(Transform parent) {
		this.parent = parent;
	}
	
	public void Regenerate() {
		Vector3[] p = blueprintPart.CalculatePoints();
		
		// For (x) fenceposts, we need (x * 2 - 1) fences and fenceposts
		ObjsReset(p.Length * 2 - 1);
		
		// The first sphere is rotated at the angle of the first line
		float sphereAngle = Mathf.Atan2(p[1].y - p[0].y, p[1].x - p[0].x);
		ObjsAppend(CreateSphereAt(p[0], sphereAngle));
		for (int i = 1; i < p.Length; i++) {
			ObjsAppend(CreateBlockBetween(p[i-1], p[i]));

			// The middle spheres are rotated at the average of their adjacent two lines
			if ((i - 1) >= 0 && (i + 1) < p.Length) {
				// Get angle before (a1) and angle after (a2) this sphere
				float a1 = Mathf.Atan2(p[i].y - p[i-1].y, p[i].x - p[i-1].x);
				float a2 = Mathf.Atan2(p[i+1].y - p[i].y, p[i+1].x - p[i].x);

				// Get the average of these two angles
				sphereAngle = Mathf.Atan2(Mathf.Sin(a1) + Mathf.Sin(a2), Mathf.Cos(a1) + Mathf.Cos(a2));
			}

			// The final sphere is rotate at the angle of the final line
			if (i == p.Length - 1) {
				sphereAngle = Mathf.Atan2(p[p.Length - 1].y - p[p.Length - 2].y, p[p.Length - 1].x - p[p.Length - 2].x);
			}

			ObjsAppend(CreateSphereAt(p[i], sphereAngle));
		}
	}
	
	void ObjsReset(int size) {
		ObjsDestroy();
		objs = new GameObject[size];
	}
	
	void ObjsAppend(GameObject obj) {
		objs[objPos] = obj;
		objPos++;
	}
	
	void ObjsDestroy() {
		if (objs != null) {
			for (int i = 0; i < objs.Length; i++) {
				GameObject.Destroy(objs[i]);
			}
		}
		objs = null;
		objPos = 0;
	}
	
	GameObject CreateSphereAt(Vector3 pos, float angle) {
		GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		// Sprite
		GameObject.Destroy(s.GetComponent<MeshRenderer>());
		GameObject sprite = (GameObject.Instantiate(terrainCircle, Vector3.zero, Quaternion.identity) as GameObject);
		sprite.transform.parent = s.transform;
		sprite.transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
		sprite.transform.position = new Vector3(0, 0, 0.5f);

		s.transform.position = pos;
		
		if (parent != null)
			s.transform.parent = parent;
		
		return s;
	}
	
	GameObject CreateBlockBetween(Vector3 pos1, Vector3 pos2) {
		GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);

		// Sprite
		GameObject.Destroy(b.GetComponent<MeshRenderer>());
		GameObject sprite = (GameObject.Instantiate(terrainLine, Vector3.zero, Quaternion.identity) as GameObject);
		sprite.transform.parent = b.transform;

		b.transform.position = (pos1 + pos2) / 2;
		
		// Rotate block
		float a = Mathf.Rad2Deg * Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x);
		b.transform.eulerAngles = new Vector3(0, 0, a);
		
		// Change its length
		float d = (pos1 - pos2).magnitude;
		b.transform.localScale = new Vector3(d, 1, 1);

		// Set x scale of sprite to 1 / 4 where 4 is the width of the line
		//TODO: Make it automatically get the correct line length from the blueprint, rather than just using 4
		sprite.transform.localScale = new Vector3(1f / 4f, 1, 1);
		
		if (parent != null)
			b.transform.parent = parent;
		
		return b;
	}
}

public enum BlueprintPartType {StraightLine, CurveBezierCubic, CurveCircularArc}

abstract public class BlueprintPart {
	protected BlueprintPartType type;
	protected Vector3[] p;

	public BlueprintPartType GetPartType() {
		return type;
	}

	abstract public int GetNodeAmount();
	abstract public void SetSegmentLength(float segmentLength);
	abstract public Vector3[] CalculatePoints();

	public void SetNodePositions(Vector3[] v) {
		if (v.Length == GetNodeAmount()) {
			p = new Vector3[GetNodeAmount()];
			for (int i = 0; i < GetNodeAmount(); i++) {
				SetNodePosition(i, v[i]);
			}
		} else {
			Debug.LogWarning("Incorrect number of points supplied. Required [" + GetNodeAmount() + "] instead of [" + v.Length + "]");
		}
	}

	public Vector3 GetNodePosition(int node) {
		if (node >= 0 && node <= this.GetNodeAmount()) {
			return this.p[node];
		} else {
			Debug.LogWarning("There is no GetNodePosition() for node " + node);
			return Vector3.zero;
		}
	}

	public void SetNodePosition(int node, Vector3 pos) {
		if (node >= 0 && node <= this.GetNodeAmount()) {
			pos.z = 0;
			this.p[node] = pos;
		} else {
			Debug.LogWarning("There is no SetNodePosition() for node " + node);
		}
	}
}

public class StraightLine : BlueprintPart {
	const int nodeAmount = 2;
	float segmentLength = 4f;

	// # Constructor
	
	public StraightLine() {
		type = BlueprintPartType.StraightLine;
	}

	// ## Meta

	override public int GetNodeAmount() {
		return nodeAmount;
	}

	// ## Segments

	override public void SetSegmentLength(float segmentLength) {
		this.segmentLength = segmentLength;
	}

	// ## Calculating

	override public Vector3[] CalculatePoints() {
		Vector3 A = this.GetNodePosition(0);
		Vector3 B = this.GetNodePosition(1);
		Vector3 d = (B - A);
		int segments = (int) (d.magnitude / segmentLength);
		if (segments < 1)
			segments = 1;

		Vector3[] P = new Vector3[segments + 1];
		for (int i = 0; i <= segments; i++) {
			P[i] = A + (d / segments) * i;
		}

		return P;
	}
	
	public Vector3 CalculateLinePoint(float a) {
		return p[0] + (p[1] - p[0]) * a;
	}
}

public class CurveBezierCubic : BlueprintPart {
	const int nodeAmount = 4;
	private int segments = 1;
	
	// ## Constructor
	
	public CurveBezierCubic() {
		type = BlueprintPartType.CurveBezierCubic;
	}

	// ## Meta

	override public int GetNodeAmount() {
		return nodeAmount;
	}

	// ## Segments
	
	public void SetSegmentNum(int segments) {
		if (segments >= 1) {
			this.segments = segments;
		} else {
			this.segments = 1;
			Debug.LogWarning("Attempted to assign an invalid number of segments to BezierCubic");
		}
	}
	
	override public void SetSegmentLength(float segmentLength) {
		// Calculates how many segments are required based upon desired length
		int segments = (int) (CalculateLength() / segmentLength);
		if (segments < 1)
			segments = 1;
		SetSegmentNum(segments);
	}

	// ## Calculating
	
	override public Vector3[] CalculatePoints() {
		Vector3[] P = new Vector3[segments + 1];
		
		float a = 1.0f;
		float d = (1 / (float) segments);
		
		for (int i = 0; i <= segments; i++) {
			P[i] = CalculateCurvePoint(a);
			a -= d;
		}
		
		return P;
	}
	
	public Vector3 CalculateCurvePoint(float a) {
		float b = 1.0f - a;
		float a2 = a*a;
		float b2 = b*b;
		return p[0]*a2*a + p[1]*3*a2*b + p[2]*3*a*b2 + p[3]*b2*b;
	}
	
	public float CalculateLength() {
		// Calculate length using 100 segments for a good length estimate
		return CalculateLength(100);
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

public class CurveCircularArc : BlueprintPart {
	const int nodeAmount = 3;
	private float segmentLength = 2f; // Number of segments is calculated based upon segmentLength during CalculatePoints()

	// These values are set by CalculateDiameter() and CalculateMiddle()
	private float calculatedDiameter;
	private Vector3 calculatedMiddle;

	// These values are set by CalculateArcType()
	private bool calculatedArcExists;      // True = an arc exists (will be false if diameter is too large)
	private bool calculatedArcIsReflex;    // True = reflex angle, false = non-reflex angle
	private bool calculatedArcIsClockwise; // True = clockwise, false = anti-clockwise

	// These values are set by CalculateAngles()
	private float calculatedAngleStart;
	private float calculatedAngleThrough; // NOTE: Pass this through AngleToggleReflexAndDirection() for the real angle

	// This value are set by CalculateLength()
	private float calculatedLength;
	
	// ## Constructor

	public CurveCircularArc() {
		type = BlueprintPartType.CurveCircularArc;
	}

	// ## Meta

	override public int GetNodeAmount() {
		return nodeAmount;
	}

	// ## Segments

	override public void SetSegmentLength(float segmentLength) {
		this.segmentLength = segmentLength;
	}

	// ## Calculating
	
	public void CalculateDiameter() {
		// Diameter = length of side / sine of opposite angle
		float l = (p[0] - p[1]).magnitude;
		
		float a0 = Mathf.Atan2(p[0].y - p[2].y, p[0].x - p[2].x);
		float a1 = Mathf.Atan2(p[1].y - p[2].y, p[1].x - p[2].x);
		
		float s = Mathf.Sin(a0 - a1);
		
		this.calculatedDiameter = l / s;
	}
	
	public void CalculateMiddle() {
		// Calculate center relative to A (p[0]):
		//Bd = B - A
		//Cd = C - A
		//Dd = 2(Bdx * Cdy - Bdy * Cdx)
		//Ux = (Cdy(Bdx2 + Bdy2) - Bdy(Cdx2 + Cdy2)) / Dd
		//Uy = (Bdx(Cdx2 + Cdy2) - Cdx(Bdx2 + Bdy2)) / Dd
		
		Vector3 Bd = p[1] - p[0];
		Vector3 Cd = p[2] - p[0];
		float Dd = 2 * (Bd.x * Cd.y - Bd.y * Cd.x);
		float Ux = (Cd.y * (Bd.x * Bd.x + Bd.y * Bd.y) - Bd.y * (Cd.x * Cd.x + Cd.y * Cd.y)) / Dd;
		float Uy = (Bd.x * (Cd.x * Cd.x + Cd.y * Cd.y) - Cd.x * (Bd.x * Bd.x + Bd.y * Bd.y)) / Dd;
		
		// Center was calculated relative to A (p[0]) so add it back
		this.calculatedMiddle = p[0] + new Vector3(Ux, Uy, 0);
	}

	public void CalculateAngles() {
		// Relies upon result from CalculateMiddle()

		// Calculates:
		// angleStart
		// angleThrough

		Vector3 m = calculatedMiddle;

		float angleToA = Mathf.Atan2(p[0].y - m.y, p[0].x - m.x);
		//float angleToB = Mathf.Atan2(p[1].y - m.y, p[1].x - m.x);
		float angleToC = Mathf.Atan2(p[2].y - m.y, p[2].x - m.x);

		this.calculatedAngleStart = angleToA;
		this.calculatedAngleThrough = ShortAngleBetweenAngles(angleToA, angleToC);
	}

	public void CalculateLength() {
		// Relies upon result from CalculateAngles(), CalculateDiameter() and CalculateArcType()

		float angleThrough = AngleToggleReflexAndDirection(this.calculatedAngleThrough, this.calculatedArcIsReflex, this.calculatedArcIsClockwise);

		// Length = angle (in radians) * radius
		this.calculatedLength = Mathf.Abs(angleThrough * this.calculatedDiameter / 2);
	}
	
	public void CalculateArcType() {
		// This function finds the correct pair of values for variables `flipAngle` and `flipDirection`
		// through brute force. This is done by generatating a high detailed curve for all 4 combinations
		// and finding which one is the correct arc.

		// The maximum arc diameter allowed. Any arc above this simply becomes a straight line
		const float maxDiameter = 200f;

		// The number of segments used for the arc when brute forcing the solution
		const int bruteForceCurveSegments = 100;

		// Ensure internal calculates are up to date
		CalculateDiameter();
		CalculateMiddle();
		CalculateAngles();

		bool flipAngle = false;     // True if angle is reflex, false if angle is not reflex (obtuse, right-angle, acute)
		bool flipDirection = false; // True if angle is clockwise, false if angle is anti-clockwise

		bool foundCorrectBools = false;
		if (Mathf.Abs(calculatedDiameter) < maxDiameter) {
			// First, calculate the boolean arguments for the curve through brute force
			int tempNum = bruteForceCurveSegments;
			Vector3[] temp = new Vector3[tempNum];
			foundCorrectBools = false;
			bool endsAtEndPoint;
			bool intersectsMidPoint;
			for (int j = 0; j < 4 && !foundCorrectBools; j++) {

				switch (j) {
					case 0:
					flipAngle = false;
					flipDirection = false;
					break;
					case 1:
					flipAngle = true;
					flipDirection = false;
					break;
					case 2:
					flipAngle = false;
					flipDirection = true;
					break;
					case 3:
					flipAngle = true;
					flipDirection = true;
					break;
				}

				intersectsMidPoint = false;

				for (int i = 0; i < tempNum; i++) {
					temp[i] = CalculateCurvePointRaw((float) i / (float) (tempNum - 1), flipAngle, flipDirection);

					if (!intersectsMidPoint)
						if (PointNearPoint(temp[i], p[1], 0.5f))
							intersectsMidPoint = true;
				}

				endsAtEndPoint = PointNearPoint(temp[tempNum - 1], p[2], 0.01f);

				if (endsAtEndPoint && intersectsMidPoint) {
					foundCorrectBools = true;
				}
			}
		} else {
			// Diameter was too big, so do not bother calculating
			//Debug.LogWarning("Diameter was too big");
			foundCorrectBools = false;
		}

		this.calculatedArcExists = foundCorrectBools;
		this.calculatedArcIsReflex = flipAngle;
		this.calculatedArcIsClockwise = flipDirection;
	}

	override public Vector3[] CalculatePoints() {
		CalculateArcType();
		CalculateLength();

		Vector3[] points;

		if (this.calculatedArcExists) {
			// Calculate number of segments based upon desired segmentLength
			int num = (int) (this.calculatedLength / segmentLength);

			// Must be at least 2 points
			if (num < 2)
				num = 2;
			points = new Vector3[num];
			
			//Debug.Log((flipAngle ? "Angle is reflex" : "Angle not reflex") + (flipDirection ? " and clockwise." : " and anti-clockwise."));

			for (int i = 0; i < num; i ++) {
				points[i] = CalculateCurvePointRaw((float) i / (float) (num-1), this.calculatedArcIsReflex, this.calculatedArcIsClockwise);
			}
		} else {
			// No fitting arc could be found, so just generate a straight line
			points = new Vector3[2];
			points[0] = p[0];
			points[0].z = 0;
			points[1] = p[2];
			points[1].z = 0;
		}
			
		return points;
	}
	
	Vector3 CalculateCurvePointRaw(float a, bool flipAngle, bool flipDirection) {
		// Relies upon result from CalculateAngles()

		float r = Mathf.Abs(calculatedDiameter) / 2;

		float d = this.calculatedAngleStart + AngleToggleReflexAndDirection(this.calculatedAngleThrough, flipAngle, flipDirection) * a;

		Vector3 P = Vector3.zero;
		P.x = this.calculatedMiddle.x + (r * Mathf.Cos(d));
		P.y = this.calculatedMiddle.y + (r * Mathf.Sin(d));
		
		return P;
	}

	// ## Miscellaneous calculations

	float AngleToggleReflexAndDirection(float a, bool flipAngle, bool flipDirection) {
		// The ordering is important. AngleToggleReflex() must come before AngleToggleDirection()
		return AngleToggleDirection(AngleToggleReflex(a, flipAngle), flipDirection);
	}

	float AngleToggleReflex(float a, bool flipAngle) {
		// If flipAngle is true, angle a is subtracted from 360 degrees
		// This makes a non-reflex angle become reflex
		return flipAngle ? (360 * Mathf.Deg2Rad) - a : a;
	}

	float AngleToggleDirection(float a, bool flipDirection) {
		// If flipDirection is true, the angle is made negative
		// This makes a clockwise angle become anti-clockwise, or vice versa
		return flipDirection ? -a : a;
	}

	float ShortAngleBetweenAngles(float a1, float a2) {
		float b = (Mathf.Abs(a1 - a2)) % (360 * Mathf.Deg2Rad);

		if (b > (180 * Mathf.Deg2Rad)) {
			b = (360 * Mathf.Deg2Rad) - b;
		}

		return b;
	}

	// Returns true if two points are very near
	bool PointNearPoint(Vector3 a, Vector3 b, float precision) {
		a.z = 0;
		b.z = 0;
		return ((a - b).magnitude < precision);
	}
}