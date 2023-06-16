internal class FarmingDefineMap {
	// Use this for initialization
    public static string GetFarmingDescString(FarmingDataMap farmingData, string color = "[FFFFFF]"){
        string descString = "";
        descString = GameCore.Instance.DataMgr.GetFarmingStringData(farmingData.condStringID);

        switch(farmingData.conditionType){
            case 1: descString = string.Format(descString, color + farmingData.conditionValue + "[-]"); break;
            case 2: descString = string.Format(descString, color + CardDataMap.GetStrType(farmingData.conditionValue) + "[-]"); break;
            case 3: descString = color + descString + "[-]"; break;
            case 4: descString = string.Format(descString, color + farmingData.conditionValue + "[-]"); break;
            case 5: descString = string.Format(descString, color + CardDataMap.GetStrRank(farmingData.conditionValue) + "[-]"); break;
        }
        return descString;
    }

    public static string GetQualify1FarmingDescString(FarmingDataMap farmingData, string color = "[FFFFFF]")
    {
        string descString;
        descString = string.Format("[조건1] " + color + "전투력 {0:N0} 이상", farmingData.powerCondition);

        return descString;
    }
    public static string GetQualify2FarmingDescString(FarmingDataMap farmingData, string color = "[FFFFFF]")
    {
        string descString = "";
        descString = GameCore.Instance.DataMgr.GetFarmingStringData(farmingData.condStringID);

        switch (farmingData.conditionType)
        {
            case 1: descString = string.Format("[조건2] " + color + descString, color + farmingData.conditionValue + "[-]"); break;
            case 2: descString = string.Format("[조건2] " + color + descString, color + CardDataMap.GetStrType(farmingData.conditionValue) + "[-]"); break;
            case 3: descString = "[조건2] " + color + descString + "[-]"; break;
            case 4: descString = string.Format("[조건2] " + color + descString, color + farmingData.conditionValue + "[-]"); break;
            case 5: descString = string.Format("[조건2] " + color + descString, color + CardDataMap.GetStrRank(farmingData.conditionValue) + "[-]"); break;
        }
        return descString;
    }
}
