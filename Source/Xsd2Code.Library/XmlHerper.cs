using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace Xsd2Code
{
    public static class XmlHerper
    {
        public static string ExtractStrFromXML(string sXMLStream, string sSearchText)
        {
            int iStartPos;
            int iEndPos;
            int iLength;
            string UpperData;

            UpperData = sXMLStream.ToUpper();
            sSearchText = sSearchText.ToUpper();
            iStartPos = UpperData.IndexOf("<" + sSearchText + ">") + 2 + sSearchText.Length;
            iEndPos = UpperData.IndexOf("</" + sSearchText + ">");
            iLength = iEndPos - iStartPos;
            if (iLength > 0)
                return sXMLStream.Substring(iStartPos, iLength);
            else
                return "";
        }

        public static string InsertXMLFromStr(string TagName, string Value)
        {
            return string.Format("<{0}>{1}</{0}>", TagName, Value);
        }
    }
}
