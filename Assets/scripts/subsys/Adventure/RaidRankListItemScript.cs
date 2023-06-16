using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RaidRankListItemScript : MonoBehaviour
{
    public struct Data
    {
        public long userUID;
        public int rank;
        public int typicalKey;
        public int level;
        public string name;
        public int power;
        public int damage;
        public System.TimeSpan time;

        public Data(long _uid, int _rank, int _lv, string _name, int _typical, int _pwr, int _dmg, int  _sec)
        {
            userUID = _uid;
            rank = _rank;
            level = _lv;
            name = _name;
            typicalKey = _typical;
            power = _pwr;
            damage = _dmg;
            time = new System.TimeSpan(_sec / 3600, (_sec % 3600) / 60, (_sec % 60));
        }
    }

    public static RaidRankListItemScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Adventure/RaidRankListItem", _parent);
        return go.GetComponent<RaidRankListItemScript>();
    }

    [SerializeField] UISprite spBG;
    [SerializeField] UISprite spIcon;
    [SerializeField] GameObject goMine;
    [SerializeField] UILabel lbRank;
    [SerializeField] UILabel lbName;
    [SerializeField] UILabel lbPower;
    [SerializeField] UILabel lbDamage;
    [SerializeField] UILabel lbTime;

    public RaidRankSData data { get; private set; }
    public static RaidRankListItemScript lastActiveItem = null;

    public void Init(RaidRankSData _data)
    {
        data = _data;

        lbRank.text = string.Format("{0:N0}위", _data.RANK);

        lbName.text = string.Format("LV.{0} {1}", _data.USER_LEVEL, _data.USER_NAME);
        lbPower.text = string.Format("팀 전투력: {0:N0}", _data.POWER);

        var unitData = GameCore.Instance.DataMgr.GetUnitData(_data.DELEGATE_ICON);
        if (unitData != null)
            GameCore.Instance.SetUISprite(spIcon, UnitDataMap.GetSmallProfileSpriteKey(unitData.prefabId));
        else
            spIcon.spriteName = "";

        lbDamage.text = string.Format("{0:N0}", _data.DAMAGE);
        lbTime.text = string.Format("{0:00}:{1:00}'{2:00}", (int)_data.TAKE_TIME / 60, (int)_data.TAKE_TIME % 60, (int)((_data.TAKE_TIME%1f)*100));

        if (_data.USER_UID == GameCore.Instance.PlayerDataMgr.PvPData.userUID)
        {
            spBG.spriteName = CommonType.BTN_2_NORMAL;
            goMine.SetActive(true);
        }
        else
        {
            spBG.spriteName = CommonType.BTN_5_NORMAL;
            goMine.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (lastActiveItem == null)
        {
            lastActiveItem = this;
            GameCore.Instance.NetMgr.Req_Raid_TeamInfo(data.USER_UID);
        }
    }
}
