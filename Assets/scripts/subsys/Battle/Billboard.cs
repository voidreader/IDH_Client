using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	Transform cachedTf;
	Transform camTf;
	public bool update;
	public void Awake()
	{
		cachedTf = transform;
		//camTf = GameObject.Find("_world").transform;

        if(GameCore.Instance != null  )
        {
            if( GameCore.Instance.GetWorldCam())
            {
                camTf = GameCore.Instance.GetWorldCam().transform;
                cachedTf.rotation = camTf.rotation;
            }
        }

		enabled = update;
	}

	void LateUpdate()
	{
		cachedTf.rotation = camTf.rotation;
	}
}
