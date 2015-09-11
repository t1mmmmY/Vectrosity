using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

[System.Serializable]
public class GridviewParameters
{
	[Range(0.0f, 1.0f)]
	public float leftBorder = 0.1f;
	[Range(0.0f, 1.0f)]
	public float rightBorder = 0.9f;
	[Range(0.0f, 1.0f)]
	public float topBorder = 0.1f;
	[Range(0.0f, 1.0f)]
	public float bottomBorder = 0.9f;

	[Range(0.0f, 0.5f)]
	public float cellPadding = 0.0f;

	[Range(0.0f, 0.5f)]
	public float cellMargin = 0.02f;
	
	public Color gridColor;
	public Color paddingCellColor;
	
	[Range(1, 10)]
	public int rows = 4;
	[Range(1, 10)]
	public int columns = 4;


	public void Copy(GridviewParameters parameters)
	{
		leftBorder = parameters.leftBorder;
		rightBorder = parameters.rightBorder;
		topBorder = parameters.topBorder;
		bottomBorder = parameters.bottomBorder;
		cellPadding = parameters.cellPadding;
		cellMargin = parameters.cellMargin;
		gridColor = parameters.gridColor;
		paddingCellColor = parameters.paddingCellColor;
		rows = parameters.rows;
		columns = parameters.columns;
	}

	public static bool operator ==(GridviewParameters a, GridviewParameters b)
	{
		if (a.leftBorder == b.leftBorder && 
		    a.rightBorder == b.rightBorder &&
		    a.topBorder == b.topBorder &&
		    a.bottomBorder == b.bottomBorder &&
		    a.cellPadding == b.cellPadding &&
		    a.cellMargin == b.cellMargin &&
		    a.gridColor == b.gridColor &&
		    a.paddingCellColor == b.paddingCellColor &&
		    a.rows == b.rows &&
		    a.columns == b.columns)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool operator !=(GridviewParameters a, GridviewParameters b)
	{
		return !(a == b);
	}
}

public class Cell 
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	public Vector2 Center { get; private set; }
	public int Padding { get; private set; }

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
	
	public Cell(Vector2 center, int width, int height, int padding)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.Padding = padding;
	}
}

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera camera;

	[SerializeField] GridviewParameters gridviewParameters;
	GridviewParameters oldGridviewParameters;

	UIPanel panel;
	VectorLine debugCellLine;
	VectorLine debugPaddingLine;
	VectorLine line;

	int lineWidth = 5;
	Color lineColor = Color.blue;

	Cell[,] cells;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		oldGridviewParameters = new GridviewParameters();
		oldGridviewParameters.Copy(gridviewParameters);

		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;

		line = new VectorLine("MainLine", new List<Vector2>(), null, lineWidth, LineType.Discrete, Joins.Weld);
		line.color = lineColor;

		BuildGrid();
	}

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			var randomCells = GetRandomCells();
			BuildLine(randomCells);
		}
	}

	void FixedUpdate()
	{
		//I need to check changes first

		if (HaveSomeChanges())
		{
			BuildGrid();
			oldGridviewParameters.Copy(gridviewParameters);
		}
	}

	public List<Cell> GetRandomCells()
	{
		List<Cell> result = new List<Cell>();
		
		for (int i = 0; i < gridviewParameters.columns; i++)
		{
			int randomIndex = Random.Range(0, gridviewParameters.rows);
			result.Add(cells[randomIndex, i]);
		}
		
		return result;
	}
	
	public List<Cell> GetAllCells()
	{
		List<Cell> result = new List<Cell>();
		
		for (int i = 0; i < gridviewParameters.rows; i++)
		{
			for (int j = 0; j < gridviewParameters.columns; j++)
			{
				result.Add(cells[i, j]);
			}
		}
		
		return result;
	}

	void DrawCells(List<Cell> cells)
	{
		// Calculate point count for Discrete type of line that will contain Rects
		int pointCount = cells.Count * 8;
		// Clear all debug lines each time (no exception if null)
		VectorLine.Destroy(ref debugCellLine);
		VectorLine.Destroy(ref debugPaddingLine);

		// Build line with proper parameters
		debugCellLine = new VectorLine("DebugCellLine", new Vector2[pointCount], null, 2, LineType.Discrete, Joins.Weld);
		debugCellLine.color = gridviewParameters.gridColor;
		debugPaddingLine = new VectorLine("DebugPaddingCellLine", new Vector2[pointCount], null, 2, LineType.Discrete, Joins.Weld);
		debugPaddingLine.color = gridviewParameters.paddingCellColor;
		
		// Build each Cell
		for(int i = 0; i < cells.Count; i++)
		{
			Cell cell = cells[i];
			debugCellLine.MakeRect(cell.GetRectPoints(), i*8); 
			debugPaddingLine.MakeRect(cell.GetPaddinRectPosition(), i*8);
		}
		
		// Show results
		debugCellLine.Draw();
		debugPaddingLine.Draw();
	}

	void BuildGrid()
	{
		cells = new Cell[gridviewParameters.rows, gridviewParameters.columns];

		Vector2 borderPosition = new Vector2(Screen.width * gridviewParameters.leftBorder, Screen.height * (1.0f - gridviewParameters.bottomBorder));
		Vector2 gridContentSize = new Vector2(Screen.width * (gridviewParameters.rightBorder - gridviewParameters.leftBorder), 
		                                 Screen.height * (gridviewParameters.bottomBorder - gridviewParameters.topBorder));
		int index = 0;

		for (int i = 0; i < gridviewParameters.rows; i++)
		{
			for (int j = 0; j < gridviewParameters.columns; j++)
			{
				int cellWidth = (int)(gridContentSize.x / gridviewParameters.columns);
				int cellHeight = (int)(gridContentSize.y / gridviewParameters.rows);
				Vector2 cellCenter = new Vector2(borderPosition.x + cellWidth / 2 + j * cellWidth,
				                                 borderPosition.y + cellHeight / 2 + i * cellHeight);

				cellWidth -= (int)(gridContentSize.x * gridviewParameters.cellMargin);
				cellHeight -= (int)(gridContentSize.y * gridviewParameters.cellMargin);

				int cellPadding = (int)(cellWidth * gridviewParameters.cellPadding);

				cells[i, j] = new Cell(cellCenter, cellWidth, cellHeight, cellPadding);
				index++;
			}
		}

		DrawCells(GetAllCells());
	}

	void BuildLine(List<Cell> inputCells)
	{	
		// Clear line points
		line.Resize(0);
		Cell prevCell = null;
		
		// Draw all segments and connection lines
		foreach(var cell in inputCells)
		{
			if (prevCell != null)
			{
				line.points2.Add(prevCell.Center);
				line.points2.Add(cell.Center);
			}
			prevCell = cell;
		}

		//Update and draw line
		line.Draw();
	}

	bool HaveSomeChanges()
	{
		return oldGridviewParameters != gridviewParameters;
	}

}
