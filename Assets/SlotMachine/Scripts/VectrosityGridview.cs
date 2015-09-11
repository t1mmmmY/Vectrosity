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

	public bool constraintToPanel = true;
	public Vector2 shift = Vector2.zero;
	public Vector2 screenSize = Vector2.zero;

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
		constraintToPanel = parameters.constraintToPanel;
		shift = parameters.shift;
		screenSize = parameters.screenSize;
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
		    a.columns == b.columns &&
		    a.constraintToPanel == b.constraintToPanel &&
		    a.shift == b.shift &&
		    a.screenSize == b.screenSize)
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

	Cell[,] cells;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		oldGridviewParameters = new GridviewParameters();
		oldGridviewParameters.Copy(gridviewParameters);

		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;

		BuildGrid();
	}

	void FixedUpdate()
	{
		//I need to check changes first
		if (gridviewParameters.constraintToPanel)
		{
			Vector2 shift = new Vector2(panel.finalClipRegion.x, panel.finalClipRegion.y);
			gridviewParameters.shift = shift;
			Vector2 screenSize = new Vector2(panel.finalClipRegion.z, panel.finalClipRegion.w);
			gridviewParameters.screenSize = screenSize;
		}
		else
		{
			Vector2 screenSize = new Vector2(Screen.width, Screen.height);
			gridviewParameters.screenSize = screenSize;
		}

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
		Vector2 screenSize = gridviewParameters.screenSize;
		
		if (debugCellLine != null)
		{
			VectorLine.Destroy(ref debugCellLine);
		}
		if (debugPaddingLine != null)
		{
			VectorLine.Destroy(ref debugPaddingLine);
		}
		debugCellLine = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		debugPaddingLine = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		
		Vector2 borderSize = new Vector2(screenSize.x * (gridviewParameters.rightBorder - gridviewParameters.leftBorder), 
		                                 screenSize.y * (gridviewParameters.bottomBorder - gridviewParameters.topBorder));
		Vector2 borderPosition = new Vector2(Screen.width / 2 - borderSize.x / 2 + gridviewParameters.shift.x + screenSize.x * gridviewParameters.leftBorder, 
		                                     Screen.height / 2 - borderSize.y / 2 + gridviewParameters.shift.y + screenSize.y * (1.0f - gridviewParameters.bottomBorder));

		int index = 0;
		
		for (int i = 0; i < gridviewParameters.rows; i++)
		{
			for (int j = 0; j < gridviewParameters.columns; j++)
			{
				int cellWidth = (int)(borderSize.x / gridviewParameters.columns);
				int cellHeight = (int)(borderSize.y / gridviewParameters.rows);
				Vector2 cellCenter = new Vector2(borderPosition.x + cellWidth / 2 + j * cellWidth,
				                                 borderPosition.y + cellHeight / 2 + i * cellHeight);
				
				cellWidth -= (int)(borderSize.x * gridviewParameters.cellMargin);
				cellHeight -= (int)(borderSize.y * gridviewParameters.cellMargin);
				
				int cellPadding = (int)(cellWidth * gridviewParameters.cellPadding);
				bool cellVisible = true;
				cells[i, j] = new Cell(cellCenter, cellWidth, cellHeight, cellPadding);
				index++;
			}
		}

		DrawCells(GetAllCells());
	}

	bool HaveSomeChanges()
	{
		return oldGridviewParameters != gridviewParameters;
	}
}
