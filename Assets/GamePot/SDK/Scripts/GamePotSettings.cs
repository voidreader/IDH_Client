using UnityEngine;
using System.IO;
using System;

public class GamePotSettings : ScriptableObject {
	private static GamePotSettings instance;


	public static void SetInstance(GamePotSettings settings)
	{
		instance = settings;
	}

	public static GamePotSettings Instance
	{
		get
		{
			if (ReferenceEquals(instance, null))
			{
				instance = CreateInstance<GamePotSettings>();
			}
			return instance;
		}
	}


	private string sdkVersion = "";
	private NUserInfo userInfo = null;

	public static string SdkVersion
	{
		get { return Instance.sdkVersion; }
		set { Instance.sdkVersion = value; }
	}
	public static NUserInfo MemberInfo
	{
		get { return Instance.userInfo; }
		set { Instance.userInfo = value; }
	}

}
