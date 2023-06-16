using System.Collections.Generic;
using System.Text;
using System.IO;


/// <summary>
/// CSV 파일을 읽어 데이터를 string[][]으로 들고 있는다.
/// Colum이 가변적이라도 사용 가능하다.
/// 언제든 어디든 참조 가능하다.
/// </summary>
public class CSVReader
{
	List<List<string>> data;
	public string this[int row, int col]
	{ get { return data[row][col]; } }

	public bool IsLoaded { get { return data != null; } }
	public int GetColumnCount(int _r) { return data[_r].Count; }
	public int GetRowCount() { return data.Count; }
	public string[] GetRowArray(int _r) { return data[_r].ToArray(); }

	/// <summary>
	/// 생성자. 동시에 파일을 읽어 초기화 한다.
	/// </summary>
	/// <param name="_path">v파일경로 또는 파일명</param>
	/// <param name="_fromResource">true : 리소시스폴더에서 찾음. false(def) : 절대 경로로 찾음</param>
	public CSVReader(string _file, bool _fromResource)
	{
		NewReadFile(_file, _fromResource);
	}

	public CSVReader(string _csvData)
	{
		NewCsvData(_csvData);

		//UnityEngine.Debug.Log(GetAllText());
	}

	/// <summary>
	/// 파일을 읽어 분할된 string 형태로 저장
	/// </summary>
	/// <param name="_path">파일 경로</param>
	/// <param name="_fromResource">true : 리소시스폴더에서 찾음. false(def) : 절대 경로로 찾음</param>
	public void NewReadFile(string _path, bool _fromResource = false)
	{
		string txt = GetFileText(_path, _fromResource);
		NewCsvData(txt);
#if UNITY_EDITOR
        UnityEngine.Debug.Log("File Name : " + _path + "\n" + GetAllText());
#endif
    }


	public void NewCsvData(string _csvData)
	{
		data = new List<List<string>>();

		bool doubleQuote = false;
		bool bNewRow = true;

		int oldIndex = 0;
		int i = 0;
		while (i < _csvData.Length)
		{
			// Add new Row
			if (bNewRow)
			{
				bNewRow = false;
				data.Add(new List<string>());
			}

			// inside doubleQuote
			if (doubleQuote)
			{
				if (_csvData[i] == '\"')
				{
                    if (_csvData.Length <= i + 1) // 하지만 우리가 만든 CSV 파싱 VBA는 파일의 끝일 수 있다...
                        break;

                    switch (_csvData[i + 1]) // CSV 양식상 doubleQuote 다음이 파일의 끝일 수 없기때문에 무결성 검사 X
                    {
                        case '\"':  // keeping doubleQuote
                            i += 2;               // jump \"\"
                            continue;

                        case ',':   // close doubleQuote & end column
                            data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
                            doubleQuote = false;

                            oldIndex = i = i + 2; // jump \",
                            continue;

                        case '\r':  // close doubleQuote & end row case \r\n
                            data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
                            bNewRow = true;
                            doubleQuote = false;

                            oldIndex = i = i + 3; // jump \"\r\n
                            continue;

                        case '\n':  // close doubleQuote & end row case \n
                            data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
                            bNewRow = true;
                            doubleQuote = false;

                            oldIndex = i = i + 2; // jump \"\n
                            continue;
                    }
				}
			}
			// outside doubleQuote
			else
			{
				switch (_csvData[i])
				{
					case '\"':  // Open doubleQuote
						doubleQuote = true;

						oldIndex = i = i + 1; // jump "Open doubleQuote"
						continue;

					case '\r':  // end row case \r\n
						data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
						bNewRow = true;

						oldIndex = i = i + 2;   // jump \r\n
						continue;

					case '\n': // end row case \n
						data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
						bNewRow = true;

						oldIndex = i = i + 1;   // jump \n
						continue;

					case ',':   // end column
						data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));

						oldIndex = i = i + 1;   // jump ,
						continue;
				}
			}
			++i;
		}

		if (oldIndex <= i)
			data[data.Count - 1].Add(_csvData.Substring(oldIndex, i - oldIndex));
	}




	/// <summary>
	/// 텍스트파일을 일어 문자열을 반환한다.
	/// </summary>
	/// <param name="_path">파일 경로</param>
	/// <param name="_fromResource">파일을 읽어올 위치. true : Resource폴더, false : _psth의 경로(def)</param>
	/// <returns></returns>
	public string GetFileText(string _path, bool _fromResource = false)
	{
		string txt = string.Empty;
		FileStream fs = null;
		try
		{
			// 리소시스 폴더에서 로드
			if (_fromResource)
			{
				var ta = UnityEngine.Resources.Load<UnityEngine.TextAsset>(_path);
                if (ta == null)
                    throw new System.Exception("Not Find Resource File : " + _path);
                
				txt = ta.text;
			}
			// 수동 경로에서 로드
			else
			{
				fs = new FileStream(_path, FileMode.Open);
				if (fs.CanRead)
				{
					byte[] buf = new byte[fs.Length];
					fs.Read(buf, 0, buf.Length);
					txt = Encoding.UTF8.GetString(buf);
				}
				fs.Close();
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
			if (fs != null) fs.Close();

			throw e;
		}


		return txt;
	}


#if UNITY_EDITOR
    /// <summary>
    /// 디버깅용이며, 파일의 모든 텍스트를 반환한다.
    /// </summary>
    /// <returns></returns>
    public string GetAllText()
	{
		if (data == null)
			return "Not Init";

		StringBuilder dsb = new StringBuilder();
		for (int i = 0; i < data.Count; ++i)
		{
			dsb.Append(data[i][0]);
			for (int j = 1; j < data[i].Count; ++j)
			{
				dsb.Append(',');
				dsb.Append(data[i][j]);
			}
			dsb.Append('\n');
		}

		return dsb.ToString();
	}
#endif
}

