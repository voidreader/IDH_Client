internal class ChrChemyDataMap : CSVParse {
    internal string name;
    internal string stat;
    internal string ef;
    internal override int SetData(string[] _csvData){
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out name);
        ToParse(_csvData[idx++], out stat);
        ToParse(_csvData[idx++], out ef);
        return id;
    }
}
