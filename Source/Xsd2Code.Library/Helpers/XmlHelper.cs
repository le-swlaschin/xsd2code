//-----------------------------------------------------------------------
// <copyright file="XmlHelper.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Helpers
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper to find pseudo xml tag.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Get value of pseudo xml tag
        /// </summary>
        /// <param name="xmlStream">xml data string</param>
        /// <param name="tag">Tag name in xml</param>
        /// <returns>return tag value</returns>
        public static string ExtractStrFromXML(string xmlStream, string tag)
        {
            int startpos;
            int endpos;
            int lenght;
            string upperData;

            upperData = xmlStream.ToUpper();
            tag = tag.ToUpper();
            startpos = upperData.IndexOf("<" + tag + ">") + 2 + tag.Length;
            endpos = upperData.IndexOf("</" + tag + ">");
            lenght = endpos - startpos;
            if (lenght > 0)
            {
                return xmlStream.Substring(startpos, lenght);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Insert tag in speudo xml string
        /// </summary>
        /// <param name="tag">tag name of pseudo xml</param>
        /// <param name="tagValue">value of tag</param>
        /// <returns>return pseudo xml string</returns>
        public static string InsertXMLFromStr(string tag, string tagValue)
        {
            return string.Format("<{0}>{1}</{0}>", tag, tagValue);
        }
    }
}
