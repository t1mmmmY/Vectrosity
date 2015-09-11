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
	public float margin = 0.02f;
	
	public Color gridColor;
	public Color paddingCellColor;
	
	[Range(1, 10)]
	public int rows = 4;
	[Range(1, 10)]
	public int columns = 4;

	public bool constraintToPanel = true;
	public Vector2 shift = Vector2.zero;


	public void Copy(GridviewParameters parameters)
	{
		leftBorder = parameters.leftBorder;
		rightBorder = parameters.rightBorder;
		topBorder = parameters.topBorder;
		bottomBorder = parameters.bottomBorder;
		cellPadding = parameters.cellPadding;
		margin = parameters.margin;
		gridColor = parameters.gridColor;
		paddingCellColor = parameters.paddingCellColor;
		rows = parameters.rows;
		columns = parameters.columns;
		constraintToPanel = parameters.constraintToPanel;
		shift = parameters.shift;
	}

	public static bool operator ==(GridviewParameters a, GridviewParameters b)
	{
		if (a.leftBorder == b.leftBorder && 
		    a.rightBorder == b.rightBorder &&
		    a.topBorder == b.topBorder &&
		    a.bottomBorder == b.bottomBorder &&
		    a.cellPadding == b.cellPadding &&
		    a.margin == b.margin &&
		    a.gridColor == b.gridColor &&
		    a.paddingCellColor == b.paddingCellColor &&
		    a.rows == b.rows &&
		    a.columns == b.columns &&
		    a.constraintToPanel == b.constraintToPanel &&
		    a.shift == b.shift)
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

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera camera;

	[SerializeField] GridviewParameters gridviewParameters;
	GridviewParameters oldGridviewParameters;

	UIPanel panel;
	VectorLine line;
	VectorLine paddingLine;

	Cell[,] cells;

	public static System.Action<Cell[]> onGetRandomCells;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		oldGridviewParameters = new GridviewParameters();
		oldGridviewParameters.Copy(gridviewParameters);

		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;

		DrawGridview();
	}

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			Cell[] randomCells = GetRandomCells();
			if (onGetRandomCells != null)
			{
				onGetRandomCells(randomCells);
			}
		}
	}

	void FixedUpdate()
	{
		//I need to check changes first
		if (gridviewParameters.constraintToPanel)
		{
			gridviewParameters.shift = panel.transform.localPosition;
		}

		if (HaveSomeChanges())
		{
			DrawGridview();
			oldGridviewParameters.Copy(gridviewParameters);
		}
	}

	Cell[] GetRandomCells()
	{
		Cell[] randomCells = new Cell[gridviewParameters.columns];
		for (int i = 0; i < gridviewParameters.columns; i++)
		{
			int randomIndex = Random.Range(0, gridviewParameters.rows);
			randomCells[i] = cells[randomIndex, i];
//			Debug.Log(randomCells[i].GetRectPoints());
		}
		return randomCells;
	}

	void DrawGridview()
	{
		cells = new Cell[gridviewParameters.rows, gridviewParameters.columns];

		if (line != null)
		{
			VectorLine.Destroy(ref line);
		}
		if (paddingLine != null)
		{
			VectorLine.Destroy(ref paddingLine);
		}
		line = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		paddingLine = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	
		
		Vector2 borderPosition = new Vector2(Screen.width * gridviewParameters.leftBorder, Screen.height * (1.0f - gridviewParameters.bottomBorder));
		Vector2 borderSize = new Vector2(Screen.width * (gridviewParameters.rightBorder - gridviewParameters.leftBorder), 
		                                 Screen.height * (gridviewParameters.bottomBorder - gridviewParameters.topBorder));
		int index = 0;

		for (int i = 0; i < gridviewParameters.rows; i++)
		{
			for (int j = 0; j < gridviewParameters.columns; j++)
			{
				int cellWidth = (int)(borderSize.x / gridviewParameters.columns);
				int cellHeight = (int)(borderSize.y / gridviewParameters.rows);
				Vector2 cellCenter = new Vector2(borderPosition.x + cellWidth / 2 + j * cellWidth + gridviewParameters.shift.x,
				                                 borderPosition.y + cellHeight / 2 + i * cellHeight + gridviewParameters.shift.y);

				cellWidth -= (int)(borderSize.x * gridviewParameters.margin);
				cellHeight -= (int)(borderSize.y * gridviewParameters.margin);

				int cellPadding = (int)(cellWidth * gridviewParameters.cellPadding);
				bool cellVisible = true;
				cells[i, j] = new Cell(cellCenter, cellWidth, cellHeight, cellPadding, cellVisible);
				DrawCell(cells[i, j], index);
				index++;
			}
		}
	}

	void DrawCell(Cell cell, int index)
	{
		line.color = gridviewParameters.gridColor;
		line.MakeRect(cell.GetRectPoints(), index * 8);
		line.Draw();

		paddingLine.color = gridviewParameters.paddingCellColor;
		paddingLine.MakeRect(cell.GetPaddinRectPosition(), index * 8);
		paddingLine.Draw();

//		DrawRect(cell.GetRectPoints(), gridviewParameters.gridColor, index);
//		index++;
//
//		DrawRect(cell.GetPaddinRectPosition(), gridviewParameters.paddingCellColor, index);
	}

//	void DrawRect(Rect rect, Color color, int index)
//	{
//		line.color = color;
//		line.MakeRect(rect, index * 8);
//		line.Draw();
//	}

	bool HaveSomeChanges()
	{
		return oldGridviewParameters != gridviewParameters;
	}

}
