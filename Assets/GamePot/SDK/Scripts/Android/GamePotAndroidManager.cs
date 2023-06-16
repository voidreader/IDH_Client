using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePotAndroidManager : MonoBehaviour {

	public static GamePotAndroidManager instance = null;

	public string facebookAppId;
	public string googleAppId;

	void Awake()
	{
#if UNITY_ANDROID
		if (instance != null)
		{
			Destroy (gameObject);
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
