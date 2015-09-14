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
	
	public Color gridColor;
	public Color paddingCellColor;
	public Color panelColor;
	
	[Range(1, 10)]
	public int rows = 4;
	[Range(1, 10)]
	public int columns = 4;

	public bool constraintToPanel = true;
	public Vector2 shift = Vector2.zero;
	public Vector2 panelPosition = Vector2.zero;
	public Vector2 screenSize = Vector2.zero;
	public bool isDebugPanel = true;
	public bool isDebugGrid = false;

	public void Copy(GridviewParameters parameters)
	{
		leftBorder = parameters.leftBorder;
		rightBorder = parameters.rightBorder;
		topBorder = parameters.topBorder;
		bottomBorder = parameters.bottomBorder;
		cellPadding = parameters.cellPadding;
		gridColor = parameters.gridColor;
		paddingCellColor = parameters.paddingCellColor;
		panelColor = parameters.panelColor;
		rows = parameters.rows;
		columns = parameters.columns;
		constraintToPanel = parameters.constraintToPanel;
		shift = parameters.shift;
		panelPosition = parameters.panelPosition;
		screenSize = parameters.screenSize;
		isDebugPanel = parameters.isDebugPanel;
		isDebugGrid = parameters.isDebugGrid;
	}

	public static bool operator ==(GridviewParameters a, GridviewParameters b)
	{
		if (a.leftBorder == b.leftBorder && 
		    a.rightBorder == b.rightBorder &&
		    a.topBorder == b.topBorder &&
		    a.bottomBorder == b.bottomBorder &&
		    a.cellPadding == b.cellPadding &&
		    a.gridColor == b.gridColor &&
		    a.paddingCellColor == b.paddingCellColor &&
		    a.panelColor == b.panelColor &&
		    a.rows == b.rows &&
		    a.columns == b.columns &&
		    a.constraintToPanel == b.constraintToPanel &&
		    a.shift == b.shift &&
		    a.panelPosition == b.panelPosition &&
		    a.screenSize == b.screenSize &&
		    a.isDebugPanel == b.isDebugPanel &&
		    a.isDebugGrid == b.isDebugGrid)
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

	public override bool Equals (object obj)
	{
		return this == (GridviewParameters)obj;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}

public class Cell 
{
	public float Width { get; private set; }
	public float Height { get; private set; }
	public Vector2 Center { get; private set; }
	public float Padding { get; private set; }
	public bool Visible { get; private set; }

	public Rect GetRectPoints()
	{
		Vector2 position = Center - new Vector2(Width / 2f, Height / 2f);
		return new Rect(position, new Vector2(Width, Height));
	}
	
	public Rect GetPaddinRectPosition()
	{
		Vector2 position = Center - new Vector2(Width / 2f - Padding, Height / 2f - Padding);
		return new Rect(position, new Vector2(Width - Padding * 2f, Height - Padding * 2f));
	}
	
	public Cell(Vector2 center, float width, float height, float padding, bool visible)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.Padding = padding;
		this.Visible = visible;
	}
}

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera vectrosityCamera;

	[SerializeField] GridviewParameters gridviewParameters;
	GridviewParameters oldGridviewParameters;

	UIPanel panel;
	VectorLine debugCellLine;
	VectorLine debugPaddingLine;
	VectorLine debugPanelLine;

	Cell[,] cells;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		oldGridviewParameters = new GridviewParameters();
		oldGridviewParameters.Copy(gridviewParameters);

		VectorLine.SetCanvasCamera(vectrosityCamera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;

		BuildGrid();
	}

	void FixedUpdate()
	{
		// Check changes first
		if (gridviewParameters.constraintToPanel)
		{
			Vector2 shift = new Vector2(panel.finalClipRegion.x, panel.finalClipRegion.y);
			gridviewParameters.shift = shift;
			Vector2 screenSize = new Vector2(panel.finalClipRegion.z, panel.finalClipRegion.w);
			gridviewParameters.screenSize = screenSize;
			Vector2 panelPosition = (Vector2)panel.transform.localPosition;
			gridviewParameters.panelPosition = panelPosition;
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

	/// <summary>
	/// Get Random cells to build random debug lines
	/// </summary>
	/// <returns>The random cells.</returns>
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

	/// <summary>
	/// Get list of all Cells in grid
	/// </summary>
	/// <returns>The all cells.</returns>
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


	private void DrawPanel()
	{
		int pointCount = 8;

		debugPanelLine = new VectorLine("DebugPanelLine", new Vector2[pointCount], null, 2, LineType.Discrete, Joins.Weld);
		debugPanelLine.color = gridviewParameters.panelColor;

		Vector2 size = new Vector2(panel.finalClipRegion.z, panel.finalClipRegion.w);
		Vector2 position = new Vector2(panel.finalClipRegion.x + panel.transform.localPosition.x + Screen.width / 2f - size.x / 2f, 
		                               panel.finalClipRegion.y + panel.transform.localPosition.y + Screen.height / 2f - size.y / 2f);
		Rect rect = new Rect(position, size);

		debugPanelLine.MakeRect(rect);

		debugPanelLine.Draw();
	}

	/// <summary>
	/// Draw cells for debugging purposes only
	/// </summary>
	/// <param name="cells">Cells.</param>
	private void DrawCells(List<Cell> cells)
	{
		// Calculate point count for Discrete type of line that will contain Rects
		int pointCount = cells.Count * 8;
		// Clear all debug lines each time (no exception if null)
//		VectorLine.Destroy(ref debugCellLine);
//		VectorLine.Destroy(ref debugPaddingLine);
		
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


	/// <summary>
	/// Build grid with cells based on panel properties
	/// </summary>
	private void BuildGrid()
	{
		// Due to documentation, there will be no exception if object is null
		VectorLine.Destroy(ref debugCellLine);
		VectorLine.Destroy(ref debugPaddingLine);
		VectorLine.Destroy(ref debugPanelLine);

		cells = new Cell[gridviewParameters.rows, gridviewParameters.columns];
		Vector2 screenSize = gridviewParameters.screenSize;

		// Setup lines to make them store and show information about cells
		debugCellLine = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		debugPaddingLine = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		// Calculate real size of grid area
		Vector2 borderSize = new Vector2(screenSize.x * (gridviewParameters.rightBorder - gridviewParameters.leftBorder), 
		                                 screenSize.y * (gridviewParameters.bottomBorder - gridviewParameters.topBorder));
		// Calculate real position of grid area
		Vector2 borderPosition = new Vector2(Screen.width / 2f + gridviewParameters.panelPosition.x - borderSize.x / 2f + gridviewParameters.shift.x - screenSize.x * (1f - gridviewParameters.rightBorder) / 2f + screenSize.x * gridviewParameters.leftBorder / 2f,
		                                     Screen.height / 2f + gridviewParameters.panelPosition.y - borderSize.y / 2f + gridviewParameters.shift.y + screenSize.y * (1f - gridviewParameters.bottomBorder) / 2f - screenSize.y * gridviewParameters.topBorder / 2f);
		// Cell dimensions is just dividing correspond grid dimension on column/row count
		Vector2 cellSize = new Vector2(borderSize.x / gridviewParameters.columns,
		                               borderSize.y / gridviewParameters.rows);



		// Calculate position of cells
		int index = 0;
		for (int i = 0; i < gridviewParameters.rows; i++)
		{
			for (int j = 0; j < gridviewParameters.columns; j++)
			{
				// Cell center depends on border position, current cell index and cell size
				Vector2 cellCenter = new Vector2(borderPosition.x + cellSize.x / 2f + j * cellSize.x,
				                                 borderPosition.y + cellSize.y / 2f + i * cellSize.y);
				// Internal area size is controlled by padding parameter
				int cellPadding = (int)(cellSize.x * gridviewParameters.cellPadding);
				// Create cell and setup its parameters
				cells[i, j] = new Cell(cellCenter, cellSize.x, cellSize.y, cellPadding, true);
				index++;
			}
		}


		// Draw grid fod debug purposes only
		if (gridviewParameters.isDebugGrid)
		{
			if (gridviewParameters.isDebugPanel)
			{
//				Rect panelRect = new Rect(borderPosition.x, borderPosition.y,
//				                          borderSize.x, borderSize.y);
				DrawPanel();
			}

			DrawCells(GetAllCells());
		}
	}

	private bool HaveSomeChanges()
	{
		return oldGridviewParameters != gridviewParameters;
	}
}
