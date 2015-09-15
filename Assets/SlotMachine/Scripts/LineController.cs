using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;
using System;

public enum SideType
{
	Top,
	Right,
	Bottom,
	Left,
	None
}

public class MathLine
{
	public Vector3 Coefficients { get; set; }
	
	public MathLine CalculateLineByPointAndVector(Vector2 point, Vector2 vector)
	{
		float a = vector.y;
		float b = - vector.x;
		float c = - point.x * vector.y + point.y * vector.x;
		
		this.Coefficients = new Vector3(a, b, c);
		return this;
	}
	
	public MathLine CalculateLineByPoints(Vector2 point1, Vector2 point2)
	{
		float a = point1.y - point2.y;
		float b = point2.x - point1.x;
		float c = point1.x * point2.y - point2.x * point1.y;
		
		this.Coefficients = new Vector3(a, b, c);
		return this;
	}
	
	public Vector2 GetIntersectionPoint(Vector3 lineCoeffs)
	{
		float denominator = Coefficients.x * lineCoeffs.y - Coefficients.y * lineCoeffs.x;
		
		//No intersection
		if (denominator == 0)
		{
			return new Vector2(Mathf.Infinity, Mathf.Infinity);
		}
		
		Vector2 result;
		result.x = - (Coefficients.z * lineCoeffs.y - lineCoeffs.z * Coefficients.y) / denominator;
		result.y = - (Coefficients.x * lineCoeffs.z - lineCoeffs.x * Coefficients.z) / denominator;
		
		return result;
	}
}

public static class CellExtention
{
	public static Dictionary<SideType, MathLine> GetCellSides(this Cell cell, float offset = 0f)
	{
		Dictionary<SideType, MathLine> result = new Dictionary<SideType, MathLine>();
		//Go through each side
		Rect paddingRect = cell.GetPaddinRectPosition();
		Vector2 LeftTopPoint = new Vector2(paddingRect.xMin - offset, paddingRect.yMax + offset);
		Vector2 RightTopPoint = new Vector2(paddingRect.xMax + offset, paddingRect.yMax + offset);
		Vector2 RightBottomPoint = new Vector2(paddingRect.xMax + offset, paddingRect.yMin - offset);
		Vector2 LeftBottomPoint = new Vector2(paddingRect.xMin - offset, paddingRect.yMin - offset);

		result[SideType.Top] = new MathLine().CalculateLineByPoints(LeftTopPoint, RightTopPoint);
		result[SideType.Right] = new MathLine().CalculateLineByPoints(RightTopPoint, RightBottomPoint);
		result[SideType.Bottom] = new MathLine().CalculateLineByPoints(LeftBottomPoint, RightBottomPoint);
		result[SideType.Left] = new MathLine().CalculateLineByPoints(LeftTopPoint, LeftBottomPoint);

		return result;
	}

	private static void GetIntersectionPoint(Vector2 center, Vector2 originDirection, IEnumerable<KeyValuePair<SideType,MathLine>> sides, MathLine inputLine, out SideType intersectionSide, out Vector2 intersectionPoint)
	{
		intersectionSide = SideType.None;
		intersectionPoint = new Vector2(Mathf.Infinity, Mathf.Infinity);

		float sqrDistance = Mathf.Infinity;
		
		foreach (var side in sides)
		{
			SideType sideType = side.Key;
			MathLine line = side.Value;
			
			Vector2 point = line.GetIntersectionPoint(inputLine.Coefficients);
			Vector2 direction = point - center;
			float distance = Vector2.SqrMagnitude(direction);
			
			if (distance <= sqrDistance && Vector2.Dot(direction, originDirection) > 0)
			{
				sqrDistance = distance;
				intersectionSide = sideType;
				intersectionPoint = point;
			}
		}
	}

	public static void GetExternalIntersectionPoint(this Cell cell, MathLine inputLine, Vector2 center, Vector2 direction, float offset, out SideType intersectionSide, out Vector2 intersectionPoint)
	{
		// Exclude Right side from calculations
		var sides = cell.GetCellSides(offset).Where(i => i.Key != SideType.Right);
		// Get best intersection data
		GetIntersectionPoint(center, direction, sides, inputLine, out intersectionSide, out intersectionPoint);
	}

	public static void GetInternalIntersectionPoint(this Cell cell, MathLine inputLine, Vector2 center, Vector2 direction, float offset, out SideType intersectionSide, out Vector2 intersectionPoint)
	{
		// Exclude Left side from calculations
		var sides = cell.GetCellSides(offset).Where(i => i.Key != SideType.Left);
		// Get best intersection data
		GetIntersectionPoint(center, direction, sides, inputLine, out intersectionSide, out intersectionPoint);
	}
}

public class Line : IDisposable, ICloneable
{
	public float LineWidth { get; set; }
	public Color LineColor { get; set; }
	public int LineOrder { get; set; }
	public Cell[] Cells { get; set; }
	public bool IsDiagonal { get; set; }
	public bool IsEnabled { get; private set; }

	private VectorLine _line;
	private Vector2? _startPoint;
	private Vector2? _endPoint;

	public Line(float lineWidth, 
	            Color lineColor, 
	            int lineOrder,
	            List<Cell> cells,
	            Vector2? startPoint = null,
	            Vector2? endPoint = null)
	{
		this.LineWidth = lineWidth;
		this.LineColor = lineColor;
		this.LineOrder = lineOrder;
		this.Cells = cells.ToArray();
		this.IsEnabled = false;

		// Set private fields
		this._startPoint = startPoint;
		this._endPoint = endPoint;
	}

	public void DrawLine()
	{
		IsEnabled = true;

		// Cleanup old Line
		VectorLine.Destroy(ref _line);
		
		// Create new Line
		_line = new VectorLine("Line", new List<Vector2>(), null, LineWidth, LineType.Discrete, Joins.Weld);
		_line.color = LineColor;
		_line.drawDepth = LineOrder;

		Vector2 lastPoint = Vector2.zero;
		// Draw all segments and connection lines
		for(var i = 0; i < Cells.Length; i++)
        {
			var cell = Cells[i];
			// Part 0
			// Add Line's Head
			if (i == 0)
			{
				lastPoint = GetLineStart(cell);
			}

			// Part 1
			_line.points2.Add(lastPoint);
			MathLine line = new MathLine().CalculateLineByPoints(lastPoint, cell.Center);
			SideType side = SideType.None;

			if (cell.Visible)
			{
				cell.GetExternalIntersectionPoint(line, cell.Center, lastPoint - cell.Center, 0f, out side, out lastPoint);
			}
			else
			{
				// Prepare line for Stump
				cell.GetExternalIntersectionPoint(line, cell.Center, lastPoint - cell.Center, LineWidth / 3f, out side, out lastPoint);
			}
			_line.points2.Add(lastPoint);

			// Part 2
			if (cell.Visible)
			{
				// Draw line through cell center
				_line.points2.Add(lastPoint);
				_line.points2.Add(cell.Center);
			}
			else
			{
				// Draw Stump to have nice line ending
				_line.points2.Add(lastPoint);
				_line.points2.Add(lastPoint + LineWidth / 4f * GetStumpVector(side));
			}

			// Part 3
			Vector2 nextPoint;
			// If current point is the last one, take Right Cell's point
			if (i == Cells.Length - 1)
			{
				nextPoint = GetLineEnding(cell);
			}
			// Otherwise take next Cell's center as point
			else 
			{
				Cell nextCell = Cells[i + 1];
				nextPoint = nextCell.Center;
			}
			line = new MathLine().CalculateLineByPoints(cell.Center, nextPoint);

			if (cell.Visible)
            {
				cell.GetInternalIntersectionPoint(line, cell.Center, nextPoint - cell.Center, 0f, out side, out lastPoint);
				// Start line from Cell's center
				_line.points2.Add(cell.Center);
				_line.points2.Add(lastPoint); 
			}
			else
			{
				cell.GetInternalIntersectionPoint(line, cell.Center, nextPoint - cell.Center, LineWidth / 3f, out side, out lastPoint);
				// Draw Stump
				_line.points2.Add(lastPoint + LineWidth / 4f * GetStumpVector(side));
				_line.points2.Add(lastPoint);
			}

			// Part 4
			// Add Line tail
			if (i == Cells.Length - 1)
			{
				_line.points2.Add(lastPoint);
				_line.points2.Add(nextPoint);
			}
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
		Array.Clear(Cells, 0, Cells.Length);
	}
	#endregion

	#region ICloneable implementation
	public object Clone ()
	{
		return this.MemberwiseClone();
	}
	#endregion
	
	private Vector2 GetStumpVector(SideType side)
	{
		switch (side)
		{
		case SideType.Top: return new Vector2(0f, -1f);
		case SideType.Bottom: return new Vector2(0f, 1f);
		case SideType.Left: return new Vector2(1f, 0f);
		case SideType.Right: return new Vector2(-1f, 0f);
		default:
			throw new System.NotImplementedException();
		}
	}
	
	private Vector2 GetLineStart(Cell cell)
    {
		if (_startPoint.HasValue)
		{
			// Use custom start point
			return _startPoint.Value;
		}
		else
		{
			// Line head - Left middle point
			return new Vector2(cell.GetRectPoints().xMin, cell.Center.y);
        }
    }
    
    private Vector2 GetLineEnding(Cell cell)
    {
		if (_endPoint.HasValue)
		{
			// Use custom end point
			return _endPoint.Value;
		}
		else
		{
			// Line tail - Right middle point
			return new Vector2(cell.GetRectPoints().xMax, cell.Center.y);
		}
	}
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
			BuildLines(combinator.GetRandomLines());
		}
		if (GUILayout.Button("Change cells visibility"))
		{
			SetCellsVisibility();
		}
		if (GUILayout.Button("Hide line"))
		{
			DisposeLines();
        }
	}

	void BuildLines(List<Line> lines)
	{
		DisposeLines();
		mainLines = new List<Line>();
		
		foreach (Line line in lines)
		{
			line.DrawLine();
			mainLines.Add(line);
		}
	}

	void SetCellsVisibility()
	{
		if (mainLines != null)
		{
			List<Line> lines = combinator.SetCellsVisibility(mainLines);
			BuildLines(lines);
		}
	}

	void DisposeLines()
	{
		if (mainLines != null)
		{
			foreach (Line line in mainLines)
			{
				line.Dispose();
			}
			mainLines = new List<Line>();
		}
	}
}