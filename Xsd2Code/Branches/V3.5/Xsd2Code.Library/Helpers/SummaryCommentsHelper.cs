// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryCommentsHelper.cs" company="Xsd2Code">
//   N/A
// </copyright>
// <summary>
//   Class for summary comments generation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Xsd2Code.Library.Helpers
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Class for summary comments generation.
    /// </summary>
    public class SummaryCommentsHelper
    {
        /// <summary>
        /// Current annotated element holder.
        /// </summary>
        private static XmlSchemaAnnotated currentAnnotated;

        /// <summary>
        /// Creates the summary comment for type declaration from schema.
        /// </summary>
        /// <param name="codeTypeDeclaration">The code type declaration.</param>
        /// <param name="schema">The input XML schema.</param>
        public static void CreateSummaryCommentFromSchema(CodeTypeDeclaration codeTypeDeclaration, XmlSchema schema)
        {
            if (GeneratorContext.GeneratorParams.Miscellaneous.EnableSummaryComment)
            {
                var xmlSchemaElement = SearchElementInSchema(codeTypeDeclaration, schema, new List<XmlSchema>());
                if (xmlSchemaElement != null)
                {
                    currentAnnotated = xmlSchemaElement;
                    if (xmlSchemaElement.Annotation != null)
                    {
                        codeTypeDeclaration.Comments.Clear();
                        foreach (var xmlDoc in xmlSchemaElement.Annotation.Items.OfType<XmlSchemaDocumentation>())
                        {
                            CreateCommentStatement(codeTypeDeclaration.Comments, xmlDoc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the summary comments for enum fields.
        /// </summary>
        /// <param name="type">The code type declaration.</param>
        /// <param name="schema">The input XML schema.</param>
        public static void ProcessEnum(CodeTypeDeclaration type, XmlSchema schema)
        {
            if (type.IsEnum && GeneratorContext.GeneratorParams.Miscellaneous.EnableSummaryComment)
            {
                CreateSummaryCommentFromSchema(type, schema);
                CreateEnumFieldComments(type, currentAnnotated as XmlSchemaSimpleType);
            }
        }

        /// <summary>
        /// Creates the summary comments for property.
        /// </summary>
        /// <param name="member">The property.</param>
        /// <param name="schema">The input XML schema.</param>
        public static void ProcessPropertyComments(CodeTypeMember member, XmlSchema schema)
        {
            if (GeneratorContext.GeneratorParams.Miscellaneous.EnableSummaryComment)
            {
                if (currentAnnotated != null)
                {
                    var xmlElement = currentAnnotated as XmlSchemaElement;
                    var xmlComplexType = xmlElement != null
                                             ? xmlElement.ElementSchemaType as XmlSchemaComplexType
                                             : currentAnnotated as XmlSchemaComplexType;
                    if (xmlComplexType != null)
                    {
                        // Search property in attributes for summary comment generation
                        var xmlAttrib = GetAttribute(
                            xmlComplexType, schema, attribute => member.Name.Equals(attribute.QualifiedName.Name));
                        if (xmlAttrib != null)
                        {
                            CreateCommentFromAnnotation(xmlAttrib.Annotation, member.Comments);
                        }
                        else
                        {
                            // Search property in XmlSchemaElement for summary comment generation
                            var element = SearchForElement(
                                member.Name, xmlComplexType.ContentTypeParticle as XmlSchemaSequence);
                            if (element != null)
                            {
                                CreateCommentFromAnnotation(element.Annotation, member.Comments);
                            }
                            else
                            {
                                if (member.Name == "Value")
                                {
                                    XmlSchemaAnnotation xmlAnnotation = null;
                                    if (xmlComplexType.ContentModel != null)
                                    {
                                        xmlAnnotation = (xmlComplexType.ContentModel.Content != null)
                                                            ? xmlComplexType.ContentModel.Content.Annotation
                                                            : null;
                                        xmlAnnotation = xmlAnnotation ?? xmlComplexType.ContentModel.Annotation;
                                    }

                                    CreateCommentFromAnnotation(xmlAnnotation, member.Comments);
                                }
                                else
                                {
                                    if (member.Name.StartsWith("Item"))
                                    {
                                        if (xmlComplexType.ContentTypeParticle != null)
                                        {
                                            CreateCommentFromAnnotation(
                                                xmlComplexType.ContentTypeParticle.Annotation, member.Comments);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the summary comments for namespace.
        /// </summary>
        /// <param name="code">The code namespace.</param>
        /// <param name="schema">The input XML schema.</param>
        public static void ProcessSchemaComents(CodeNamespace code, XmlSchema schema)
        {
            if (GeneratorContext.GeneratorParams.Miscellaneous.EnableSummaryComment)
            {
                foreach (XmlSchemaDocumentation documentation in
                    schema.Items.OfType<XmlSchemaAnnotation>().SelectMany(
                        annotation => annotation.Items.OfType<XmlSchemaDocumentation>()))
                {
                    CreateCommentStatement(code.Comments, documentation, false);
                }
            }
        }

        /// <summary>
        /// Recursive search of element of complex type.
        /// </summary>
        /// <param name="type">Element to search.</param>
        /// <param name="xmlComplexType">Complex type for search.</param>
        /// <param name="xmlElement">Current element.</param>
        /// <param name="currentElementName">Name of the current element.</param>
        /// <param name="hierarchicalElmtName">The hierarchical element name.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <returns>Found XmlSchemaAnnotated or null value.</returns>
        private static XmlSchemaAnnotated SearchType(CodeTypeDeclaration type, XmlSchemaComplexType xmlComplexType, XmlSchemaElement xmlElement, string currentElementName, string hierarchicalElmtName, XmlSchema schema)
        {
            if (xmlComplexType != null)
            {
                var xmlSequence = xmlComplexType.ContentTypeParticle as XmlSchemaSequence;
                if (xmlSequence != null)
                {
                    var element = ProcessElement(type, xmlSequence.Items, xmlElement, currentElementName, hierarchicalElmtName, schema);
                    if (element != null)
                    {
                        return element;
                    }
                }

                var xmlChoice = xmlComplexType.ContentTypeParticle as XmlSchemaChoice;
                if (xmlChoice != null)
                {
                    var element = ProcessElement(type, xmlChoice.Items, xmlElement, currentElementName, hierarchicalElmtName, schema);
                    if (element != null)
                    {
                        return element;
                    }
                }

                var xmlAttrib = GetAttribute(
                    xmlComplexType,
                    schema,
                    attr =>
                    type.Name.Equals(hierarchicalElmtName + attr.Name, StringComparison.InvariantCultureIgnoreCase)
                    ||
                    ((xmlElement != null) &&
                     type.Name.Equals(hierarchicalElmtName + xmlElement.Name + attr.Name, StringComparison.InvariantCultureIgnoreCase)));
                if (xmlAttrib != null)
                {
                    return xmlAttrib.AttributeSchemaType;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursive search of element.
        /// </summary>
        /// <param name="type">Element to search.</param>
        /// <param name="xmlElement">Current element.</param>
        /// <param name="currentElementName">Name of the current element.</param>
        /// <param name="hierarchicalElmtName">The hierarchical element name.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <returns>Found XmlSchemaAnnotated or null value.</returns>
        private static XmlSchemaAnnotated SearchElement(CodeTypeDeclaration type, XmlSchemaElement xmlElement, string currentElementName, string hierarchicalElmtName, XmlSchema schema)
        {
            var found = false;
            if (type.IsClass)
            {
                if (xmlElement.Name == null)
                {
                    return null;
                }

                if (type.Name.Equals(hierarchicalElmtName + xmlElement.Name, StringComparison.InvariantCultureIgnoreCase) ||
                    type.Name.Equals(xmlElement.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                }
            }
            else
            {
                if (type.Name.Equals(xmlElement.QualifiedName.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                }
            }

            if (found)
            {
                return xmlElement;
            }

            return SearchType(
                type,
                xmlElement.ElementSchemaType as XmlSchemaComplexType,
                xmlElement,
                currentElementName,
                hierarchicalElmtName,
                schema);
        }

        /// <summary>
        /// Gets hierarchical element name.
        /// </summary>
        /// <param name="type">Element to search.</param>
        /// <param name="currentElement">Current element.</param>
        /// <param name="currentHierarchicalName">The current hierarchical element name.</param>
        /// <returns>The hierarchical element name</returns>
        private static string GetHierarchicalName(CodeTypeDeclaration type, XmlSchemaElement currentElement, string currentHierarchicalName)
        {
            if (currentElement == null)
            {
                return currentHierarchicalName;
            }

            if (type.Name.StartsWith(currentHierarchicalName + currentElement.QualifiedName.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return currentHierarchicalName + currentElement.QualifiedName.Name;
            }

            return currentHierarchicalName;
        }

        /// <summary>
        /// Processes sequences or choices of elements.
        /// </summary>
        /// <param name="type">Element to search.</param>
        /// <param name="xmlObjectCollection">Item collection of sequence or chioce.</param>
        /// <param name="currentElement">Current element.</param>
        /// <param name="currentElementName">Name of the current element.</param>
        /// <param name="hierarchicalElmtName">The hierarchical element name.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <returns>Found XmlSchemaAnnotated or null value.</returns>
        private static XmlSchemaAnnotated ProcessElement(CodeTypeDeclaration type, XmlSchemaObjectCollection xmlObjectCollection, XmlSchemaElement currentElement, string currentElementName, string hierarchicalElmtName, XmlSchema schema)
        {
            return xmlObjectCollection.OfType<XmlSchemaElement>().Select(
                current =>
                ((hierarchicalElmtName != current.QualifiedName.Name)
                 && (currentElementName != current.QualifiedName.Name))
                    ? SearchElement(
                        type,
                        current,
                        (currentElement != null) ? currentElement.QualifiedName.Name : string.Empty,
                        GetHierarchicalName(type, currentElement, hierarchicalElmtName),
                        schema)
                    : null).FirstOrDefault(current => current != null)

                   ?? xmlObjectCollection.OfType<XmlSchemaChoice>().Select(
                       currChoice =>
                       ProcessElement(type, currChoice.Items, currentElement, currentElementName, hierarchicalElmtName, schema))
                          .FirstOrDefault(currElement => currElement != null);
        }

        /// <summary>
        /// Create CodeCommentStatement from schema documentation.
        /// </summary>
        /// <param name="codeStatementColl">CodeCommentStatementCollection collection.</param>
        /// <param name="xmlDoc">Schema documentation.</param>
        /// <param name="docComment">Indicates documentation comment.</param>
        private static void CreateCommentStatement(CodeCommentStatementCollection codeStatementColl, XmlSchemaDocumentation xmlDoc, bool docComment = true)
        {
            foreach (var textLine in xmlDoc.Markup.Select(itemDoc => itemDoc.InnerText.Trim()).Where(textLine => textLine.Length > 0))
            {
                CodeDomHelper.CreateSummaryComment(codeStatementColl, textLine, docComment);
            }
        }

        /// <summary>
        /// Search for element in sequence.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>Found element or null value.</returns>
        private static XmlSchemaElement SearchForElement(string name, XmlSchemaSequence sequence)
        {
            if (sequence == null)
            {
                return null;
            }

            XmlSchemaElement element =
                sequence.Items.OfType<XmlSchemaElement>().FirstOrDefault(item => name.Equals(item.QualifiedName.Name));
            return element
                   ?? sequence.Items.OfType<XmlSchemaSequence>().Select(seq => SearchForElement(name, seq)).FirstOrDefault();
        }

        /// <summary>
        /// Search for attribute group in schema.
        /// </summary>
        /// <param name="name">The attribute group name.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <returns>Found attribute group or null value.</returns>
        private static XmlSchemaAttributeGroup SearchForAttributeGroup(XmlQualifiedName name, XmlSchema schema)
        {
            return schema.Items.OfType<XmlSchemaAttributeGroup>().FirstOrDefault(group => group.QualifiedName == name) ??
                   schema.Includes.OfType<XmlSchemaInclude>().Select(
                       include => SearchForAttributeGroup(name, include.Schema)).FirstOrDefault(group => group != null);
        }

        /// <summary>
        /// Search for attribute in attribute collection.
        /// </summary>
        /// <param name="collection">The attribute collection.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>Found attribute or null value.</returns>
        private static XmlSchemaAttribute SearchForAttribute(XmlSchemaObjectCollection collection, XmlSchema schema, Func<XmlSchemaAttribute, bool> predicate)
        {
            var xmlAttrib = collection.OfType<XmlSchemaAttribute>().FirstOrDefault(predicate);
            if (xmlAttrib != null)
            {
                return xmlAttrib;
            }

            return
                collection.OfType<XmlSchemaAttributeGroupRef>().Select(
                    current => SearchForAttributeGroup(current.RefName, schema)).Where(group => group != null).Select(
                        group => SearchForAttribute(group.Attributes, schema, predicate)).FirstOrDefault();
        }

        /// <summary>
        /// Gets attribute from complex type.
        /// </summary>
        /// <param name="xmlComplexType">The complex type.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>Found attribute or null value.</returns>
        private static XmlSchemaAttribute GetAttribute(XmlSchemaComplexType xmlComplexType, XmlSchema schema, Func<XmlSchemaAttribute, bool> predicate)
        {
            XmlSchemaAttribute xmlAttrib = null;
            if (xmlComplexType != null)
            {
                var xmlContent = xmlComplexType.ContentModel != null
                                     ? xmlComplexType.ContentModel.Content
                                     : null;

                // Search property in attributes for summary comment generation
                xmlAttrib = SearchForAttribute(xmlComplexType.Attributes, schema, predicate);

                if (xmlAttrib == null)
                {
                    var extension = xmlContent as XmlSchemaSimpleContentExtension;
                    xmlAttrib = extension != null
                                    ? extension.Attributes.OfType<XmlSchemaAttribute>().FirstOrDefault(predicate)
                                    : null;
                }

                if (xmlAttrib == null)
                {
                    var tmp = xmlContent as XmlSchemaSimpleContentRestriction;
                    xmlAttrib = tmp != null
                                    ? tmp.Attributes.OfType<XmlSchemaAttribute>().FirstOrDefault(predicate)
                                    : null;
                }

                if (xmlAttrib == null)
                {
                    var tmp = xmlContent as XmlSchemaComplexContentExtension;
                    xmlAttrib = tmp != null
                                    ? tmp.Attributes.OfType<XmlSchemaAttribute>().FirstOrDefault(predicate)
                                    : null;
                }

                if (xmlAttrib == null)
                {
                    var tmp = xmlContent as XmlSchemaComplexContentRestriction;
                    xmlAttrib = tmp != null
                                    ? tmp.Attributes.OfType<XmlSchemaAttribute>().FirstOrDefault(predicate)
                                    : null;
                }
            }

            return xmlAttrib;
        }

        /// <summary>
        /// Generate summary comment from XmlSchemaAnnotation.
        /// </summary>
        /// <param name="xmlSchemaAnnotation">XmlSchemaAnnotation from XmlSchemaElement or XmlSchemaAttribute.</param>
        /// <param name="codeCommentStatementCollection">codeCommentStatementCollection from member.</param>
        private static void CreateCommentFromAnnotation(XmlSchemaAnnotation xmlSchemaAnnotation, CodeCommentStatementCollection codeCommentStatementCollection)
        {
            if (xmlSchemaAnnotation != null)
            {
                foreach (var xmlDoc in xmlSchemaAnnotation.Items.OfType<XmlSchemaDocumentation>())
                {
                    CreateCommentStatement(codeCommentStatementCollection, xmlDoc);
                }
            }
        }

        /// <summary>
        /// Search XmlElement in schema.
        /// </summary>
        /// <param name="codeTypeDeclaration">Represents a type declaration for a class, structure, interface, or enumeration.</param>
        /// <param name="schema">The input XML schema.</param>
        /// <param name="visitedSchemas">The visited schemas.</param>
        /// <returns>Found XmlSchemaElement or null value.</returns>
        private static XmlSchemaAnnotated SearchElementInSchema(CodeTypeDeclaration codeTypeDeclaration, XmlSchema schema, List<XmlSchema> visitedSchemas)
        {
            var type = schema.Items.OfType<XmlSchemaType>().FirstOrDefault(p => codeTypeDeclaration.Name.Equals(p.Name));
            if (type != null)
            {
                return type;
            }

            var complexType = schema.Items.OfType<XmlSchemaComplexType>().FirstOrDefault(
                p => codeTypeDeclaration.Name.StartsWith(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (complexType != null)
            {
                var anonymousType = SearchType(codeTypeDeclaration, complexType, null, string.Empty, complexType.Name, schema);
                if (anonymousType != null)
                {
                    return anonymousType;
                }
            }

            var xmlSubElement = ProcessElement(codeTypeDeclaration, schema.Items, null, string.Empty, string.Empty, schema);
            if (xmlSubElement != null)
            {
                return xmlSubElement;
            }

            // If not found search in schema inclusion
            foreach (var schemaInc in schema.Includes.OfType<XmlSchemaObject>().Select(item => item as XmlSchemaInclude)
                .Where(schemaInc => (schemaInc != null) && !visitedSchemas.Exists(loc => schemaInc.Schema == loc)))
            {
                visitedSchemas.Add(schemaInc.Schema);
                var includeElmts = SearchElementInSchema(codeTypeDeclaration, schemaInc.Schema, visitedSchemas);
                visitedSchemas.Remove(schemaInc.Schema);
                if (includeElmts != null)
                {
                    return includeElmts;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates enum fields comments.
        /// </summary>
        /// <param name="type">The type declaration.</param>
        /// <param name="xmlSimpleType">The xml simple type for enum.</param>
        private static void CreateEnumFieldComments(CodeTypeDeclaration type, XmlSchemaSimpleType xmlSimpleType)
        {
            if (xmlSimpleType != null)
            {
                var xmlSimpleContentRestriction = xmlSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                if (xmlSimpleContentRestriction != null)
                {
                    foreach (var field in type.Members.OfType<CodeMemberField>())
                    {
                        field.Comments.Clear();
                        var facet =
                            xmlSimpleContentRestriction.Facets.OfType<XmlSchemaFacet>().FirstOrDefault(
                                currFacet => currFacet.Value.Equals(field.Name));
                        if (facet != null)
                        {
                            CreateCommentFromAnnotation(facet.Annotation, field.Comments);
                        }
                    }
                }
            }
        }
    }
}
