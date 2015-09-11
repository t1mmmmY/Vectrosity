using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

[RequireComponent(typeof(UIPanel))]
public class VectrosityGridview : MonoBehaviour 
{
	[SerializeField] UICamera camera;

	[Range(0.0f, 1.0f)]
	[SerializeField] float leftBorder = 0.1f;
	[Range(0.0f, 1.0f)]
	[SerializeField] float rightBorder = 0.9f;
	[Range(0.0f, 1.0f)]
	[SerializeField] float topBorder = 0.1f;
	[Range(0.0f, 1.0f)]
	[SerializeField] float bottomBorder = 0.9f;

	[SerializeField] Color gridColor;

	[Range(1, 10)]
	[SerializeField] int rows = 4;
	[Range(1, 10)]
	[SerializeField] int columns = 4;

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

//	void FixedUpdate()
//	{
//		//I need to check changes first
//		DrawGridview();
//	}

	Cell[] GetRandomCells()
	{
		Cell[] randomCells = new Cell[columns];
		for (int i = 0; i < columns; i++)
		{
			int randomIndex = Random.Range(0, rows);
			randomCells[i] = cells[randomIndex, i];
//			Debug.Log(randomCells[i].GetRectPoints());
		}
		return randomCells;
	}

	void DrawGridview()
	{
		cells = new Cell[rows, columns];
		
		line = new VectorLine("Grid", new Vector2[rows * columns * 8], null, 3f, LineType.Discrete, Joins.Weld);	

		Vector2 borderPosition = new Vector2(Screen.width * leftBorder, Screen.height * (1.0f - bottomBorder));
		Vector2 borderSize = new Vector2(Screen.width * (rightBorder - leftBorder), Screen.height * (bottomBorder - topBorder));
		int index = 0;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				int cellWidth = (int)(borderSize.x / columns);
				int cellHeight = (int)(borderSize.y / rows);
				Vector2 cellCenter = new Vector2(borderPosition.x + cellWidth / 2 + j * cellWidth,
				                                 borderPosition.y + cellHeight / 2 + i * cellHeight);
				bool cellVisible = true;
				cells[i, j] = new Cell(cellCenter, cellWidth, cellHeight, cellVisible);
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
		line.color = gridColor;
		line.MakeRect(rect, index * 8);
		line.Draw();
	}
}
