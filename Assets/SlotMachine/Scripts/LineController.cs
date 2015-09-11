using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

[RequireComponent(typeof(VectrosityGridview))]
public class LineController : MonoBehaviour
{
	public int lineWidth = 5;
	public int maxLineCount = 10;
	public bool smoothLine = false;

	Queue<VectorLine> lineQueue = new Queue<VectorLine>();
	VectrosityGridview grid;
	int count = 0;

	void Start ()
	{
		grid = GetComponent<VectrosityGridview>();
	}

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			var randomCells = grid.GetRandomCells();
			BuildLine(randomCells);
		}
	}

	void BuildLine(List<Cell> inputCells)
	{
		// We have reached max count, so delete first Line
		if (lineQueue.Count == maxLineCount)
		{
			var firstLine = lineQueue.Dequeue();
			VectorLine.Destroy(ref firstLine);
		}

		var line = new VectorLine("Line " + count, new List<Vector2>(), null, lineWidth, LineType.Continuous, Joins.Weld);
		line.color = new Color(Random.value, Random.value, Random.value);

		if (smoothLine)
		{
			line.Resize(100);
			line.MakeSpline(inputCells.Select(i => i.Center).ToArray());
		}
		else
		{
			// Draw all segments and connection lines
			foreach(var cell in inputCells)
			{
				line.points2.Add(cell.Center);
			}
		}

		//Update and draw Line
		line.Draw();

		// Push Line into queue
		lineQueue.Enqueue(line);
	}
}
