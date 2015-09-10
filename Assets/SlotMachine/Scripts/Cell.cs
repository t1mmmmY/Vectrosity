using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour 
{
	//May be changed
	UI2DSprite sprite;

	void Awake()
	{
		sprite = GetComponent<UI2DSprite>();
		if (sprite == null)
		{
			Debug.LogWarning("UI2DSprite not found");
		}
	}

	public void SetSize(int size)
	{
		sprite.width = size;
		sprite.height = size;
	}

	public Vector3 position
	{
		get 
		{
			//May be changed
			return transform.position;
		}
	}

	public Rect borders
	{
		get
		{
			if (sprite == null)
			{
				Debug.LogError("UI2DSprite not found");
				return new Rect(Vector2.zero, Vector2.zero);
			}

			//May be changed
			return new Rect((Vector2)position, sprite.localSize);
		}
	}

}
