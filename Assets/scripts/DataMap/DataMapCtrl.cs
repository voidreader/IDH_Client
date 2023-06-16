using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV 데이터를 파싱하여 저장될 수 있는 인터페이스
/// </summary>
abstract class CSVParse
{
	internal int id = -1;

	/// <summary>
	/// CSV 데이터를 받아 파싱하여 저장한다.
	/// </summary>
	/// <param name="_csvData">CSV파일의 한 행으로, ','로 프슬릿되어있음</param>
	/// <returns>ID를 반환. 0 이하 일때 실패</returns>
	internal abstract int SetData(string[] _csvData);

	internal static void ToParse(string _str, out int _out)
	{
		if (!int.TryParse(_str, out _out))
			_out = -1;
	}
	internal static void ToParse(string _str, out float _out)
	{
		if (!float.TryParse(_str, out _out))
			_out = -1;
	}
	internal static void ToParse(string _str, out bool _out)
	{
		_out = _str != "0";
	}
	internal static void ToParse(string _str, out string _out)
	{
		_out = _str;
	}
    internal static void ToParse(string _str, out DateTime _out)
    {
        _out = DateTime.Parse(_str);
    }
    public static void ToParse(string _str, out AttackType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value < (int)AttackType.Count)
			_out = (AttackType)value;
		else
			_out = AttackType.None;
	}
	public static void ToParse(string _str, out EffectType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value < (int)EffectType.Count)
			_out = (EffectType)value;
		else
			_out = EffectType.None;
    }

    public static void ToParse(string _str, out TargetType _out)
    {
        int value;
        if (int.TryParse(_str, out value) && 0 <= value && value < (int)TargetType.Count)
            _out = (TargetType)value;
        else
            _out = TargetType.SingleEnemy;
    }

    public static void ToParse(string _str, out UnitStat _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value < (int)UnitStat.Count)
			_out = (UnitStat)value;
		else
			_out = UnitStat.None;
	}

	public static void ToParse(string _str, out CardType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value <= (int)CardType.Count)
			_out = (CardType)value;
		else
			_out = CardType.Count;
	}

	public static void ToParse(string _str, out ItemSubType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value <= (int)ItemSubType.Count)
			_out = (ItemSubType)value;
		else
			_out = ItemSubType.Count;
	}

	public static void ToParse(string _str, out ItemEffectType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value <= (int)ItemEffectType.Count)
			_out = (ItemEffectType)value;
		else
			_out = ItemEffectType.Count;
	}
	
	public static void ToParse(string _str, out MyRoomEffectType _out)
	{
		int value;
		if (int.TryParse(_str, out value) && 0 <= value && value <= (int)MyRoomEffectType.Count)
			_out = (MyRoomEffectType)value;
		else
			_out = MyRoomEffectType.Count;
	}

    public static void ToParse(string _str, out MissionDataMap.Type _out)
    {
        int value;
        if (int.TryParse(_str, out value) && 0 <= value && value <= (int)MissionDataMap.Type.Count)
            _out = (MissionDataMap.Type)value;
        else
            _out = MissionDataMap.Type.Count;
    }

    public static void ToParse(string _str, out MissionType _out)
    {
        int value;
        if (int.TryParse(_str, out value) && 0 <= value && value <= (int)MissionType.Count)
            _out = (MissionType)value;
        else
            _out = MissionType.Count;
    }
    public static void ToParse(string _str, out ACheckType _out)
    {
        int value;
        if (int.TryParse(_str, out value) && 0 <= value && value <= (int)ACheckType.Count)
            _out = (ACheckType)value;
        else
            _out = ACheckType.Count;
    }
}


/// <summary>
/// 모든 종류의 데이터맵을 1대1로 관리하는 컨트롤러
/// </summary>
/// <typeparam name="DataMap"></typeparam>
internal class DataMapCtrl<DataMap>  where DataMap : CSVParse, new()
{
	Dictionary<int, DataMap> dataMap;
    string path = string.Empty;


	/// <summary>
	/// CSV 데이터를 읽어들여 모든 행데이터를 _target에 파싱하여 저장한다.(리스트로 저장되도록 설계됨)
	/// </summary>
	/// <param name="_type">CSV DataMap 타입</param>
	internal DataMapCtrl(DataMapType _type)
        : this(_type, FilePath.dataMapPath[_type])
	{
    }

    internal DataMapCtrl(DataMapType _type, string _fileName)
    {
//#if !UNITY_EDITOR
//        path = System.IO.Path.Combine(System.IO.Path.Combine(Application.streamingAssetsPath,"Table"), _fileName);
//        if (GameCore.Instance.bLogLoadTable)
//            Debug.Log("ReadDataMap :" + _type + " (" + path + ")");

//        // 파일 읽기

//        //CSVReader csv = new CSVReader(path, false);
//        WWW www = new WWW(path);
//        while (!www.isDone) ;

//        if (www.error != null)
//        {
//            Debug.LogError(_fileName + " DataMap Read Fail. " + www.error);
//            return;
//        }

//        //Debug.Log(path + "\n" + www.text);
//        CSVReader csv = new CSVReader(www.text);
//        Init(csv);
//#else
        path = _fileName;
        var enumerator = GameCore.Instance.ResourceMgr.AbMgr.LoadAssetBundle<TextAsset>("datatable", ABType.AB_CSV, path, false, (obj) =>
        {
            //Debug.Log("path : " + path + "  obj : " + obj.obj.Length);
            CSVReader csv = new CSVReader(((TextAsset)obj.obj[0]).text);
            Init(csv);
        });

        while (enumerator.MoveNext()) ;
//#endif
    }

    public void Init(CSVReader csv)
    {
        if (!csv.IsLoaded)
            return;

        dataMap = new Dictionary<int, DataMap>();

        // 타겟에게 행별로 데이터 추가
        for (int i = 0; i < csv.GetRowCount(); ++i)
            if (!AddData(csv.GetRowArray(i)))
                Debug.LogWarning(path + " " + i + " Line. Import Failed.");
    }

    internal virtual bool AddData(string[] _csvData)
	{
		DataMap data = new DataMap();
		int id = data.SetData(_csvData);

		if (id <= 0)
		{
			Debug.LogWarning("잘못된 아이디 " + id);
            // AddData() 안에서 에러로그 처리 Debug.LogWarning("실패");
            return false;
		}

		if (dataMap.ContainsKey(id))
		{
			Debug.LogWarning(id + " 데이터는 이미 존재합니다.");
			return false;
		}

		dataMap.Add(id,data);
		return true;
	}

	internal virtual DataMap GetData(int _id)
	{
		if (_id == -1)
			return null;

		if (!IsHave(_id))
		{
			Debug.LogWarning(_id + "에 해당하는 데이터가 존재하지 않습니다." + typeof(DataMap) );
			return null;
		}
		return dataMap[_id];
	}

    internal bool IsHave(int _id)
    {
        return dataMap.ContainsKey(_id);
    }

	internal void RemoveData(int _id)
	{
		if (dataMap.ContainsKey(_id))
			dataMap.Remove(_id);
	}

	internal Dictionary<int, DataMap>.Enumerator GetEnumerator()
	{
		return dataMap.GetEnumerator();
	}

    internal int GetCount()
    {
        return dataMap.Count;
    }

    internal List<DataMap> GetList()
    {
        List<DataMap> list = new List<DataMap>();
        var iter = dataMap.GetEnumerator();
        while (iter.MoveNext())
            list.Add(iter.Current.Value);

        return list;
    }
}


internal class DataMapListCtrl<DataMap> where DataMap : CSVParse, new()
{
    Dictionary<int, List<DataMap>> dataMapList;
    List<DataMap> saveList;
    string path = string.Empty;


    /// <summary>
    /// CSV 데이터를 읽어들여 모든 행데이터를 _target에 파싱하여 저장한다.(리스트로 저장되도록 설계됨)
    /// </summary>
    /// <param name="_type">CSV DataMap 타입</param>
    internal DataMapListCtrl(DataMapType _type)
        : this(_type, FilePath.dataMapPath[_type])
    {
    }

    internal DataMapListCtrl(DataMapType _type, string _fileName)
    {

//#if UNITY_EDITOR
//        var path = System.IO.Path.Combine(System.IO.Path.Combine(Application.streamingAssetsPath, "Table"), _fileName);
//        if (GameCore.Instance.bLogLoadTable)
//            Debug.Log("ReadDataMap :" + _type + " (" + path + ")");

//        //CSVReader csv = new CSVReader(path, false);
//        WWW www = new WWW(path);
//        while (!www.isDone) ;

//        if (www.error != null)
//        {
//            Debug.LogError("DataMap Read Fail. " + www.error);
//            return;
//        }

//        //Debug.Log(path + "\n" + www.text);
//        CSVReader csv = new CSVReader(www.text);
//        Init(csv);
//#else
        path = _fileName;
        var enumerator = GameCore.Instance.ResourceMgr.AbMgr.LoadAssetBundle<TextAsset>("datatable", ABType.AB_CSV, path, false, (obj) =>
        {
            CSVReader csv = new CSVReader(((TextAsset)obj.obj[0]).text);
            Init(csv);
        });

        while (enumerator.MoveNext()) ;
//#endif
    }


    public void Init(CSVReader csv)
    {
        if (!csv.IsLoaded)
            return;

        dataMapList = new Dictionary<int, List<DataMap>>();
        saveList = new List<DataMap>();
        // 타겟에게 행별로 데이터 추가
        for (int i = 0; i < csv.GetRowCount(); ++i)
            if (!AddData(csv.GetRowArray(i)))
                Debug.LogError(path + " " + i + " Line. Import Failed.");

        dataMapList.Add(saveList[0].id, saveList);
    }

    internal virtual bool AddData(string[] _csvData)
    {
        DataMap data = new DataMap();
        int id = data.SetData(_csvData);

        if (id <= 0)
        {
            Debug.LogError("잘못된 아이디 " + id);
            // AddData() 안에서 에러로그 처리 Debug.LogError("실패");
            return false;
        }

        if (saveList.Count > 0)
            if (saveList[0].id != data.id)
            {
                //Debug.LogError(id + " 리스트 교체");
                dataMapList.Add(saveList[0].id, saveList);
                saveList = new List<DataMap>();
                saveList.Add(data);
                return true;
            }

        saveList.Add(data);
        return true;
    }

    internal virtual List<DataMap> GetData(int _id)
    {
        if (_id == -1)
            return null;

        if (!dataMapList.ContainsKey(_id))
        {
            Debug.LogError(_id + "에 해당하는 데이터가 존재하지 않습니다." + typeof(DataMap));
            return null;
        }
        return dataMapList[_id];
    }

    internal void RemoveData(int _id)
    {
        if (dataMapList.ContainsKey(_id))
            dataMapList.Remove(_id);
    }

    internal Dictionary<int, List<DataMap>>.Enumerator GetEnumerator()
    {
        return dataMapList.GetEnumerator();
    }
}