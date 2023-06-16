using UnityEngine;
using System.Collections;

public class ScrollingUV : MonoBehaviour
{
	public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
	public Vector2 OffSet = new Vector2(0.0f, 0.0f);
	public Vector2 uvOffset = Vector2.zero;
	public Vector4 Tiling = Vector4.one;
	
	[SerializeField] UITexture			texture = null;
	
	void Awake()
	{
		texture = GetComponent<UITexture>();
		ResetData();
	}
	
	void OnDisable()
	{
		ResetData();
	}
	
	private void ResetData()
	{
		uvOffset        = OffSet;
		
		if(texture != null)
		{
			texture.material.SetFloat("_TexScrollX", uvOffset.x);
			texture.material.SetFloat("_TexScrollY", uvOffset.y);
			texture.material.SetVector("_Tiling", Tiling);
		}
	}
	
	void LateUpdate()
	{
		if(texture != null)
		{
			if(texture.drawCall != null)
			{
				if (null != texture.drawCall.dynamicMaterial)
				{
					uvOffset += (uvAnimationRate * Time.deltaTime);
					
					texture.onRender = (Material mat) =>
					{
						mat.SetFloat("_TexScrollX", uvOffset.x);
						mat.SetFloat("_TexScrollY", uvOffset.y);
						mat.SetVector("_Tiling", Tiling);
					};
					
					// 					texture.drawCall.dynamicMaterial.SetFloat("_TexScrollX", uvOffset.x);
					// 					texture.drawCall.dynamicMaterial.SetFloat("_TexScrollY", uvOffset.y);
					// 					texture.drawCall.dynamicMaterial.SetVector("_Tiling", Tiling);
				}
			}
		}
	}
}