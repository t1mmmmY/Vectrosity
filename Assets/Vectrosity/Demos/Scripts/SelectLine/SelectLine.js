#pragma strict
import Vectrosity;

var lineThickness = 10.0;
var extraThickness = 2;
private var line : VectorLine;
private var wasSelected = false;
private var index = 0;

function Start () {
	line = new VectorLine("SelectLine", new Vector2[10], null, lineThickness, LineType.Continuous, Joins.Fill);
	SetPoints();
}

function SetPoints () {
	for (var i = 0; i < line.points2.Count; i++) {
		line.points2[i] = Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height-20));
	}
	line.Draw();
}

function Update () {
	if (line.Selected (Input.mousePosition, extraThickness, index)) {
		if (!wasSelected) {	// We use wasSelected to update the line color only when needed, instead of every frame
			line.SetColor (Color.green);
			wasSelected = true;
		}
		if (Input.GetMouseButtonDown(0)) {
			SetPoints();
		}
	}
	else {
		if (wasSelected) {
			wasSelected = false;
			line.SetColor (Color.white);
		}
	}
}

function OnGUI () {
	GUI.Label (Rect(10, 10, 800, 30), "Click the line to make a new line");
}