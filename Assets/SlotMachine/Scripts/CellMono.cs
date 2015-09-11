using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UI2DSprite))]
public class CellMono : MonoBehaviour 
{
	//May be changed
	UI2DSprite sprite;

	public bool visible
	{
		get 
		{
			return sprite.isVisible;
		}
	}

	void Awake()
	{
		sprite = GetComponent<UI2DSprite>();
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
			//May be changed
			return new Rect((Vector2)position, sprite.localSize);
		}
	}

}
