using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIPanel))]
public class Roll : MonoBehaviour 
{
//	[Range(0.0f, 1.0f)]
	float _leftBorder = 0.0f;
//	[Range(0.0f, 1.0f)]
	float _rightBorder = 1.0f;
	[SerializeField] Cell[] cellPrefabs;
	int _cellsCount = 10;
	[SerializeField] UIGrid grid;

	UIPanel panel;
	List<Cell> cells;
	bool reposition = false;
	int _cellSize = 180;

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
			newCell.SetSize(_cellSize);
			cells.Add(newCell);
		}
		grid.Reposition();
	}


	void LateUpdate()
	{
		if (!reposition)
		{
			reposition = true;
			grid.Reposition();
		}
	}
}
