using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMachine : MonoBehaviour 
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

}
