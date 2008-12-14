//-----------------------------------------------------------------------
// <copyright file="GeneratorExtension.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Library.Extensions
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Xml.Schema;
    using Xsd2Code;
    using System.Xml;
    using System.Configuration;
    using Xsd2Code.Library.Helpers;
    using System.Xml.Serialization;
    using System.IO;

    /// <summary>
    /// Convertion des array properties en collection
    /// </summary>
    public class GeneratorExtension : ICodeExtension
    {
        /// <summary>
        /// Sorted list for custom collection
        /// </summary>
        private static SortedList<string, string> collectionTypesField = new SortedList<string, string>();

        /// <summary>
        /// Process method for cs or vb CodeDom generation
        /// </summary>
        /// <param name="code">CodeNamespace generated</param>
        /// <param name="schema">XmlSchema to generate</param>
        public void Process(CodeNamespace code, XmlSchema schema)
        {
            collectionTypesField.Clear();

            CodeTypeDeclaration[] types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            XmlSchemaElement currentElement;
            foreach (CodeTypeDeclaration type in types)
            {
                if (type.IsClass || type.IsStruct)
                {
                    bool addedToConstructor = false;
                    bool newCTor = false;
                    CodeConstructor ctor = this.GetConstructor(type, ref newCTor);

                    #region Find item in XmlSchema for generate class documentation.
                    currentElement = null;
                    if (GeneratorContext.EnableSummaryComment)
                    {
                        XmlSchemaElement xmlElement = SearchElementInSchema(type, schema);
                        if (xmlElement != null)
                        {
                            currentElement = xmlElement;
                            if (xmlElement.Annotation != null)
                            {
                                foreach (XmlSchemaObject item in xmlElement.Annotation.Items)
                                {
                                    XmlSchemaDocumentation xmlDoc = item as XmlSchemaDocumentation;
                                    if (xmlDoc != null)
                                    {
                                        CreateCommentStatment(type.Comments, xmlDoc);
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    int j = 0;
                    foreach (CodeTypeMember member in type.Members)
                    {
                        #region Process Fields
                        if (member is CodeMemberField)
                        {
                            ProcessField(member, ctor, code, ref addedToConstructor);
                        }
                        #endregion

                        #region Process properties
                        if (member is CodeMemberProperty)
                        {
                            ProcessProperty(type, member, currentElement);
                        }
                        #endregion
                        j++;
                    }

                    // Add new ctor if required
                    if (addedToConstructor && newCTor)
                    {
                        type.Members.Add(ctor);
                    }

                    if (GeneratorContext.EnableDataBinding)
                    {
                        #region add public PropertyChangedEventHandler event
                        // -------------------------------------------------------------------------------
                        // public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
                        // -------------------------------------------------------------------------------
                        CodeMemberEvent cme = new CodeMemberEvent();
                        cme.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                        cme.Name = "PropertyChanged";
                        cme.Type = new CodeTypeReference(typeof(PropertyChangedEventHandler));
                        type.Members.Add(cme);
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
                        CodeMemberMethod propChanged = new CodeMemberMethod();
                        propChanged.Name = "OnPropertyChanged";
                        propChanged.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "info"));

                        if (GeneratorContext.Language == GenerationLanguage.CSharp || GeneratorContext.Language == GenerationLanguage.VisualCpp)
                        {
                            propChanged.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("PropertyChangedEventHandler handler = PropertyChanged")));
                            CodeExpressionStatement cs1 = new CodeExpressionStatement(new CodeSnippetExpression("handler(this, new PropertyChangedEventArgs(info))"));
                            CodeStatement[] statements = new CodeExpressionStatement[] { cs1 };
                            propChanged.Statements.Add(new CodeConditionStatement(new CodeSnippetExpression("handler != null"), statements));
                        }
                        else
                        {
                            propChanged.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))")));
                        }

                        type.Members.Add(propChanged);
                        #endregion
                    }

                    if (GeneratorContext.IncludeSerializeMethod)
                    {
                        type.Members.Add(CodeDomHelper.GetSerializeCodeDomMethod(type));
                        type.Members.Add(CodeDomHelper.GetDeserialize(type));
                        type.Members.Add(CodeDomHelper.GetSaveToFileCodeDomMethod(type));
                        type.Members.Add(CodeDomHelper.GetLoadFromFileCodeDomMethod(type));
                    }

                    #region Clone
                    /* Prepare for next release 2.8!
                    CodeMemberMethod cloneMethod = new CodeMemberMethod();
                    cloneMethod.Attributes = MemberAttributes.Public;
                    cloneMethod.Name = "Clone";
                    cloneMethod.ReturnType = new CodeTypeReference(typeof(object));

                    cloneMethod.Statements.Add(new CodeVariableDeclarationStatement(
                    new CodeTypeReference(type.Name), "cloneObject", new CodeObjectCreateExpression(new CodeTypeReference(type.Name))));

                    foreach (CodeTypeMember member in type.Members)
                    {
                        #region Process Fields
                        if (member is CodeMemberProperty)
                        {
                            CodeMemberProperty cmp = member as CodeMemberProperty;
                            CodeAssignStatement cdtAssignStmt = new CodeAssignStatement(new CodeSnippetExpression(string.Format("cloneObject.{0}", member.Name)), new CodeFieldReferenceExpression( new CodeThisReferenceExpression(), member.Name));
                            cloneMethod.Statements.Add(cdtAssignStmt);
                        }
                        #endregion
                    }
                    cloneMethod.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("cloneObject")));
                    type.Members.Add(cloneMethod);
                    */
                    #endregion Clone
                }
            }

            #region Custom Collection
            foreach (string collName in collectionTypesField.Keys)
            {
                CodeTypeDeclaration ctd = new CodeTypeDeclaration(collName);
                ctd.IsClass = true;
                ctd.BaseTypes.Add(GeneratorContext.CollectionBase + "<" + collectionTypesField[collName] + ">");
                ctd.IsPartial = true;

                bool newCTor = false;
                CodeConstructor ctor = this.GetConstructor(ctd, ref newCTor);

                ctd.Members.Add(ctor);
                code.Types.Add(ctd);
            }
            #endregion
        }

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
                XmlSchemaElement xmlElement = item as XmlSchemaElement;
                if (xmlElement != null)
                {
                    XmlSchemaElement xmlSubElement = SearchElement(type, xmlElement, string.Empty);
                    if (xmlSubElement != null)
                    {
                        return xmlSubElement;
                    }
                }
            }

            // If not found search in schema inclusion
            foreach (XmlSchemaObject item in schema.Includes)
            {
                XmlSchemaInclude schemaInc = item as XmlSchemaInclude;
                if (schemaInc != null)
                {
                    XmlSchemaElement includeElmts = SearchElementInSchema(type, schemaInc.Schema);
                    if (includeElmts != null)
                    {
                        return includeElmts;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursive search of elemement.
        /// </summary>
        /// <param name="type">Element to search</param>
        /// <param name="xmlElement">Current element</param>
        /// <param name="parentsName">Name of parent element</param>
        /// <returns>return found XmlSchemaElement or null value</returns>
        private static XmlSchemaElement SearchElement(CodeTypeDeclaration type, XmlSchemaElement xmlElement, string parentsName)
        {
            bool found = false;
            if (type.IsClass)
            {
                if (xmlElement.Name == null)
                {
                    return null;
                }
                else
                {
                    if (type.Name.Equals(parentsName + xmlElement.Name) ||
                        (type.Name.Equals(xmlElement.Name)))
                    {
                        found = true;
                    }
                }
            }
            else
            {
                if (type.Name.Equals(xmlElement.QualifiedName.Name))
                {
                    found = true;
                }
            }

            if (found)
            {
                return xmlElement;
            }
            else
            {
                XmlSchemaComplexType xmlComplexType = xmlElement.ElementSchemaType as XmlSchemaComplexType;
                if (xmlComplexType != null)
                {
                    XmlSchemaSequence xmlSequence = xmlComplexType.ContentTypeParticle as XmlSchemaSequence;
                    if (xmlSequence != null)
                    {
                        foreach (XmlSchemaObject item in xmlSequence.Items)
                        {
                            XmlSchemaElement currentItem = item as XmlSchemaElement;
                            if (currentItem != null)
                            {
                                if (parentsName == xmlElement.QualifiedName.Name)
                                {
                                    return null;
                                }

                                XmlSchemaElement subItem = SearchElement(type, currentItem, parentsName + xmlElement.QualifiedName.Name);
                                if (subItem != null)
                                {
                                    return subItem;
                                }
                            }
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
        private static void CreateCommentStatment(CodeCommentStatementCollection codeStatmentColl, XmlSchemaDocumentation xmlDoc)
        {
            codeStatmentColl.Clear();
            foreach (XmlNode itemDoc in xmlDoc.Markup)
            {
                string textLine = itemDoc.InnerText.Trim();
                if (textLine.Length > 0)
                {
                    CodeDomHelper.CreateSummaryComment(codeStatmentColl, textLine);
                }
            }
        }

        /// <summary>
        /// Field process.
        /// </summary>
        /// <param name="member">CodeTypeMember member</param>
        /// <param name="ctor">CodeMemberMethod constructor</param>
        /// <param name="ns">CodeNamespace XSD</param>
        /// <param name="addedToConstructor">Indicates if create a new constructor</param>
        private static void ProcessField(CodeTypeMember member, CodeMemberMethod ctor, CodeNamespace ns, ref bool addedToConstructor)
        {
            CodeMemberField field = (CodeMemberField)member;

            #region Add EditorBrowsable.Never for private Attribute
            // ---------------------------------------------
            // [EditorBrowsable(EditorBrowsableState.Never)]
            // ---------------------------------------------
            if (member.Attributes == MemberAttributes.Private)
            {
                if (GeneratorContext.HidePrivateFieldInIde)
                {
                    CodeTypeReference attributeType = new CodeTypeReference(typeof(EditorBrowsableAttribute).Name.Replace("Attribute", string.Empty));
                    CodeAttributeArgument argument = new CodeAttributeArgument();
                    argument.Value = new CodePropertyReferenceExpression(new CodeSnippetExpression(typeof(EditorBrowsableState).Name), "Never");
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, new CodeAttributeArgument[] { argument }));
                }
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
                {
                    field.Type = colType;
                }
            }
            #endregion

            #region Object allocation in CTor
            // ---------------------------------------
            // if ((this.nameField == null))
            // {
            //    this.nameField = new List<Name>();
            // }
            // ---------------------------------------
            if (GeneratorContext.CollectionObjectType != CollectionType.Array)
            {
                CodeTypeDeclaration declaration = FindTypeInNamespace(field.Type.BaseType, ns);
                if ((thisIsCollectionType || (((declaration != null) && declaration.IsClass) && ((declaration.TypeAttributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))))
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
            if (GeneratorContext.EnableDataBinding)
            {
                type.BaseTypes.Add(typeof(INotifyPropertyChanged));
            }

            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
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
            CodeAssignStatement statement;
            statement = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name), new CodeObjectCreateExpression(type, new CodeExpression[0]));
            return new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)), new CodeStatement[] { statement });
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
                {
                    return declaration;
                }
            }

            return null;
        }

        /// <summary>
        /// Property process
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        /// <param name="member">Type members include fields, methods, properties, constructors and nested types</param>
        /// <param name="xmlElement">Represent the root element in schema</param>
        private static void ProcessProperty(CodeTypeDeclaration type, CodeTypeMember member, XmlSchemaElement xmlElement)
        {
            #region Find item in XmlSchema for summary documentation.
            if (GeneratorContext.EnableSummaryComment)
            {
                if (xmlElement != null)
                {
                    XmlSchemaComplexType xmlComplexType = xmlElement.ElementSchemaType as XmlSchemaComplexType;
                    bool foundInAttributes = false;
                    if (xmlComplexType != null)
                    {
                        #region Search property in attributes for summary comment generation
                        foreach (XmlSchemaObject attribute in xmlComplexType.Attributes)
                        {
                            XmlSchemaAttribute xmlAttrib = attribute as XmlSchemaAttribute;
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
                            XmlSchemaSequence xmlSequence = xmlComplexType.ContentTypeParticle as XmlSchemaSequence;
                            if (xmlSequence != null)
                            {
                                foreach (XmlSchemaObject item in xmlSequence.Items)
                                {
                                    XmlSchemaElement currentItem = item as XmlSchemaElement;
                                    if (currentItem != null)
                                    {
                                        if (member.Name.Equals(currentItem.QualifiedName.Name))
                                        {
                                            CreateCommentFromAnnotation(currentItem.Annotation, member.Comments);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion

            CodeMemberProperty prop = (CodeMemberProperty)member;

            if (prop.Type.ArrayElementType != null)
            {
                CodeTypeReference colType = GetCollectionType(prop.Type.BaseType);
                if (colType != null)
                {
                    prop.Type = colType;
                }
            }

            // Add OnPropertyChanged in setter 
            if (GeneratorContext.EnableDataBinding)
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
                    CodeMethodInvokeExpression propChange = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnPropertyChanged"), new CodeExpression[] { new CodeSnippetExpression("\"" + prop.Name + "\"") });
                    
                    CodeAssignStatement propAssignStatment = prop.SetStatements[0] as CodeAssignStatement;
                    if (propAssignStatment != null)
                    {
                        CodeFieldReferenceExpression cfreL = propAssignStatment.Left as CodeFieldReferenceExpression;
                        CodePropertySetValueReferenceExpression cfreR = propAssignStatment.Right as CodePropertySetValueReferenceExpression;

                        if (cfreL != null)
                        {
                            CodeStatementCollection setValueCondition = new CodeStatementCollection();
                            setValueCondition.Add(propAssignStatment);
                            setValueCondition.Add(propChange);
                            /*
                            CodeStatement[] setValueCondition = new CodeStatement[2];
                            setValueCondition[0] = propAssignStatment;
                            setValueCondition[1] = propChange;
                            */

                            // ---------------------------------------------
                            // if ((xxxField.Equals(value) != true)) { ... }
                            // ---------------------------------------------
                            CodeConditionStatement condStatmentCondEquals = new CodeConditionStatement(
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
                            CodeConditionStatement condStatmentCondNotNull = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), cfreL.FieldName), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), new CodeStatement[] { condStatmentCondEquals }, CodeDomHelper.CodeStmtColToArray(setValueCondition));

                            CodeMemberProperty property = member as CodeMemberProperty;

                            if (property.Type.BaseType != new CodeTypeReference(typeof(long)).BaseType &&
                                property.Type.BaseType != new CodeTypeReference(typeof(int)).BaseType &&
                                property.Type.BaseType != new CodeTypeReference(typeof(bool)).BaseType)
                            {
                                prop.SetStatements[0] = condStatmentCondNotNull;
                            }
                            else
                            {
                                prop.SetStatements[0] = condStatmentCondEquals;
                            }
                        }
                        else
                        {
                            prop.SetStatements.Add(propChange);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Generate summary comment from XmlSchemaAnnotation 
        /// </summary>
        /// <param name="xmlSchemaAnnotation">XmlSchemaAnnotation from XmlSchemaElement or XmlSchemaAttribute</param>
        /// <param name="codeCommentStatementCollection">codeCommentStatementCollection from member</param>
        private static void CreateCommentFromAnnotation(XmlSchemaAnnotation xmlSchemaAnnotation, CodeCommentStatementCollection codeCommentStatementCollection)
        {
            if (xmlSchemaAnnotation != null)
            {
                foreach (XmlSchemaObject annotation in xmlSchemaAnnotation.Items)
                {
                    XmlSchemaDocumentation xmlDoc = annotation as XmlSchemaDocumentation;
                    if (xmlDoc != null)
                    {
                        CreateCommentStatment(codeCommentStatementCollection, xmlDoc);
                    }
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
            switch (GeneratorContext.CollectionObjectType)
            {
                case CollectionType.List:
                    collTypeRef = new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(baseType) });
                    break;

                case CollectionType.ObservableCollection:
                    collTypeRef = new CodeTypeReference("ObservableCollection", new CodeTypeReference[] { new CodeTypeReference(baseType) });
                    break;

                case CollectionType.DefinedType:
                    string typname = baseType.Replace(".", string.Empty) + "Collection";

                    if (!collectionTypesField.Keys.Contains(typname))
                    {
                        collectionTypesField.Add(typname, baseType);
                    }

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
        private CodeConstructor GetConstructor(CodeTypeDeclaration type, ref bool newCTor)
        {
            #region get or set Constructor
            CodeConstructor ctor = null;
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeConstructor)
                {
                    ctor = member as CodeConstructor;
                }
            }

            if (ctor == null)
            {
                newCTor = true;
                ctor = ProcessClass(type);
            }

            if (GeneratorContext.EnableSummaryComment)
            {
                CodeDomHelper.CreateSummaryComment(ctor.Comments, string.Format("{0} class constructor", ctor.Name));
            }

            return ctor;
            #endregion
        }
    }
}