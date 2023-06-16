using UnityEngine;


public abstract class SingletonInst<T> : MonoBehaviour where T : SingletonInst<T>
{
	private static T _Instance = null;

	public static T Instance
	{
		get
		{
			if (null == _Instance)
			{
				_Instance = GameObject.FindObjectOfType(typeof(T)) as T;
				if (null == _Instance)
				{
					_Instance = new GameObject("Singleton_" + typeof(T).ToString(), typeof(T)).GetComponent<T>();
					//_Instance.Init();

					DontDestroyOnLoad(_Instance);
				}
			}

			return _Instance;
		}
	}

	virtual protected void Awake()
	{
		if (null == _Instance)
		{
			_Instance = this as T;
			_Instance.Init();
		}
	}

	virtual protected void OnDestroy()
	{
		if (null != _Instance)
		{
			_Instance.Clear();
			_Instance = null;
		}
	}

	private void OnApplicationQuit()
	{
		_Instance = null;
	}

	public virtual void Init() { }
	public virtual void Clear() { }
}

