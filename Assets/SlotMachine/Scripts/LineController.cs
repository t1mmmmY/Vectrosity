using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

[RequireComponent(typeof(VectrosityGridview))]
public class LineController : MonoBehaviour
{
	public int lineWidth = 5;
	public Color lineColor = Color.red;
	public bool smoothLine = false;

	VectrosityGridview grid;
	VectorLine mainLine;

	void Start ()
	{
		grid = GetComponent<VectrosityGridview>();
	}

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			BuildLine(grid.GetRandomCells());
		}
		if (GUILayout.Button("Hide line"))
		{
			VectorLine.Destroy(ref mainLine);
        }
	}

	void BuildLine(List<Cell> inputCells)
	{
		// Cleanup old Line
		VectorLine.Destroy(ref mainLine);

		// Create new Line
		mainLine = new VectorLine("Line", new List<Vector2>(), null, lineWidth, LineType.Continuous, Joins.Weld);
		mainLine.color = lineColor;

		if (smoothLine)
		{
			mainLine.Resize(100);
			mainLine.MakeSpline(inputCells.Select(i => i.Center).ToArray());
		}
		else
		{
			// Draw all segments and connection lines
			foreach(var cell in inputCells)
			{
				mainLine.points2.Add(cell.Center);
			}
		}

		mainLine.Draw();
	}
}
