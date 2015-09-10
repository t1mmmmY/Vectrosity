using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachineGenerator : MonoBehaviour 
{
	[SerializeField] SlotMachine slotMachine;

	[Range(2, 10)]
	[SerializeField] int countRolls = 4;

	[Range(5, 100)]
	[SerializeField] int countCellsInRoll = 10;


	//Not working correctly yet
	[Range(0.0f, 0.2f)]
	[SerializeField] float border = 0.01f; 

	[SerializeField] Roll rollPrefab;
	[SerializeField] UIWidget parentWidget;
	
	List<Roll> rolls;
	int standardCellSize = 180;
	
	void Awake()
	{
		rolls = new List<Roll>();
	}
	
	void Start()
	{
		float step = 1.0f / countRolls;
		int cellSize = standardCellSize * 4 / countRolls;

		for (int i = 0; i < countRolls; i++)
		{
			Roll newRoll = NGUITools.AddChild(parentWidget.gameObject, rollPrefab.gameObject).GetComponent<Roll>();

			//Set anchors
			float leftBorder = step * i + border;
			float rightBorder = step * (i + 1) - border;
			newRoll.SetBorders(leftBorder, rightBorder);

			//Set cell count
			newRoll.SetCellsCount(countCellsInRoll);

			//Set cell size
			newRoll.SetCellSize(cellSize);

			rolls.Add(newRoll);
		}
		slotMachine.Init( rolls.ToArray());
	}
}
