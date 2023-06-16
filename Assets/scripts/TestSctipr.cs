using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSctipr : MonoBehaviour
{
	public SpriteRenderer sky;

	void Start ()
	{
		sky = UnityCommonFunc.AddSingleComponent<SpriteRenderer>(gameObject);
		sky.sprite = Resources.LoadAll<Sprite>("test")[20];
		sky.drawMode = SpriteDrawMode.Sliced;
		sky.transform.localScale = Vector3.one;
		sky.size = new Vector2(12.8f*5, 7.2f);
	}
}
