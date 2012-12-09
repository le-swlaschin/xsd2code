using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Xsd2Code.Library.Helpers
{
    public class SchemaHelper
    {
        public static bool IsIncludedType(XmlSchemaObjectCollection schemas, string name)
        {
            foreach (var xmlSchema in schemas.Cast<XmlSchemaInclude>())
            {
                foreach (XmlSchemaObject item in xmlSchema.Schema.Items)
                {
                    var complexItem = item as XmlSchemaComplexType;
                    if (complexItem != null)
                    {
                        if (complexItem.Name == name)
                        {
                            return true;
                        }
                    }
                    var elementItem = item as XmlSchemaElement;
                    if (elementItem != null)
                    {
                        if (elementItem.Name == name)
                        {
                            return true;
                        }
                    }
                    var simpleItem = item as XmlSchemaSimpleType;
                    if (simpleItem != null)
                    {
                        if (simpleItem.Name == name)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
