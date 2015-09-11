using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellSelector : MonoBehaviour 
{
	[SerializeField] List<Cell> cells;

	public static System.Action<List<Cell>> onSelectCells;

	void OnGUI()
	{
		if (GUILayout.Button("Get random cells"))
		{
			cells = GetRandomCells();
			if (onSelectCells != null)
			{
				onSelectCells(cells);
			}
		}
	}


	List<Cell> GetRandomCells()
	{
		Dictionary<int, List<Cell>> dictionaryCells = SlotMachine.Instance.GetVisibleCells();
		List<Cell> randomCells = new List<Cell>();

		foreach (int key in dictionaryCells.Keys)
		{
			int randomIndex = Random.Range(0, dictionaryCells[key].Count);
			randomCells.Add(dictionaryCells[key][randomIndex]);
		}

		return randomCells;
	}
}
