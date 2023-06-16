using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class JsonParse
{
	internal abstract bool SetData(JSONObject _json);


	internal static void ToParse(JSONObject _json, string _field, out int _result)
	{
		int _tmp = default(int);
		_json.GetField(ref _tmp, _field);
		_result = _tmp;
	}
	internal static void ToParse(JSONObject _json, string _field, out long _result)
	{
		long _tmp = default(long);
		_json.GetField(ref _tmp, _field);
		_result = _tmp;
	}
	internal static void ToParse(JSONObject _json, string _field, out float _result)
	{
		float _tmp = default(float);
		_json.GetField(ref _tmp, _field);
		_result = _tmp;
	}

    internal static void ToParse(JSONObject _json, string _field, out float[] _result)
    {
        float _tmp = default(float);
        var floatsData = _json.GetField(_field);
        if (floatsData == null) { _result = null;  return; }
        _result = new float[floatsData.Count];
        for (int i = 0; i < floatsData.Count; i++)
        {
            _result[i] = float.Parse(floatsData.list[i].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        }
        //_result = _tmp;
    }
    internal static void ToParse(JSONObject _json, string _field, out bool _result)
	{
		int _tmp = default(int);
		_json.GetField(ref _tmp, _field);
		_result = _tmp != 0;
	}
	internal static void ToParse(JSONObject _json, string _field, out string _result)
	{
		string _tmp = null;
		_json.GetField(ref _tmp, _field);
		_result = _tmp;
	}
	internal static void ToParse(JSONObject _json, string _field, out DateTime _result)
	{
		string _tmp = null;
		_json.GetField(ref _tmp, _field);
		if (_tmp != null)		_result = DateTime.Parse(_tmp);
		else								_result = default(DateTime);
	}

	internal static void ToParse(JSONObject _json, string _field, out Vector2 _result)
	{
		float x = -1, y = -1;
		var vec = _json.GetField(_field);
		vec.GetField(ref x, "x");
		vec.GetField(ref y, "y");

		_result = new Vector2(x, y);
	}

	internal static void ToParse(JSONObject _json, string _field, out Vector2[] _result)
	{
		float x = -1, y = -1;
		var position = _json.GetField(_field);
		_result = new Vector2[position.Count];
		for (int i = 0; i < position.Count; ++i)
		{
			var vec = position[i];
			vec.GetField(ref x, "x");
			vec.GetField(ref y, "y");
			_result[i] = new Vector2(x, y);
		}
	}


	internal static void ToParse(JSONObject _json, string _field, out MakeSlotScript.MakeState _result)
	{
		int tmp = -1;
		_json.GetField(ref tmp, _field);
		_result = (MakeSlotScript.MakeState)tmp;
	}

	internal static void ToParse(JSONObject _json, string _field, out MyRoomHistorySData.MRHType _result)
	{
		int tmp = -1;
		_json.GetField(ref tmp, _field);
		_result = (MyRoomHistorySData.MRHType)tmp;
	}

    
    internal static void ToParse(JSONObject _json, string _field, out MyRoomEffectType _result)
    {
        int tmp = -1;
        _json.GetField(ref tmp, _field);
        _result = (MyRoomEffectType)tmp;
    }
}

