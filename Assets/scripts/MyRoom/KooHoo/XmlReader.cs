using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Linq;


public interface IXmlObject
{
    void ReadXmlElement(XElement element);
    void WriteXml(XmlWriter writer);
}

public class XmlElementController
{
    private IEnumerable<XElement> elementList { get; set; }

    public void SetElementList(IEnumerable<XElement> elementList)
    {
        this.elementList = elementList;
    }


}

public static class XmlParser
{
    public delegate T CreateObject<T>();

    public static void ReadDataList<T>(string path, ref List<T> reVal, CreateObject<T> createMethod) where T : IXmlObject
    {
        XDocument xdoc = XDocument.Load(path);

        IEnumerable<XElement> elementList = xdoc.Root.Elements();

        foreach(var element in elementList)
        {
            T temp = createMethod();
            temp.ReadXmlElement(element);
            reVal.Add(temp);
        }
    }

    public static void ReadDataList(string path, out IEnumerable<XElement> elementList)
    {
        XDocument xdoc = XDocument.Load(path);
        elementList = xdoc.Root.Elements();
    }

    public static void ReadDataRoot<T>(string path, ref T reVal) where T : IXmlObject
    {
        XDocument xdoc = XDocument.Load(path);
        reVal.ReadXmlElement(xdoc.Root);
    }

    public static void Write<T>(string path, List<T> xmlObjectList) where T : IXmlObject
    {
        XmlWriter wr = XmlWriter.Create(path);
        wr.WriteStartDocument();
        wr.WriteRaw("\n");
        wr.WriteStartElement("Root");
        wr.WriteRaw("\n");

        foreach (var a in xmlObjectList)
        {
            if (a == null) continue;
            a.WriteXml(wr);
        }

        wr.Close();
    }
}
