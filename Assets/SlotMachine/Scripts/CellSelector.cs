using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellSelector : MonoBehaviour 
{
	[SerializeField] List<CellMono> cells;

	public static System.Action<List<CellMono>> onSelectCells;

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


	List<CellMono> GetRandomCells()
	{
		Dictionary<int, List<CellMono>> dictionaryCells = SlotMachine.Instance.GetVisibleCells();
		List<CellMono> randomCells = new List<CellMono>();

		foreach (int key in dictionaryCells.Keys)
		{
			int randomIndex = Random.Range(0, dictionaryCells[key].Count);
			randomCells.Add(dictionaryCells[key][randomIndex]);
		}

		return randomCells;
	}
}
