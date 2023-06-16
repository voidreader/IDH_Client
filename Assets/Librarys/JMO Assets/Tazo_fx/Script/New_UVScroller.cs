using UnityEngine;
using System.Collections;

public class New_UVScroller : MonoBehaviour {
	public float speedy = 0.5f;
	public float speedx = 0.0f;
	
	void FixedUpdate ()
	{
		float offsety = Time.deltaTime * speedy;
		float offsetx = Time.deltaTime * speedx;

		
		GetComponent<Renderer>().material.mainTextureOffset = new Vector2 (offsetx,offsety);
	}
}