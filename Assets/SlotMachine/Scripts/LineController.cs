using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;
using System;

public class Line : IDisposable
{
	public float LineWidth { get; set; }
	public Color LineColor { get; set; }
	public List<Cell> Cells { get; set; }
	public bool IsEnabled { get; private set; }

	private VectorLine _line;

	public Line(float lineWidth, Color lineColor, List<Cell> cells)
	{
		this.LineWidth = lineWidth;
		this.LineColor = lineColor;
		this.Cells = new List<Cell>();
		this.Cells.AddRange(cells);
		this.IsEnabled = false;
	}

	public void DrawLine()
	{
		IsEnabled = true;

		// Cleanup old Line
		VectorLine.Destroy(ref _line);
		
		// Create new Line
		_line = new VectorLine("Line", new List<Vector2>(), null, LineWidth, LineType.Continuous, Joins.Weld);
		_line.color = LineColor;

		// Draw all segments and connection lines
		foreach(var cell in Cells)
        {
            _line.points2.Add(cell.Center);
        }
        
        _line.Draw();
	}

	public void HideLine()
	{
		IsEnabled = false;

		// Cleanup Line
		VectorLine.Destroy(ref _line);
	}

	#region IDisposable implementation
	public void Dispose ()
	{
		// Hide and destroy line
		HideLine();
		// Clear dependencies on cells
		Cells.Clear();
	}
	#endregion
}

[RequireComponent(typeof(CombinationsController))]
public class LineController : MonoBehaviour
{
	List<Line> mainLines;
	CombinationsController combinator;

	void Start ()
	{
		combinator = GetComponent<CombinationsController>();
	}

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			BuildCombinations(combinator.GetRandomCombinations());
		}
		if (GUILayout.Button("Hide line"))
		{
			DisposeLines();
        }
	}

	void BuildCombinations(List<CombinationOfCells> combinations)
	{
		DisposeLines();
		mainLines = new List<Line>();
		
		foreach (CombinationOfCells combination in combinations)
		{
			Line line = BuildLine(combination.cells, combination.lineWidth, combination.lineColor);
			mainLines.Add(line);
		}
	}

	Line BuildLine(List<Cell> inputCells, float width, Color color)
	{
		Line line = new Line(width, color, inputCells);
		line.DrawLine();

		return line;
	}

	void DisposeLines()
	{
		if (mainLines != null)
		{
			foreach (Line line in mainLines)
			{
				line.Dispose();
			}
		}
	}
}
