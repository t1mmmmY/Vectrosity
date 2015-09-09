// The DrawLinesTouch script adapted to work with mouse input
#pragma strict
import Vectrosity;
import System.Collections.Generic;

var lineMaterial : Material;
var maxPoints = 5000;
var lineWidth = 4.0;
var minPixelMove = 5;	// Must move at least this many pixels per sample for a new segment to be recorded
var useEndCap = false;
var capTex : Texture2D;
var capMaterial : Material;

private var line : VectorLine;
private var previousPosition : Vector2;
private var sqrMinPixelMove : int;
private var canDraw = false;

function Start () {
	if (useEndCap) {
		VectorLine.SetEndCap ("cap", EndCap.Mirror, capMaterial, capTex);
		lineMaterial = capMaterial;
	}
	
	line = new VectorLine("DrawnLine", new List.<Vector2>(), lineMaterial, lineWidth, LineType.Continuous, Joins.Weld);
	line.endPointsUpdate = 1;	// Optimization for updating only the last point of the line, and the rest is not re-computed
	if (useEndCap) {
		line.endCap = "cap";
	}
	// Used for .sqrMagnitude, which is faster than .magnitude
	sqrMinPixelMove = minPixelMove*minPixelMove;
}

function Update () {
	var mousePos = Input.mousePosition;
	if (Input.GetMouseButtonDown(0)) {
		line.points2.Clear();
		line.Draw();
		previousPosition = mousePos;
		line.points2.Add (mousePos);
		canDraw = true;
	}
	else if (Input.GetMouseButton(0) && (mousePos - previousPosition).sqrMagnitude > sqrMinPixelMove && canDraw) {
		previousPosition = mousePos;
		line.points2.Add (mousePos);
		if (line.points2.Count >= maxPoints) {
			canDraw = false;
		}
		line.Draw();
	}
}