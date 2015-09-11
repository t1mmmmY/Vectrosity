using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

[RequireComponent(typeof(UIPanel))]
public class Cell 
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	public Vector2 Center { get; private set; }
	public int Padding { get; private set; }

	public bool IsVisible { get; set; }
	
	public Rect GetRectPoints()
	{
		Vector2 position = Center - new Vector2(Width / 2f, Height / 2f);
		return new Rect(position, new Vector2(Width, Height));
	}

	public Rect GetPaddinRectPosition()
	{
		Vector2 position = Center - new Vector2(Width / 2f - Padding, Height / 2f - Padding);
		return new Rect(position, new Vector2(Width - Padding * 2, Height - Padding * 2));
	}

	public Vector2 GetLeftMiddlePoint()
	{
		return new Vector2(Center.x - (Width / 2f - Padding), Center.y);
	}
	
	public Vector2 GetRightMiddlePoint()
	{
		return new Vector2(Center.x + (Width / 2f - Padding), Center.y);
	}

	public Cell(Vector2 center, int width, int height, int padding, bool isVisible)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.Padding = padding;
		this.IsVisible = isVisible;
	}
}

public class CellGrid
{
	public int Width { get; set; }
	public int Height { get; set; }
	public Vector2 Center { get; set; }
	public int RowCount { get; set; }
	public int ColumnCount { get; set; }
	public int Padding { get; set; }
	public int Margin { get; set; }

	private Cell[,] cells;

	public CellGrid(Vector2 center, int width, int height, int rowCount, int columnCount, int padding, int margin)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.RowCount = rowCount;
		this.ColumnCount = columnCount;
		this.Padding = padding;
		this.Margin = margin;

		cells = new Cell[this.ColumnCount, this.RowCount];
		BuildCells();
	}

	void BuildCells()
	{
		for (int i = 0; i < this.ColumnCount; i++)
		{
			for (int j = 0; j < this.RowCount; j++)
			{
				int width = Mathf.CeilToInt(Width / ColumnCount - 2 * Margin);
				int height = Mathf.CeilToInt(Height / RowCount - 2 * Margin);

				float centerX = (i + 1) * Width / ColumnCount - width / 2f - Margin;
				float centerY = (j + 1) * Height / RowCount - height / 2f - Margin;

				cells[i, j] = new Cell(new Vector2(centerX, centerY), width, height, this.Padding, true);
			}
		}
	}

	private Cell GetRandomCellInColumn(int columnNumber)
	{
		// Generate random cell's index from that column. Avoid Random.value == 1f
		int randomItemNumber = Mathf.FloorToInt((Random.value - 0.00001f) * RowCount);
		return cells[columnNumber, randomItemNumber];
	}

	public List<Cell> GetRandomLine()
	{
		List<Cell> result = new List<Cell>();

		// Going through each column and get random Cell from it
		for(int i = 0; i < ColumnCount; i++)
		{
			result.Add(GetRandomCellInColumn(i));
		}

		return result;
	}

	public List<Cell> GetAllCells()
	{
		List<Cell> result = new List<Cell>();

		for (int i = 0; i < this.ColumnCount; i++)
		{
			for (int j = 0; j < this.RowCount; j++)
			{
				result.Add(cells[i, j]);
			}
		}

		return result;
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

	private CellGrid grid;

	private Cell[] startCells;
	private Cell[] endCells;

	private VectorLine debugCellLine;
	private VectorLine debugCellPaddingLine;

	// Use this for initialization
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		
		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;
		//camera.cachedCamera.cull

		grid = new CellGrid(center: new Vector2(Screen.width / 2, Screen.height / 2),
		                    width: Screen.width,
		                    height: Screen.height,
		                    rowCount: 3,
		                    columnCount: 5,
		                    padding: 10,
		                    margin: 10);

		// For debug purposes
		DrawCells(grid.GetAllCells());

		// Set up main line
		line = new VectorLine("MainLine", new List<Vector2>(), null, lineWidth, LineType.Discrete, Joins.Weld);
		line.color = lineColor;

		// Set initial random cells
		startCells = grid.GetRandomLine().ToArray();
		endCells = grid.GetRandomLine().ToArray();
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
		// Clear all debug lines each time (no exception if null)
		VectorLine.Destroy(ref debugCellLine);
		VectorLine.Destroy(ref debugCellPaddingLine);

		// Build line with proper parameters
		debugCellLine = new VectorLine("DebugLine", new Vector2[pointCount], null, 1, LineType.Discrete, Joins.Weld);
		debugCellLine.color = Color.white;
		debugCellPaddingLine = new VectorLine("DebugPaddingLine", new Vector2[pointCount], null, 1, LineType.Discrete, Joins.Weld);
		debugCellPaddingLine.color = Color.blue;

		// Build each Cell
		for(int i = 0; i < cells.Count; i++)
		{
			Cell cell = cells[i];
			debugCellLine.MakeRect(cell.GetRectPoints(), i*8); 
			debugCellPaddingLine.MakeRect(cell.GetPaddinRectPosition(), i*8);
		}

		// Show results
		debugCellLine.Draw();
		debugCellPaddingLine.Draw();
	}

	bool isRebuild = false;
	float lastLerp = 999f;

	// Update is called once per frame
	void Update ()
	{
		float lerp = Mathf.Repeat(Time.time, 1f);

		// We started loop again (tail is bigger than head)
		if (lastLerp > lerp)
		{
			isRebuild = false;
		}
		lastLerp = lerp;

		if (!isRebuild)
		{
			isRebuild = true;
			startCells = endCells;
			endCells = grid.GetRandomLine().ToArray<Cell>();
		}

		BuildLine(startCells, endCells, lerp); 
	}
}
