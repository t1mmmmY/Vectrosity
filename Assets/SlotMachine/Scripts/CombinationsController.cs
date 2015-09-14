using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(VectrosityGridview))]
public class CombinationsController : MonoBehaviour 
{
	[Range(1, 10)]
	[SerializeField] int maxCombinations = 5;
	[Range(0.0f, 20.0f)]
	[SerializeField] float minWidth = 1.0f;
	[Range(0.0f, 20.0f)]
	[SerializeField] float maxWidth = 10.0f;

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
		
		for (int i = 0; i < allCells.GetLength(1); i++)
		{
			int randomIndex = Random.Range(0, allCells.GetLength(0));
			result.Add(allCells[randomIndex, i]);
		}
		
		return result;
	}
	
	/// <summary>
	/// Gets the random combinations.
	/// </summary>
	/// <returns>The random combinations.</returns>
	public List<CombinationOfCells> GetRandomCombinations()
	{
		List<CombinationOfCells> combinations = new List<CombinationOfCells>();
		
		int countCombinations = Random.Range(1, maxCombinations);
		for (int i = 0; i < countCombinations; i++)
		{
			List<Cell> randomCells = GetRandomCells();
			Color lineColor = new Color(Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f));
			float lineWidth = Random.Range(minWidth, maxWidth);
			combinations.Add(new CombinationOfCells(randomCells, lineColor, lineWidth));
		}
		
		return combinations;
	}
}
