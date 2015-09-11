using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera camera;
	[SerializeField] Rect testRect;

	UIPanel panel;
	VectorLine line;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		line = new VectorLine("Grid", new List<Vector3>(), null, 3f, LineType.Discrete, Joins.Weld);	
		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;
		DrawRect();
	}

	void DrawRect()
	{
		line.MakeRect(testRect);
	}
}
