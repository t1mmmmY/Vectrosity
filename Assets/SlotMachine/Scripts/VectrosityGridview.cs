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
	
	public Color gridColor;
	
	[Range(1, 10)]
	public int rows = 4;
	[Range(1, 10)]
	public int columns = 4;
}

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera camera;

	[SerializeField] GridviewParameters gridviewParameters;

	UIPanel panel;
	VectorLine line;

	Cell[,] cells;

	public static System.Action<Cell[]> onGetRandomCells;
	
	void Start ()
	{
		panel = GetComponent<UIPanel>();


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

	void LateUpdate()
	{
		//I need to check changes first

		if (HaveSomeChanges())
		{
			DrawGridview();
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
		
		line = new VectorLine("Grid", new Vector2[gridviewParameters.rows * gridviewParameters.columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	

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
				Vector2 cellCenter = new Vector2(borderPosition.x + cellWidth / 2 + j * cellWidth,
				                                 borderPosition.y + cellHeight / 2 + i * cellHeight);
				bool cellVisible = true;
				cells[i, j] = new Cell(cellCenter, cellWidth, cellHeight, 0, cellVisible);
				DrawCell(cells[i, j], index);
				index++;
			}
		}
	}

	void DrawCell(Cell cell, int index)
	{
		DrawRect(cell.GetRectPoints(), index);
	}

	void DrawRect(Rect rect, int index)
	{
		line.color = gridviewParameters.gridColor;
		line.MakeRect(rect, index * 8);
		line.Draw();
	}

	bool HaveSomeChanges()
	{
		return false;
	}

}
