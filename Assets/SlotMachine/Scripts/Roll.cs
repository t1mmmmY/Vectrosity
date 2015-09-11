using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIPanel))]
public class Roll : MonoBehaviour 
{
	[SerializeField] Cell[] cellPrefabs;
	[SerializeField] UIGrid grid;

	float _leftBorder = 0.0f;
	float _rightBorder = 1.0f;
	int _cellsCount = 10;
	int _cellSize = 180;
	
	UIPanel panel;
	List<Cell> cells;
	bool reposition = false;

	void Awake()
	{
		panel = GetComponent<UIPanel>();
		cells = new List<Cell>();
	}

	public void SetBorders(float left, float right)
	{
		_leftBorder = left;
		_rightBorder = right;

		panel.SetAnchor(transform.parent);
		panel.leftAnchor.Set(_leftBorder, 0);
		panel.rightAnchor.Set(_rightBorder, 0);
	}

	public void SetCellsCount(int count)
	{
		_cellsCount = count;
	}

	public void SetCellSize(int cellSize)
	{
		_cellSize = cellSize;
		grid.cellWidth = _cellSize;
		grid.cellHeight = _cellSize;
	}

	public void Init()
	{

		for (int i = 0; i < _cellsCount; i++)
		{
			int randomCellNumber = Random.Range(0, cellPrefabs.Length);
			Cell newCell = NGUITools.AddChild(grid.gameObject, cellPrefabs[randomCellNumber].gameObject).GetComponent<Cell>();
			newCell.gameObject.name = string.Format("{0}_{1}", cellPrefabs[randomCellNumber].gameObject.name, i);
			newCell.SetSize(_cellSize);
			cells.Add(newCell);
		}
		grid.Reposition();
	}

	public List<Cell> GetVisibleCells()
	{
		List<Cell> visibleCells = new List<Cell>();
		foreach (Cell cell in cells)
		{
			if (cell.visible)
			{
				visibleCells.Add(cell);
			}
		}

		return visibleCells;
	}

	//Reposition rolls
	void LateUpdate()
	{
		if (!reposition)
		{
			reposition = true;
			grid.Reposition();
		}
	}
}
