using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterFamily
{
	public class UIPopupToastMsg : UIBase
	{
		[SerializeField] UITweener m_Tween = null;
		[SerializeField] UILabel m_txtMessage = null;

		public override void Initialize()
		{
			base.Initialize();
		}

		public override void Open()
		{
			base.Open();

			m_Tween.ResetToBeginning();
			m_Tween.PlayForward();
		}

		public override void Close()
		{
			base.Close();
		}

		public void SetMessage(string strMsg)
		{
			m_txtMessage.text = strMsg;

			Open();
		}

        public void SetMessage(int nIdx, object arg)
        {
            //m_txtMessage.text = string.Format(TableManager.GetString(nIdx), arg);
            Open();
        }
    }
}
