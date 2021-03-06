﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(VectrosityGridview))]
public class CombinationsController : MonoBehaviour 
{
	[Range(1, 10)]
	[SerializeField] int minCombinations = 1;
	[Range(1, 10)]
	[SerializeField] int maxCombinations = 5;
	[Range(0.0f, 20.0f)]
	[SerializeField] float minWidth = 1.0f;
	[Range(0.0f, 20.0f)]
	[SerializeField] float maxWidth = 10.0f;
	[Range(0.0f, 1.0f)]
	[SerializeField] float cellsVisibility = 1.0f;

	VectrosityGridview gridview;

	void Start()
	{
		gridview = GetComponent<VectrosityGridview>();
	}

	/// <summary>
	/// Get Random cells to build random debug lines
	/// </summary>
	/// <returns>The random cells.</returns>
	public List<Cell> GetRandomCells()
	{
		Cell[,] allCells = gridview.GetAllCells();
		List<Cell> result = new List<Cell>();

		//Get one cell from each column and add to the list
		for (int i = 0; i < allCells.GetLength(1); i++)
		{
			int randomIndex = Random.Range(0, allCells.GetLength(0));

			//Clone cell
			Cell randomCell = allCells[randomIndex, i].Clone() as Cell;

			//Set cell visibility
			bool isVisible = GetVisibility();
			randomCell.SetVisible(isVisible);
			
			result.Add(randomCell);
		}
		
		return result;
	}
	
	/// <summary>
	/// Gets the random lines.
	/// </summary>
	/// <returns>The random combinations.</returns>
	public List<Line> GetRandomLines()
	{
		List<Line> randomLines = new List<Line>();

		int countLines = Random.Range(minCombinations, maxCombinations);

		//Create combinations
		for (int i = 0; i < countLines; i++)
		{
			List<Cell> randomCells = GetRandomCells();

			//Create random color
			Color lineColor = new Color(Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f));

			float lineWidth = Random.Range(minWidth, maxWidth);
			int lineOrder = i;

			Vector2? startPoint = null;
			Vector2? endPoint = null;

			// Example on how set custom line start
			if ( Random.value <= 0.5f)
			{
				Cell firstCell = randomCells.FirstOrDefault();
				Rect firstRect = firstCell.GetRectPoints();

				if (firstCell.IndexX == 0)
				{
					startPoint = new Vector2(firstRect.xMin, firstRect.yMin);
				}
				if (firstCell.IndexX == gridview.RowCount - 1)
				{
					startPoint = new Vector2(firstRect.xMin, firstRect.yMax);
				}
			}

			// Example on how set custom line end
			if ( Random.value <= 0.5f)
			{
				Cell lastCell = randomCells.LastOrDefault();
				Rect lastRect = lastCell.GetRectPoints();

				if (lastCell.IndexX == 0)
				{
					endPoint = new Vector2(lastRect.xMax, lastRect.yMin);
				}
				if (lastCell.IndexX == gridview.RowCount - 1)
				{
					endPoint = new Vector2(lastRect.xMax, lastRect.yMax);
				}
			}

			//Add combination to the list
			randomLines.Add(new Line(lineWidth, lineColor, lineOrder, randomCells, startPoint, endPoint));
		}
		
		return randomLines;
	}

	public List<Line> SetCellsVisibility(List<Line> lines)
	{
		List<Line> newLines = new List<Line>();

		foreach (Line line in lines)
		{
			Line newLine = line.Clone() as Line;
			newLine.Cells = new Cell[line.Cells.Length];

			for (int i = 0; i < line.Cells.Length; i++)
			{
				newLine.Cells[i] = line.Cells[i].Clone() as Cell;
				newLine.Cells[i].SetVisible(GetVisibility());
			}

			newLines.Add(newLine);
		}

		return newLines;
	}

	private bool GetVisibility()
	{
		return Random.Range(0.0f, 1.0f) <= cellsVisibility;
	}
}
