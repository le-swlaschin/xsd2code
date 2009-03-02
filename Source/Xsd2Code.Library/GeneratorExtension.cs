using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Xsd2Code.Library.Helpers;

namespace Xsd2Code.Library
{
    /// <summary>
    /// Convertion des array properties en collection
    /// </summary>
    public class GeneratorExtension : ICodeExtension
    {
        /// <summary>
        /// Sorted list for custom collection
        /// </summary>
        private static readonly SortedList<string, string> collectionTypesField = new SortedList<string, string>();

        #region ICodeExtension Members

        /// <summary>
        /// Process method for cs or vb CodeDom generation
        /// </summary>
        /// <param name="code">CodeNamespace generated</param>
        /// <param name="schema">XmlSchema to generate</param>
        public void Process(CodeNamespace code, XmlSchema schema)
        {
            collectionTypesField.Clear();

            var types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            foreach (var type in types)
            {
                if (!type.IsClass && !type.IsStruct) continue;

                bool addedToConstructor = false;
                bool newCTor = false;

                var ctor = GetConstructor(type, ref newCTor);

                // Generate WCF DataContract
                if (GeneratorContext.GeneratorParams.GenerateDataContracts)
                {
                    CodeTypeReference attrib = new CodeTypeReference("System.Runtime.Serialization.DataContractAttribute");
                    var codeAttributeArgument = new List<CodeAttributeArgument>();
                    codeAttributeArgument.Add(new CodeAttributeArgument("Name", new CodeSnippetExpression(string.Concat('"', type.Name, '"'))));
                    if (!string.IsNullOrEmpty(schema.TargetNamespace))
                    {
                        codeAttributeArgument.Add(new CodeAttributeArgument("NameSpace", new CodeSnippetExpression(string.Concat('\"', schema.TargetNamespace, '\"'))));
                    }
                    type.CustomAttributes.Add(new CodeAttributeDeclaration(attrib, codeAttributeArgument.ToArray()));
                }

                #region Find item in XmlSchema for generate class documentation.

                XmlSchemaElement currentElement = null;
                if (GeneratorContext.GeneratorParams.EnableSummaryComment)
                {
                    var xmlSchemaElement = SearchElementInSchema(type, schema);
                    if (xmlSchemaElement != null)
                    {
                        currentElement = xmlSchemaElement;
                        if (xmlSchemaElement.Annotation != null)
                        {
                            foreach (var item in xmlSchemaElement.Annotation.Items)
                            {
                                var xmlDoc = item as XmlSchemaDocumentation;
                                if (xmlDoc == null) continue;
                                CreateCommentStatment(type.Comments, xmlDoc);
                            }
                        }
                    }
                }

                #endregion

                foreach (CodeTypeMember member in type.Members)
                {
                    #region Process Fields

                    var codeMemberField = member as CodeMemberField;
                    if (codeMemberField != null)
                        ProcessField(codeMemberField, ctor, code, ref addedToConstructor);

                    #endregion

                    #region Process properties

                    var codeMemberProperty = member as CodeMemberProperty;
                    if (codeMemberProperty != null)
                        ProcessProperty(type, codeMemberProperty, currentElement, schema);

                    #endregion

                }

                // Add new ctor if required
                if (addedToConstructor && newCTor)
                    type.Members.Add(ctor);

                if (GeneratorContext.GeneratorParams.EnableDataBinding)
                {
                    #region add public PropertyChangedEventHandler event

                    // -------------------------------------------------------------------------------
                    // public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
                    // -------------------------------------------------------------------------------
                    var propertyChangedEvent = new CodeMemberEvent
                                                   {
                                                       // ReSharper disable BitwiseOperatorOnEnumWihtoutFlags
                                                       Attributes = (MemberAttributes.Final | MemberAttributes.Public),
                                                       // ReSharper restore BitwiseOperatorOnEnumWihtoutFlags
                                                       Name = "PropertyChanged",
                                                       Type = new CodeTypeReference(typeof(PropertyChangedEventHandler))
                                                   };

                    type.Members.Add(propertyChangedEvent);

                    #endregion

                    #region Add OnPropertyChanged method.

                    // -----------------------------------------------------------
                    //  private void OnPropertyChanged(string info) {
                    //      PropertyChangedEventHandler handler = PropertyChanged;
                    //      if (handler != null) {
                    //          handler(this, new PropertyChangedEventArgs(info));
                    //      }
                    //  }
                    // -----------------------------------------------------------
                    var propertyChangedMethod = new CodeMemberMethod { Name = "OnPropertyChanged" };
                    propertyChangedMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "info"));


                    switch (GeneratorContext.GeneratorParams.Language)
                    {
                        case GenerationLanguage.VisualBasic:

                            propertyChangedMethod.Statements.Add(
                                new CodeExpressionStatement(
                                    new CodeSnippetExpression(
                                        "RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))")));

                            break;

                        case GenerationLanguage.CSharp:
                        case GenerationLanguage.VisualCpp:

                            propertyChangedMethod.Statements.Add(
                                new CodeExpressionStatement(
                                    new CodeSnippetExpression("PropertyChangedEventHandler handler = PropertyChanged")));

                            var codeExpressionStatement =
                                new CodeExpressionStatement(
                                    new CodeSnippetExpression("handler(this, new PropertyChangedEventArgs(info))"));

                            CodeStatement[] statements = new[] { codeExpressionStatement };

                            propertyChangedMethod.Statements.Add(
                                new CodeConditionStatement(new CodeSnippetExpression("handler != null"), statements));

                            break;
                    }

                    type.Members.Add(propertyChangedMethod);

                    #endregion
                }

                if (GeneratorContext.GeneratorParams.IncludeSerializeMethod)
                {
                    type.Members.Add(CodeDomHelper.GetSerializeCodeDomMethod(type));
                    type.Members.Add(CodeDomHelper.GetDeserialize(type));
                    type.Members.Add(CodeDomHelper.GetSaveToFileCodeDomMethod(type));
                    type.Members.Add(CodeDomHelper.GetLoadFromFileCodeDomMethod(type));
                }

                #region Clone

                if (GeneratorContext.GeneratorParams.GenerateCloneMethod)
                    type.Members.Add(CodeDomHelper.GetCloneMethod(type));

                #endregion Clone
            }

            #region Custom Collection

            foreach (string collName in collectionTypesField.Keys)
            {
                var ctd = new CodeTypeDeclaration(collName) { IsClass = true };
                ctd.BaseTypes.Add(string.Format("{0}<{1}>", GeneratorContext.GeneratorParams.CollectionBase, collectionTypesField[collName]));
                ctd.IsPartial = true;

                bool newCTor = false;
                var ctor = GetConstructor(ctd, ref newCTor);

                ctd.Members.Add(ctor);
                code.Types.Add(ctd);
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Search XmlElement in schema.
        /// </summary>
        /// <param name="type">Element to find</param>
        /// <param name="schema">schema object</param>
        /// <returns>return found XmlSchemaElement or null value</returns>
        private static XmlSchemaElement SearchElementInSchema(CodeTypeDeclaration type, XmlSchema schema)
        {
            foreach (XmlSchemaObject item in schema.Items)
            {
                var xmlElement = item as XmlSchemaElement;
                if (xmlElement == null) continue;

                var xmlSubElement = SearchElement(type, xmlElement, string.Empty, string.Empty);
                if (xmlSubElement != null) return xmlSubElement;
            }

            // If not found search in schema inclusion
            foreach (var item in schema.Includes)
            {
                var schemaInc = item as XmlSchemaInclude;
                if (schemaInc == null) continue;

                var includeElmts = SearchElementInSchema(type, schemaInc.Schema);
                if (includeElmts != null) return includeElmts;
            }

            return null;
        }

        /// <summary>
        /// Recursive search of elemement.
        /// </summary>
        /// <param name="type">Element to search</param>
        /// <param name="xmlElement">Current element</param>
        /// <param name="CurrentElementName">Name of the current element.</param>
        /// <param name="HierarchicalElementName">Name of the hierarchical element.</param>
        /// <returns>
        /// return found XmlSchemaElement or null value
        /// </returns>
        private static XmlSchemaElement SearchElement(CodeTypeDeclaration type, XmlSchemaElement xmlElement,
                                                      string CurrentElementName, string HierarchicalElementName)
        {
            bool found = false;
            if (type.IsClass)
            {
                if (xmlElement.Name == null)
                    return null;

                if (type.Name.Equals(HierarchicalElementName + xmlElement.Name) ||
                    (type.Name.Equals(xmlElement.Name)))
                    found = true;
            }
            else
            {
                if (type.Name.Equals(xmlElement.QualifiedName.Name))
                    found = true;
            }

            if (found)
                return xmlElement;

            var xmlComplexType = xmlElement.ElementSchemaType as XmlSchemaComplexType;
            if (xmlComplexType != null)
            {
                var xmlSequence = xmlComplexType.ContentTypeParticle as XmlSchemaSequence;
                if (xmlSequence != null)
                {
                    foreach (XmlSchemaObject item in xmlSequence.Items)
                    {
                        var currentItem = item as XmlSchemaElement;
                        if (currentItem != null)
                        {
                            if (HierarchicalElementName == xmlElement.QualifiedName.Name ||
                                CurrentElementName == xmlElement.QualifiedName.Name)
                                return null;

                            XmlSchemaElement subItem = SearchElement(type, currentItem,
                                                                     xmlElement.QualifiedName.Name,
                                                                     HierarchicalElementName
                                                                     + xmlElement.QualifiedName.Name);
                            if (subItem != null)
                                return subItem;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Create CodeCommentStatement from schema documentation.
        /// </summary>
        /// <param name="codeStatmentColl">CodeCommentStatementCollection collection</param>
        /// <param name="xmlDoc">Schema documentation</param>
        private static void CreateCommentStatment(CodeCommentStatementCollection codeStatmentColl,
                                                  XmlSchemaDocumentation xmlDoc)
        {
            codeStatmentColl.Clear();
            foreach (XmlNode itemDoc in xmlDoc.Markup)
            {
                string textLine = itemDoc.InnerText.Trim();
                if (textLine.Length > 0)
                    CodeDomHelper.CreateSummaryComment(codeStatmentColl, textLine);
            }
        }

        /// <summary>
        /// Field process.
        /// </summary>
        /// <param name="member">CodeTypeMember member</param>
        /// <param name="ctor">CodeMemberMethod constructor</param>
        /// <param name="ns">CodeNamespace XSD</param>
        /// <param name="addedToConstructor">Indicates if create a new constructor</param>
        private static void ProcessField(CodeTypeMember member, CodeMemberMethod ctor, CodeNamespace ns,
                                         ref bool addedToConstructor)
        {
            var field = (CodeMemberField)member;

            #region Add EditorBrowsable.Never for private Attribute

            // ---------------------------------------------
            // [EditorBrowsable(EditorBrowsableState.Never)]
            // ---------------------------------------------
            if (member.Attributes == MemberAttributes.Private)
            {
                if (GeneratorContext.GeneratorParams.HidePrivateFieldInIde)
                {
                    var attributeType = new CodeTypeReference(
                        typeof(EditorBrowsableAttribute).Name.Replace("Attribute", string.Empty));

                    var argument = new CodeAttributeArgument
                                       {
                                           Value = new CodePropertyReferenceExpression(
                                               new CodeSnippetExpression(typeof(EditorBrowsableState).Name), "Never")
                                       };

                    field.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, new[] { argument }));
                }

                /*
                CodeTypeReference attrib = new CodeTypeReference("DataMember");
                field.CustomAttributes.Add(new CodeAttributeDeclaration(attrib));
                */
            }

            #endregion

            #region Change to generic collection type

            // --------------------------------
            // private List <Actor> nameField;
            // --------------------------------
            bool thisIsCollectionType = field.Type.ArrayElementType != null;
            if (thisIsCollectionType)
            {
                CodeTypeReference colType = GetCollectionType(field.Type.BaseType);
                if (colType != null)
                    field.Type = colType;
            }

            #endregion

            #region Object allocation in CTor

            // ---------------------------------------
            // if ((this.nameField == null))
            // {
            //    this.nameField = new List<Name>();
            // }
            // ---------------------------------------
            if (GeneratorContext.GeneratorParams.CollectionObjectType != CollectionType.Array)
            {
                CodeTypeDeclaration declaration = FindTypeInNamespace(field.Type.BaseType, ns);
                if ((thisIsCollectionType
                     ||
                     (((declaration != null) && declaration.IsClass)
                      && ((declaration.TypeAttributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))))
                {
                    ctor.Statements.Insert(0, CreateInstanceCodeStatments(field.Name, field.Type));
                    addedToConstructor = true;
                }
            }

            #endregion
        }

        /// <summary>
        /// Add INotifyPropertyChanged implementation
        /// </summary>
        /// <param name="type">type of declaration</param>
        /// <returns>return CodeConstructor</returns>
        private static CodeConstructor ProcessClass(CodeTypeDeclaration type)
        {
            if (GeneratorContext.GeneratorParams.EnableDataBinding)
                type.BaseTypes.Add(typeof(INotifyPropertyChanged));

            var ctor = new CodeConstructor { Attributes = MemberAttributes.Public };
            return ctor;
        }

        /// <summary>
        /// Create new instance of object
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="type">CodeTypeReference Type</param>
        /// <returns>return instance CodeConditionStatement</returns>
        private static CodeConditionStatement CreateInstanceCodeStatments(string name, CodeTypeReference type)
        {
            var statement = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                                                                    new CodeObjectCreateExpression(type, new CodeExpression[0]));
            return
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)),
                    new CodeStatement[] { statement });
        }

        /// <summary>
        /// Recherche le CodeTypeDeclaration d'un objet en fonction de son type de base (nom de classe)
        /// </summary>
        /// <param name="typeName">Search name</param>
        /// <param name="ns">Seach into</param>
        /// <returns>CodeTypeDeclaration found</returns>
        private static CodeTypeDeclaration FindTypeInNamespace(string typeName, CodeNamespace ns)
        {
            foreach (CodeTypeDeclaration declaration in ns.Types)
            {
                if (declaration.Name == typeName)
                    return declaration;
            }

            return null;
        }

        /// <summary>
        /// Property process
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        /// <param name="member">Type members include fields, methods, properties, constructors and nested types</param>
        /// <param name="xmlElement">Represent the root element in schema</param>
        private static void ProcessProperty(CodeTypeDeclaration type, CodeTypeMember member, XmlSchemaElement xmlElement, XmlSchema schema)
        {
            #region Find item in XmlSchema for summary documentation.

            if (GeneratorContext.GeneratorParams.EnableSummaryComment)
            {
                if (xmlElement != null)
                {
                    var xmlComplexType = xmlElement.ElementSchemaType as XmlSchemaComplexType;
                    bool foundInAttributes = false;
                    if (xmlComplexType != null)
                    {
                        #region Search property in attributes for summary comment generation

                        foreach (XmlSchemaObject attribute in xmlComplexType.Attributes)
                        {
                            var xmlAttrib = attribute as XmlSchemaAttribute;
                            if (xmlAttrib != null)
                            {
                                if (member.Name.Equals(xmlAttrib.QualifiedName.Name))
                                {
                                    CreateCommentFromAnnotation(xmlAttrib.Annotation, member.Comments);
                                    foundInAttributes = true;
                                }
                            }
                        }

                        #endregion

                        #region Search property in XmlSchemaElement for summary comment generation

                        if (!foundInAttributes)
                        {
                            var xmlSequence = xmlComplexType.ContentTypeParticle as XmlSchemaSequence;
                            if (xmlSequence != null)
                            {
                                foreach (XmlSchemaObject item in xmlSequence.Items)
                                {
                                    var currentItem = item as XmlSchemaElement;
                                    if (currentItem != null)
                                    {
                                        if (member.Name.Equals(currentItem.QualifiedName.Name))
                                            CreateCommentFromAnnotation(currentItem.Annotation, member.Comments);
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            var prop = (CodeMemberProperty)member;

            if (prop.Type.ArrayElementType != null)
            {
                CodeTypeReference colType = GetCollectionType(prop.Type.BaseType);
                if (colType != null)
                    prop.Type = colType;
            }

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                CodeTypeReference attrib = new CodeTypeReference("DataMember");
                prop.CustomAttributes.Add(new CodeAttributeDeclaration(attrib));
            }

            // Add OnPropertyChanged in setter 
            if (GeneratorContext.GeneratorParams.EnableDataBinding)
            {
                #region Setter adaptaion for databinding

                if (type.BaseTypes.IndexOf(new CodeTypeReference(typeof(CollectionBase))) == -1)
                {
                    // -----------------------------
                    // if (handler != null) {
                    //    OnPropertyChanged("Name");
                    // -----------------------------
                    //CodeExpressionStatement propChange = new CodeExpressionStatement(new CodeSnippetExpression("OnPropertyChanged(\"" + prop.Name + "\")"));
                    //CodeMethodInvokeExpression canDeserialize = CodeDomHelper.GetInvokeMethod("xmlSerializer", "CanDeserialize", new CodeExpression[] { new CodeSnippetExpression("xmlTextReader") });
                    var propChange =
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnPropertyChanged"),
                            new CodeExpression[] { new CodeSnippetExpression("\"" + prop.Name + "\"") });

                    var propAssignStatment = prop.SetStatements[0] as CodeAssignStatement;
                    if (propAssignStatment != null)
                    {
                        var cfreL = propAssignStatment.Left as CodeFieldReferenceExpression;
                        var cfreR = propAssignStatment.Right as CodePropertySetValueReferenceExpression;

                        if (cfreL != null)
                        {
                            var setValueCondition = new CodeStatementCollection { propAssignStatment, propChange };
                            /*
                            CodeStatement[] setValueCondition = new CodeStatement[2];
                            setValueCondition[0] = propAssignStatment;
                            setValueCondition[1] = propChange;
                            */

                            // ---------------------------------------------
                            // if ((xxxField.Equals(value) != true)) { ... }
                            // ---------------------------------------------
                            var condStatmentCondEquals = new CodeConditionStatement(
                                new CodeBinaryOperatorExpression(
                                    new CodeMethodInvokeExpression(
                                        new CodeFieldReferenceExpression(
                                            null,
                                            cfreL.FieldName),
                                        "Equals",
                                        cfreR),
                                    CodeBinaryOperatorType.IdentityInequality,
                                    new CodePrimitiveExpression(true)),
                                CodeDomHelper.CodeStmtColToArray(setValueCondition));

                            // ---------------------------------------------
                            // if ((xxxField != null)) { ... }
                            // ---------------------------------------------
                            var condStatmentCondNotNull =
                                new CodeConditionStatement(
                                    new CodeBinaryOperatorExpression(
                                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                                                                         cfreL.FieldName),
                                        CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                                    new CodeStatement[] { condStatmentCondEquals },
                                    CodeDomHelper.CodeStmtColToArray(setValueCondition));

                            var property = member as CodeMemberProperty;

                            if (property != null)
                            {
                                if (property.Type.BaseType != new CodeTypeReference(typeof(long)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(int)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(bool)).BaseType)
                                    prop.SetStatements[0] = condStatmentCondNotNull;
                                else
                                    prop.SetStatements[0] = condStatmentCondEquals;
                            }
                        }
                        else
                            prop.SetStatements.Add(propChange);
                    }
                }

                #endregion
            }

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                CodeTypeReference attrib = new CodeTypeReference("System.Runtime.Serialization.DataMemberAttribute");
                prop.CustomAttributes.Add(new CodeAttributeDeclaration(attrib));
            }
        }

        /// <summary>
        /// Generate summary comment from XmlSchemaAnnotation 
        /// </summary>
        /// <param name="xmlSchemaAnnotation">XmlSchemaAnnotation from XmlSchemaElement or XmlSchemaAttribute</param>
        /// <param name="codeCommentStatementCollection">codeCommentStatementCollection from member</param>
        private static void CreateCommentFromAnnotation(XmlSchemaAnnotation xmlSchemaAnnotation,
                                                        CodeCommentStatementCollection codeCommentStatementCollection)
        {
            if (xmlSchemaAnnotation != null)
            {
                foreach (XmlSchemaObject annotation in xmlSchemaAnnotation.Items)
                {
                    var xmlDoc = annotation as XmlSchemaDocumentation;
                    if (xmlDoc != null)
                        CreateCommentStatment(codeCommentStatementCollection, xmlDoc);
                }
            }
        }

        /// <summary>
        /// Get CodeTypeReference for collection
        /// </summary>
        /// <param name="baseType">base type to generate</param>
        /// <returns>return CodeTypeReference of collection</returns>
        private static CodeTypeReference GetCollectionType(string baseType)
        {
            #region Generic collection

            CodeTypeReference collTypeRef = null;
            switch (GeneratorContext.GeneratorParams.CollectionObjectType)
            {
                case CollectionType.List:
                    collTypeRef = new CodeTypeReference("List", new[] { new CodeTypeReference(baseType) });
                    break;

                case CollectionType.ObservableCollection:
                    collTypeRef = new CodeTypeReference("ObservableCollection", new[] { new CodeTypeReference(baseType) });
                    break;

                case CollectionType.DefinedType:
                    string typname = baseType.Replace(".", string.Empty) + "Collection";

                    if (!collectionTypesField.Keys.Contains(typname))
                        collectionTypesField.Add(typname, baseType);

                    collTypeRef = new CodeTypeReference(typname);
                    break;
            }

            return collTypeRef;

            #endregion
        }

        /// <summary>
        /// Search defaut constructor. If not exist, create a new ctor.
        /// </summary>
        /// <param name="type">CodeTypeDeclaration type</param>
        /// <param name="newCTor">Indicates if new constructor</param>
        /// <returns>return current or new CodeConstructor</returns>
        static private CodeConstructor GetConstructor(CodeTypeDeclaration type, ref bool newCTor)
        {
            #region get or set Constructor

            CodeConstructor ctor = null;
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeConstructor)
                    ctor = member as CodeConstructor;
            }

            if (ctor == null)
            {
                newCTor = true;
                ctor = ProcessClass(type);
            }

            if (GeneratorContext.GeneratorParams.EnableSummaryComment)
                CodeDomHelper.CreateSummaryComment(ctor.Comments, string.Format("{0} class constructor", ctor.Name));

            /* RU20090225: TODO: Implement WCF attribute generation
                        if (GeneratorContext.GeneratorParams.GenerateDataContracts)
                        {
                            var attribute = CodeDomHelper.CreateSimpleAttribute(typeof(DataContractAttribute));
                            ctor.CustomAttributes.Add(attribute);
                        }
            */

            return ctor;

            #endregion
        }
    }
}