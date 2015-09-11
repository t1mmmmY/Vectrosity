using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

[RequireComponent(typeof(UIPanel))]
public class Cell 
{
	public int Width { get; set; }
	public int Height { get; set; }
	public Vector2 Center { get; set; }
	public bool IsVisible { get; set; }

	public Rect GetRectPoints()
	{
		Vector2 position = Center - new Vector2(Width / 2f, Height / 2f);
		return new Rect(position, new Vector2(Width, Height));
	}

	public Vector2 GetLeftMiddlePoint()
	{
		return new Vector2(Center.x - Width / 2f, Center.y);
	}
	
	public Vector2 GetRightMiddlePoint()
	{
		return new Vector2(Center.x + Width / 2f, Center.y);
	}

	public Cell(Vector2 center, int width, int height, bool isVisible)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.IsVisible = isVisible;
	}
}

public class VectrosityPanelGrid : MonoBehaviour
{
	public UIPanel panel;
	public UICamera camera;
	public VectorLine line;

	public int segmentPixelWidth = 50;
	public float lineWidth = 5f;
	public Color lineColor = Color.white;

	public Vector2[] screenJoints1;
	public Vector2[] screenJoints2;
	public Vector2[] screenJoints3;

	private Cell[] cellJoints1;
	private Cell[] cellJoints2;
	private Cell[] cellJoints3;

	private Cell[] startCells;
	private Cell[] endCells;

	private VectorLine debugLine;

	// Use this for initialization
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		
		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;
		//camera.cachedCamera.cull

		line = new VectorLine("MainLine", new List<Vector2>(), null, lineWidth, LineType.Discrete, Joins.Weld);
		line.color = lineColor;

		// Convert arrays of joints to arrays of cells
		cellJoints1 = screenJoints1.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();
		cellJoints2 = screenJoints2.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();
		cellJoints3 = screenJoints3.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();

		List<Cell> cells = cellJoints1.ToList<Cell>();
		cells.AddRange(cellJoints2.ToList<Cell>());
		cells.AddRange(cellJoints3.ToList<Cell>());

		DrawCells(cells);
	}

	Vector2 VectorRelativeToAbsolute(Vector2 input)
	{
		Vector2 result = new Vector2(input.x, input.y);

		result /= 100f;
		result.x *= Screen.width; //apply scale
		result.y *= Screen.height / 2f; //apply scale
		result.y += Screen.height / 2f; //apply shift

		return result;
	}

	Cell MakeCell(Vector2 position)
	{
		return new Cell(position, segmentPixelWidth, segmentPixelWidth, Random.value > 0.5f); //square cell
	}

	void BuildLine(Cell[] start, Cell[] end, float lerpValue)
	{
		// Perform some initial check
		lerpValue = Mathf.Clamp01(lerpValue);
		if ((start.Length == 0) || (start.Length != end.Length))
		{
			Debug.LogWarning("Array can't be empty, or length of arrays mismatch");
			return;
		}

		// Clear line points
		line.Resize(0);

		// Draw all segments and connection lines
		for (int i = 0; i < start.Length; i++)
		{
			// Draw segments and theirs stumps in order to get nice segment/line joints
			// Order is important to have working Joins.Weld option

			// Avoid drawing left stump for first segment
			if (i != 0)
			{
				line.points2.Add(Vector2.Lerp(start[i].GetLeftMiddlePoint(), end[i].GetLeftMiddlePoint(), lerpValue) - new Vector2(lineWidth / 2f, 0f));
				line.points2.Add(Vector2.Lerp(start[i].GetLeftMiddlePoint(), end[i].GetLeftMiddlePoint(), lerpValue));
			}
			// Draw segment itself
			if (start[i].IsVisible)
			{
				line.points2.Add(Vector2.Lerp(start[i].GetLeftMiddlePoint(), end[i].GetLeftMiddlePoint(), lerpValue));
				line.points2.Add(Vector2.Lerp(start[i].GetRightMiddlePoint(), end[i].GetRightMiddlePoint(), lerpValue));
			}
			// Avoid drawing right stump for last segment
			if (i != start.Length - 1)
			{
				line.points2.Add(Vector2.Lerp(start[i].GetRightMiddlePoint(), end[i].GetRightMiddlePoint(), lerpValue));
				line.points2.Add(Vector2.Lerp(start[i].GetRightMiddlePoint(), end[i].GetRightMiddlePoint(), lerpValue) + new Vector2(lineWidth / 2f, 0f));
			}
			// Draw segment connection lines (we need to do this after drawing segment in order to make Joins.Weld option work)
			if (i < (start.Length - 1))
			{
				//Draw connection cell line
				line.points2.Add(Vector2.Lerp(start[i].GetRightMiddlePoint(), end[i].GetRightMiddlePoint(), lerpValue) + new Vector2(lineWidth / 2f, 0f));
				line.points2.Add(Vector2.Lerp(start[i+1].GetLeftMiddlePoint(), end[i+1].GetLeftMiddlePoint(), lerpValue) - new Vector2(lineWidth / 2f, 0f));
			}
		}

		//Update and draw line
		line.Draw();
	}
	
	void DrawCells(List<Cell> cells)
	{
		// Calculate point count for Discrete type of line that will contain Rects
		int pointCount = cells.Count * 8;
		// Clear debug line each time (no exception if null)
		VectorLine.Destroy(ref debugLine);
		// Build line with proper parameters
		debugLine = new VectorLine("DebugLine", new Vector2[pointCount], null, 1, LineType.Discrete, Joins.Weld);
		debugLine.color = Color.white;

		// Build each Cell
		for(int i = 0; i < cells.Count; i++)
		{
			Cell cell = cells[i];
			debugLine.MakeRect(cell.GetRectPoints(), i*8); 
		}

		// Show results
		debugLine.Draw();
	}

	// Update is called once per frame
	void Update ()
	{
		int num = Mathf.FloorToInt(Mathf.Repeat(Time.time, 3f));
		float lerp = Mathf.Repeat(Time.time, 1f);

		if (num == 0)
		{
			startCells = cellJoints1;
			endCells = cellJoints2;
		}
		else if (num == 1)
		{
			startCells = cellJoints2;
			endCells = cellJoints3;
		}
		else if (num == 2)
		{
			startCells = cellJoints3;
			endCells = cellJoints1;
		}

		BuildLine(startCells, endCells, lerp); 
	}
}
