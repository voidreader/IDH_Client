using UnityEngine;
using System.Collections;
//using for DllImport
using System.Runtime.InteropServices;


public class GamePotiOSManager : MonoBehaviour {

	public static GamePotiOSManager instance = null;
	public string facebookAppId;

	void Awake()
	{
#if UNITY_IOS
		if(instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			gameObject.AddComponent<GamePotEventListener> ();
			DontDestroyOnLoad (gameObject);
		}
#endif
	}
}

