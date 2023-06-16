using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 게임오브젝트 전용 오브젝트풀
/// 게임오브젝트의 활성화 여부로 풀링을 한다.(활성화 : 사용중 , 비활성화 : 비사용중)
/// 한번에 너무 많이 사용되는 형태에는 부적합.(사용된 오브젝트 검사시 전수 검사를 하므로)
/// </summary>
/// <typeparam name="T">MonoBehaviour</typeparam>
class ObjectPool<T> where T : Component
{
	List<T> usingPool;
	List<T> unusingPool;
	GameObject prefab;
	Transform parent;

	internal ObjectPool(GameObject _prefab, Transform _parent)
	{
		usingPool = new List<T>(4);
		unusingPool = new List<T>(4);
		parent = _parent;
		prefab = _prefab;
		ExpandObject(4);
	}

	internal ObjectPool(string _prefabName, Transform _parent)
	{
		usingPool = new List<T>(4);
		unusingPool = new List<T>(4);
		parent = _parent;
		prefab = Resources.Load<GameObject>(_prefabName);
		ExpandObject(4);
	}

	// BringObject
	internal T BringObject()
	{
		if (unusingPool.Count == 0)
			if(UpdateUnusingObject() == 0)
				ExpandObject(GetCount());
		

		var obj = unusingPool[unusingPool.Count - 1];
		unusingPool.RemoveAt(unusingPool.Count - 1);
		usingPool.Add(obj);
		obj.gameObject.SetActive(true);
		return obj;
	}

	// ReturnObject;
	internal void ReturnObject(T _obj)
	{
		_obj.gameObject.SetActive(false);
		usingPool.Remove(_obj);
		unusingPool.Add(_obj);
		_obj.transform.parent = parent;
	}

	internal void ReturnObjectAll()
	{
		for(int i = usingPool.Count-1; 0 <= i; --i)
		{
			var obj = usingPool[i];
			usingPool.RemoveAt(i);

			if (obj == null)
				continue;

			obj.gameObject.SetActive(false);
			unusingPool.Add(obj);
			obj.transform.parent = parent;
		}
	}

	// UpdateUnusingObject;
	private int UpdateUnusingObject()
	{
		int cnt = 0;
		for (int i = 0; i < usingPool.Count; ++i)
		{
			var obj = usingPool[i];
			if( obj == null )
			{
				usingPool.RemoveAt(i--);
			}
			if (!obj.gameObject.activeSelf)
			{
				usingPool.RemoveAt(i--);
				unusingPool.Add(obj);
				++cnt;
			}
		}

		return cnt;
	}

	// GetCount;
	internal int GetCount()
	{
		return unusingPool.Count + usingPool.Count;
	}

	// GetUsingCount;
	internal int GetUsingCount()
	{
		return usingPool.Count;
	}


	// Clear;
	internal void Clear()
	{
		for (int i = 0; i < usingPool.Count; ++i)
			GameObject.Destroy(usingPool[i].gameObject);
		usingPool.Clear();

		for (int i = 0; i < unusingPool.Count; ++i)
			GameObject.Destroy(unusingPool[i].gameObject);
		unusingPool.Clear();
	}

	// ExpandObject;
	private void ExpandObject(int _count)
	{
		for (int i = 0; i < _count; ++i)
		{
			var go = GameObject.Instantiate(prefab, parent);
			go.SetActive(false);
			unusingPool.Add(go.GetComponent<T>());
		}
	}
}
