using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LogComponent : MonoBehaviour
{
	public UILabel lbHead;
	public UILabel lbBody;
	public UILabel lbTime;

	public UIButton btn;
	public UILabel lbBtn;

	Action<int> cbBtn;

	public static LogComponent Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("MyRoom/LogComponent", _parent);
		return go.GetComponent<LogComponent>();
	}

	internal void SetData(MyRoomHistorySData _data, Action<int> _cbBtn)
	{
		cbBtn = _cbBtn;
		btn.onClick.Clear();

		switch (_data.TYPE)
		{
			case MyRoomHistorySData.MRHType.PvP:
				if (_data.SUCCESS)	lbHead.text = "[00F0FFFF]방어 성공[-]";
				else								lbHead.text = "[FF0000FF]방어 실패[-]";
				lbBody.text = string.Format("{0} 님[898989FF]이 당신을 공격했습니다.", _data.ATTACK_USER_NAME);

				switch(_data.REVENGE)
				{
					case 0:
						btn.defaultColor = new Color32(0x7E, 0x00, 0xFF, 0xFF);
						btn.enabled = true;
						lbBtn.text = "전투";
						btn.onClick.Add(new EventDelegate(() => cbBtn(_data.HISTORY_UID)));
						break;

					case 1:
						btn.defaultColor = new Color32(0x80, 0x80, 0x80, 0xFF);
						btn.enabled = false;
						lbBtn.text = "[FF0000FF]복수 실패[-]";
						break;

					case 2:
						btn.defaultColor = new Color32(0x80, 0x80, 0x80, 0xFF);
						btn.enabled = false;
						lbBtn.text = "[00F0FFFF]복수 성공[-]";
						break;
				}
				break;

			case MyRoomHistorySData.MRHType.Clean:
				lbHead.text = "[24FF00FF]청소 도움[-]";
				lbBody.text = string.Format("{0} 님[898989FF]이 숙소?번을 청소했습니다.", _data.ATTACK_USER_NAME);
				btn.defaultColor = new Color32(0x01, 0x9E, 0x59, 0xFF);
				btn.enabled = true;
				lbBtn.text = "방문";
				btn.onClick.Add(new EventDelegate(() => cbBtn(_data.HISTORY_UID)));
				break;
			default:
				lbHead.text = "알 수 없는 데이터";
				lbBody.text = _data.TYPE.ToString();
				break;
		}

		var t = _data.CREATE_TIME;
		lbTime.text = string.Format("{0}.{1:00}.{2:00}. {3}:{4}", t.Year, t.Month, t.Day, t.Hour, t.Minute);
	}


	
}
