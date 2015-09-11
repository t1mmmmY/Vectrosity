using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachine : BaseSingleton<SlotMachine> 
{
	[SerializeField] Roll[] rolls;

	public void Init(Roll[] newRolls)
	{
		rolls = new Roll[newRolls.Length];
		for (int i = 0; i < rolls.Length; i++)
		{
			rolls[i] = newRolls[i];
			rolls[i].Init();
		}
	}

	public Dictionary<int, List<CellMono>> GetVisibleCells()
	{
		Dictionary<int, List<CellMono>> dictionaryCells = new Dictionary<int, List<CellMono>>();
//		List<Cell> visibleCells = new List<Cell>();
		for (int i = 0; i < rolls.Length; i++)
		{
			dictionaryCells.Add(i, rolls[i].GetVisibleCells());
//			visibleCells.AddRange(rolls[i].GetVisibleCells());
		}
		return dictionaryCells;
	}

}