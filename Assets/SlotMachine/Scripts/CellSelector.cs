using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellSelector : MonoBehaviour 
{
	[SerializeField] List<Cell> cells;

	public static System.Action<List<Cell>> onSelectCells;

	void OnGUI()
	{
//		if (GUILayout.Button("Get random cells"))
//		{
//			cells = GetRandomCells();
//			if (onSelectCells != null)
//			{
//				onSelectCells(cells);
//			}
//		}
	}


	List<Cell> GetRandomCells()
	{
		return cells;
	}
}
