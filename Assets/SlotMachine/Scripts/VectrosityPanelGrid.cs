using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

[RequireComponent(typeof(UIPanel))]
public class Cell 
{
	public int Width { get; set; }
	public int Height { get; set; }
	public Vector2 Center { get; set; }
	public bool IsVisible { get; set; }

	public Rect GetRectPoints()
	{
		return new Rect(Center, new Vector2(Width, Height));
	}

	public Vector2 GetLeftMiddlePoint()
	{
		return new Vector2(Center.x - Width / 2f, Center.y);
	}

	public Vector2 GetRightMiddlePoint()
	{
		return new Vector2(Center.x + Width / 2f, Center.y);
	}

	public Cell(Vector2 center, int width, int height, bool isVisible)
	{
		this.Center = center;
		this.Width = width;
		this.Height = height;
		this.IsVisible = isVisible;
	}
}

public class VectrosityPanelGrid : MonoBehaviour
{
	public UIPanel panel;
	public UICamera camera;
	public VectorLine line;

	public int segmentPixelWidth = 50;

	public Vector2[] screenJoints1;
	public Vector2[] screenJoints2;
	public Vector2[] screenJoints3;

	private Cell[] cellJoints1;
	private Cell[] cellJoints2;
	private Cell[] cellJoints3;

	// Use this for initialization
	void Start ()
	{
		panel = GetComponent<UIPanel>();
		
		VectorLine.SetCanvasCamera(camera.cachedCamera);
		VectorLine.canvas.planeDistance = 0;
		VectorLine.canvas.gameObject.layer = panel.gameObject.layer;
		//camera.cachedCamera.cull

		line = new VectorLine("MainLine", new List<Vector2>(), null, 5f, LineType.Discrete, Joins.Weld);
		// Convert arrays of joints to arrays of cells
		cellJoints1 = screenJoints1.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();
		cellJoints2 = screenJoints2.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();
		cellJoints3 = screenJoints3.Select(i => MakeCell(VectorRelativeToAbsolute(i))).ToArray<Cell>();

		StartCoroutine(ChangeColor());
	}

	IEnumerator ChangeColor()
	{
		line.color = new Color(Random.value, Random.value, Random.value);
		yield return new WaitForSeconds(1.0f);

		//Repeat again
		StartCoroutine(ChangeColor());
	}

	Vector2 VectorRelativeToAbsolute(Vector2 input)
	{
		Vector2 result = new Vector2(input.x, input.y);

		result /= 100f;
		result.x *= Screen.width; //apply scale
		result.y *= Screen.height / 2f; //apply scale
		result.y += Screen.height / 2f; //apply shift

		return result;
	}

	Cell MakeCell(Vector2 position)
	{
		return new Cell(position, segmentPixelWidth, segmentPixelWidth, Random.value > 0.5f); //square cell
	}

	void BuildLine(Cell[] start, Cell[] end, float lerpValue)
	{
		// Perform some initial check
		lerpValue = Mathf.Clamp01(lerpValue);
		if ((start.Length == 0) || (start.Length != end.Length))
		{
			Debug.LogWarning("Array can't be empty, or length of arrays mismatch");
			return;
		}

		// Clear line points
		line.Resize(0);

		// Adding initial point
		Vector2 posStart = new Vector2(0, 0); //left middle screen point
		Vector2 posEnd = Vector2.Lerp(start[0].GetLeftMiddlePoint(), end[0].GetLeftMiddlePoint(), lerpValue);

		line.points2.Add(VectorRelativeToAbsolute(posStart));
		line.points2.Add(posEnd);

		// Adding all segments
		for (int i = 1; i < start.Length; i++)
		{
			if (start[i-1].IsVisible)
			{
				line.points2.Add(Vector2.Lerp(start[i-1].GetLeftMiddlePoint(), end[i-1].GetLeftMiddlePoint(), lerpValue));
				line.points2.Add(Vector2.Lerp(start[i-1].GetRightMiddlePoint(), end[i-1].GetRightMiddlePoint(), lerpValue));
			}

			line.points2.Add(Vector2.Lerp(start[i-1].GetRightMiddlePoint(), end[i-1].GetRightMiddlePoint(), lerpValue));
			line.points2.Add(Vector2.Lerp(start[i].GetLeftMiddlePoint(), end[i].GetLeftMiddlePoint(), lerpValue));

			if (end[i-1].IsVisible)
			{
				line.points2.Add(Vector2.Lerp(start[i].GetLeftMiddlePoint(), end[i].GetLeftMiddlePoint(), lerpValue));
				line.points2.Add(Vector2.Lerp(start[i].GetRightMiddlePoint(), end[i].GetRightMiddlePoint(), lerpValue));
			}
		}

		// Adding final point
		posStart = Vector2.Lerp(start[start.Length - 1].GetRightMiddlePoint(), end[end.Length - 1].GetRightMiddlePoint(), lerpValue);
		posEnd = new Vector2(100f, 0f); //right middle screen point

		line.points2.Add(posStart);
		line.points2.Add(VectorRelativeToAbsolute(posEnd));

		//Update and draw line
		line.Draw();
	}

	// Update is called once per frame
	void Update ()
	{
		int num = Mathf.FloorToInt(Mathf.Repeat(Time.time, 3f));
		float lerp = Mathf.Repeat(Time.time, 1f);

		if (num == 0)
		{
			BuildLine(cellJoints1, cellJoints2, lerp); 
		}
		else if (num == 1)
		{
			BuildLine(cellJoints2, cellJoints3, lerp); 
		}
		else if (num == 2)
		{
			BuildLine(cellJoints3, cellJoints1, lerp); 
		}
	}
}
