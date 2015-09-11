using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using System.Linq;

public class SlotMachineLines : MonoBehaviour 
{

	void Start()
	{
		CellSelector.onSelectCells += OnSelectCells;
	}

	void OnDestroy()
	{
		CellSelector.onSelectCells -= OnSelectCells;
	}

	void OnSelectCells(List<CellMono> cells)
	{
		foreach(CellMono cell in cells)
		{
			Debug.Log(string.Format("Position {0}; Borders {1}", cell.position, cell.borders));
		}

	}


}
