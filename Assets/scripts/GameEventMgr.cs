using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 이벤트 핸들링을 관리한다.
/// </summary>
public class GameEventMgr
{
	/// <summary> 이벤트 핸들러 리스트 테이블</summary>
	private Dictionary<GameEventType, List<IEventHandler>> handlerMap;

	private List<GameEvent> asyncEventList;

	/// <summary>
	/// 생성자. 이벤트 핸들링 맵을 초기화 한다.
	/// </summary>
	public GameEventMgr()
	{
		handlerMap = new Dictionary<GameEventType, List<IEventHandler>>();
		asyncEventList = new List<GameEvent>();
	}


	/// <summary>
	/// 이벤트 핸들러 등록
	/// </summary>
	/// <param name="_handler">등록할 이벤트 핸들러</param>
	/// <param name="_eventTypes">핸들러가 등록된 이벤트 타입들</param>
	public void RegisterHandler(IEventHandler _handler, params GameEventType[] _eventTypes)
	{
		for (int i = 0; i < _eventTypes.Length; i++)
			RegisterHandler(_handler, _eventTypes[i]);
	}


	/// <summary>
	/// 이벤트 핸들러 등록
	/// </summary>
	/// <param name="_handler">등록할 이벤트 핸들러</param>
	/// <param name="_eventTypes">핸들러가 등록된 이벤트 타입</param>
	public void RegisterHandler(IEventHandler _handler, GameEventType _eventTypes)
	{
		if (_handler == null)
			return;
		if (!handlerMap.ContainsKey(_eventTypes))
			handlerMap.Add(_eventTypes, new List<IEventHandler>());

		if (!handlerMap[_eventTypes].Contains(_handler))
			handlerMap[_eventTypes].Add(_handler);
	}


	/// <summary>
	/// 이벤트 핸들러 등록 해제(주의사항 ::단일처리 안됨 )
	/// </summary>
	/// <param name="_handler">해제될 핸들러</param>
	public void UnregisterHandler(IEventHandler _handler)
	{
        //구 로직(단일 처리가 안됨)
        var enumeratorHandler = handlerMap.GetEnumerator();

        while (enumeratorHandler.MoveNext())
        {
            List<IEventHandler> list = enumeratorHandler.Current.Value;
            list.Remove(_handler);
        }
    }

    /// <summary>
    /// 이벤트 핸들링
    /// </summary>
    /// <param name="_evt">처리할 이벤트 데이터</param>
    public void SendEvent(GameEvent _evt)
	{
        bool bEventHandle = false;

		List<IEventHandler> handlers = null;

		if (_evt != null && handlerMap.TryGetValue(_evt.EvtType, out handlers))
		{
            for (int index = 0; index < handlers.Count; index++)
                bEventHandle |= handlers[index].HandleMessage(_evt);
        }


#if UNITY_EDITOR
        // for Log
        if ( !bEventHandle )
			Debug.LogError("Debug  :No Working handle Event: " + _evt.EvtType + "\n  Handler Count : " + ((handlers!= null) ? handlers.Count : 0));
#endif
	}

	internal void Update()
	{
		while( asyncEventList.Count != 0 )
		{
			var evt = asyncEventList[asyncEventList.Count - 1];
			asyncEventList.RemoveAt(asyncEventList.Count - 1);
			SendEvent(evt);
		}
	}
}
