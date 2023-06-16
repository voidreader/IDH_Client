/// <summary>
/// Generic Mono singleton.
/// </summary>
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{

	private static T m_Instance = null;

	internal static T Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;
				if (m_Instance == null)
				{
					m_Instance = new GameObject("Singleton of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
					m_Instance.Init();
				}
			}
			return m_Instance;
		}
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this as T;
			//Init();
		}
	}

	internal virtual void Init() { }


	private void OnApplicationQuit()
	{
		m_Instance = null;
	}
}