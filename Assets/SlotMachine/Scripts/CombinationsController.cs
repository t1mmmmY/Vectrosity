using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(VectrosityGridview))]
public class CombinationsController : MonoBehaviour 
{
	[Range(1, 10)]
	[SerializeField] int minCombinations = 1;
	[Range(1, 10)]
	[SerializeField] int maxCombinations = 5;
	[Range(0.0f, 20.0f)]
	[SerializeField] float minWidth = 1.0f;
	[Range(0.0f, 20.0f)]
	[SerializeField] float maxWidth = 10.0f;
	[Range(0.0f, 1.0f)]
	[SerializeField] float cellsVisibility = 1.0f;

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

		//Get one cell from each column and add to the list
		for (int i = 0; i < allCells.GetLength(1); i++)
		{
			int randomIndex = Random.Range(0, allCells.GetLength(0));

			//Clone cell
			Cell randomCell = allCells[randomIndex, i].Clone() as Cell;

			//Set cell visibility
			bool isVisible = Random.Range(0.0f, 1.0f) <= cellsVisibility;
			randomCell.SetVisible(isVisible);

			result.Add(randomCell);
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

		int countCombinations = Random.Range(minCombinations, maxCombinations);

		//Create combinations
		for (int i = 0; i < countCombinations; i++)
		{
			List<Cell> randomCells = GetRandomCells();

			//Create random color
			Color lineColor = new Color(Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f),
			                            Random.Range(0.0f, 1.0f));

			float lineWidth = Random.Range(minWidth, maxWidth);

			//Add combination to the list
			combinations.Add(new CombinationOfCells(randomCells, lineColor, lineWidth));
		}
		
		return combinations;
	}
}
