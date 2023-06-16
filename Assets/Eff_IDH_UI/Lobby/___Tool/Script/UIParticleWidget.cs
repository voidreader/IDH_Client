using UnityEngine;
using System.Collections.Generic;

public class UIParticleWidget : UIWidget
{
	[HideInInspector] Renderer mRenderer;
	[HideInInspector] int renderQueue = -1;
    private Material cachedMat;

    public Renderer cachedRenderer
    {
        get
        {
            if( mRenderer == null )
            {
                mRenderer = GetComponent<Renderer>();
            }

            return mRenderer;
        }
    }
	
	/// <summary>
	/// Material used by Renderer.
	/// </summary>
	public override Material material
    {
		get
        {
            if( mMat == null )
            {
                mMat = cachedRenderer.sharedMaterial;
                cachedRenderer.material = mMat;
            }

            return mMat;
        }
	}

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if( drawCall != null )
        {
            if( Application.isPlaying == true && cachedMat == null )
            {
                cachedMat = new Material( mMat );
                mMat = cachedMat;
                cachedRenderer.material = mMat;
            }

            if( renderQueue != drawCall.finalRenderQueue )
            {
                renderQueue = drawCall.finalRenderQueue;
                mMat.renderQueue = renderQueue;
            }
        }
    }

    /// <summary>
    /// Dammy Mesh
    /// </summary>
    public override void OnFill( List<Vector3> verts, List<Vector2> uvs, List<Color> cols )
    {

        verts.Add( new Vector3( 10000f, 10000f ) );
        verts.Add( new Vector3( 10000f, 10000f ) );
        verts.Add( new Vector3( 10000f, 10000f ) );
        verts.Add( new Vector3( 10000f, 10000f ) );

        uvs.Add( new Vector2( 0f, 0f ) );
        uvs.Add( new Vector2( 0f, 1f ) );
        uvs.Add( new Vector2( 1f, 1f ) );
        uvs.Add( new Vector2( 1f, 0f ) );

        cols.Add( color );
        cols.Add( color );
        cols.Add( color );
        cols.Add( color );
    }
}
