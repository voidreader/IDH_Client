using System;

public class MailSData : JsonParse
{
    internal long MAIL_UID;
    internal int MAIL_TYPE;
    internal int ITEM_ID;
    internal int ITEM_VALUE;
    internal int ITEM_ENCHANT;
    internal DateTime DELETE_DATE;
    internal string MAIL_DESC;

    internal CardType type;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "MAIL_UID", out MAIL_UID);
        ToParse(_json, "MAIL_TYPE", out MAIL_TYPE);
        ToParse(_json, "ITEM_ID", out ITEM_ID);
        ToParse(_json, "ITEM_VALUE", out ITEM_VALUE);
        ToParse(_json, "ITEM_ENCHANT", out ITEM_ENCHANT);
        ToParse(_json, "DELETE_DATE", out DELETE_DATE);
        ToParse(_json, "MAIL_DESC", out MAIL_DESC);

        if (CommonType.ITEM_DEF_KEY < ITEM_ID && ITEM_ID < CommonType.ITEM_DEF_KEY + 1000000)
        {
            var item = GameCore.Instance.DataMgr.GetItemData(ITEM_ID);
            if (item != null)
                type = item.type;
            else
                type = CardType.Count;
        }
        else
        {
            var unit = GameCore.Instance.DataMgr.GetUnitData(ITEM_ID);
            if (unit != null)
            {
                type = CardType.Character;
            }
        }

        MAIL_DESC = MAIL_DESC.Replace("\\\"", "\"");
        MAIL_DESC = MAIL_DESC.Replace("\\n", "\n");

        return true;
    }
}

