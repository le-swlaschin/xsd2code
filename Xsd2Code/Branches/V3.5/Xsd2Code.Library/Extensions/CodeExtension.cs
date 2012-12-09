// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeExtension.cs" company="Xsd2Code">
//   N/A
// </copyright>
// <summary>
//   Base class for code generation extension
// </summary>
// <remarks>
//  Revision history:
//  Created 2009-03-16 by Ruslan Urban
//  based on GeneratorExtension.cs
//  Updated 2009-05-18 move wcf CodeDom generation into Net35Extention.cs by Pascal Cabanel
//  Updated 2009-05-18 Remove .Net 2.0 XML attributes by Pascal Cabanel
//  Updated 2009-06-16 Add EntityBase class.
//                     Add new serialize/deserialize methods.
//                     Dispose object in serialize/deserialize methods.
//  Updated 2010-01-07 Deerwood McCord Jr. (DCM) applied patch from Rob van der Veer
//  Updated 2010-01-20 Deerwood McCord Jr. Cleaned CodeSnippetStatements by replacing with specific CodeDom Expressions
//                     Refactored OnPropertyChanged to use more CodeDom Specific version found in CodeDomHelper.CreateOnPropertyChangeMethod()
//  Updated 2010-08-16 Pascal Cabanel. Add tracking changes class.
//                     Refactored GeneratorContext.GeneratorParams.
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xsd2Code.Library.Helpers;

namespace Xsd2Code.Library.Extensions
{
    /// <summary>
    /// Base class for code generation extension
    /// </summary>
    public abstract class CodeExtension : ICodeExtension
    {
        #region private fields

        /// <summary>
        /// Sorted list for custom collection
        /// </summary>
        private static readonly SortedList<string, string> CollectionTypes = new SortedList<string, string>();

        /// <summary>
        /// Contains all enum.
        /// </summary>
        private static List<string> enumListField;

        /// <summary>
        /// Contains all collection fields.
        /// </summary>
        private static readonly List<string> LazyLoadingFields = new List<string>();

        /// <summary>
        /// Contains all collection fields.
        /// </summary>
        protected static List<string> CollectionTypesFields = new List<string>();

        /// <summary>
        /// Contains all collection fields.
        /// </summary>
        protected static List<string> ShouldSerializeFields = new List<string>();

        /// <summary>
        /// List of public properties
        /// </summary>
        protected static List<string> PropertiesListFields = new List<string>();

        /// <summary>
        /// List of private fileds
        /// </summary>
        protected static List<string> MemberFieldsListFields = new List<string>();

        private static List<string> TypeList { get; set; }

        #endregion

        #region public method

        /// <summary>
        /// Process method for cs or vb CodeDom generation
        /// </summary>
        /// <param name="code">CodeNamespace generated</param>
        /// <param name="schema">XmlSchema to generate</param>
        public virtual void Process(CodeNamespace code, XmlSchema schema)
        {
            ImportNamespaces(code);
            var types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            // Generate generic base class
            if (GeneratorContext.GeneratorParams.Language == GenerationLanguage.CSharp &&
                GeneratorContext.GeneratorParams.TrackingChanges.Enabled)
            {
                if (GeneratorContext.GeneratorParams.TrackingChanges.GenerateTrackingClasses)
                {
                    IEnumerable<CodeTypeDeclaration> classList =
                        TrackingChangesExtention.GenerateTrackingChangesClasses();
                    foreach (CodeTypeDeclaration codeTypeDeclaration in classList)
                    {
                        code.Types.Insert(0, codeTypeDeclaration);
                    }
                }
            }

            // Generate generic base class
            if (GeneratorContext.GeneratorParams.GenericBaseClass.Enabled &&
                GeneratorContext.GeneratorParams.GenericBaseClass.GenerateBaseClass)
            {
                code.Types.Insert(0, GenerateBaseClass());
            }

            enumListField = (from p in types
                             where p.IsEnum
                             select p.Name).ToList();

            // First we do an initial loop through the types to rename them to pascal case (and fix their attributes accordingly)
            if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
            {
                TypeList = new List<string>();
                foreach (CodeTypeDeclaration type in types)
                {
                    TypeList.Add(type.Name);
                    IEnumerable<CodeAttributeDeclaration> attributes = type.CustomAttributes.Cast<CodeAttributeDeclaration>();

                    if (!attributes.Any(att => att.Name == "System.Xml.Serialization.XmlRootAttribute"))
                    {
                        var xmlAtt = new CodeAttributeDeclaration("System.Xml.Serialization.XmlRootAttribute");
                        xmlAtt.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(type.Name)));
                        type.CustomAttributes.Add(xmlAtt);
                    }
                    else
                    {
                        attributes.First(att => att.Name == "System.Xml.Serialization.XmlRootAttribute").Arguments.Add(
                            new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(type.Name)));
                    }
                    if (attributes.Any(att => att.Name == "System.Xml.Serialization.XmlTypeAttribute"))
                    {
                        var arguments = attributes.First(att => att.Name == "System.Xml.Serialization.XmlTypeAttribute").Arguments;

                        bool typeNameArgumentExist = false;
                        foreach (CodeAttributeArgument argument in arguments)
                        {
                            if (argument.Name == "TypeName")
                            {
                                typeNameArgumentExist = true;
                                argument.Value = new CodePrimitiveExpression(type.Name);
                                break;
                            }
                        }
                        if (!typeNameArgumentExist)
                            arguments.Add(new CodeAttributeArgument("TypeName", new CodePrimitiveExpression(type.Name)));
                    }
                    type.Name = CodeDomHelper.GetPascalCaseName(type.Name);
                }
            }

            // Fixes http://xsd2code.codeplex.com/WorkItem/View.aspx?WorkItemId=14391
            CollectionTypes.Clear();
            foreach (CodeTypeDeclaration type in types)
            {
                if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                {
                    //If one type extends another it needs the Include attribute to be fixed
                    IEnumerable<CodeAttributeDeclaration> attributes = type.CustomAttributes.Cast<CodeAttributeDeclaration>();
                    if (attributes.Any(att => att.Name == "System.Xml.Serialization.XmlIncludeAttribute"))
                    {
                        foreach (
                            CodeAttributeDeclaration includeAtt in
                                attributes.Where(att => att.Name == "System.Xml.Serialization.XmlIncludeAttribute"))
                        {
                            CodeTypeReference typeOfArg = ((CodeTypeOfExpression)(includeAtt.Arguments[0].Value)).Type;
                            if (TypeList.Contains(typeOfArg.BaseType))
                                typeOfArg.BaseType = CodeDomHelper.GetPascalCaseName(typeOfArg.BaseType);
                        }
                    }

                    //and the types it extends to be fixed
                    foreach (CodeTypeReference baseType in type.BaseTypes)
                    {
                        if (TypeList.Contains(baseType.BaseType))
                            baseType.BaseType = CodeDomHelper.GetPascalCaseName(baseType.BaseType);
                    }
                }
                LazyLoadingFields.Clear();
                CollectionTypesFields.Clear();

                // Fixes http://xsd2code.codeplex.com/WorkItem/View.aspx?WorkItemId=8781
                // and http://xsd2code.codeplex.com/WorkItem/View.aspx?WorkItemId=6944
                if (GeneratorContext.GeneratorParams.Miscellaneous.ExcludeIncludedTypes)
                {
                    //if the typeName is NOT defined in the current schema, skip it.
                    if (SchemaHelper.IsIncludedType(schema.Includes, type.Name))
                    {
                        code.Types.Remove(type);
                    }
                }


                // Remove default remarks attribute
                type.Comments.Clear();

                // Remove default .Net 2.0 XML attributes if disabled or silverlight project.
                // Fixes http://xsd2code.codeplex.com/workitem/11761
                if (!GeneratorContext.GeneratorParams.Serialization.GenerateXmlAttributes)
                {
                    RemoveDefaultXmlAttributes(type.CustomAttributes);
                }
                else if (GeneratorContext.GeneratorParams.TargetFramework == TargetFramework.Silverlight)
                {
                    RemoveNonSilverlightXmlAttributes(type.CustomAttributes);
                }

                if (!type.IsClass && !type.IsStruct)
                {
                    SummaryCommentsHelper.ProcessEnum(type, schema);
                    continue;
                }

                ProcessClass(code, schema, type);
            }

            foreach (string collName in CollectionTypes.Keys)
                CreateCollectionClass(code, collName);
        }

        /// <summary>
        /// Determines whether the specified schema contains the type.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified schema contains the type; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>Used to Exclude Included Types from Schema</remarks>
        private bool ContainsTypeName(XmlSchema schema, CodeTypeDeclaration type)
        {
            foreach (XmlSchemaObject item in schema.Items)
            {
                var complexItem = item as XmlSchemaComplexType;
                if (complexItem != null)
                {
                    if (complexItem.Name == type.Name)
                    {
                        return true;
                    }
                }
                var elementItem = item as XmlSchemaElement;
                if (elementItem != null)
                {
                    if (elementItem.Name == type.Name)
                    {
                        return true;
                    }
                }
                var simpleItem = item as XmlSchemaSimpleType;
                if (simpleItem != null)
                {
                    if (simpleItem.Name == type.Name)
                    {
                        return true;
                    }
                }
            }

            //TODO: Does not work for combined anonymous types 
            //fallback: Check if the namespace attribute of the type equals the namespace of the file.
            //first, find the XmlType attribute.
            foreach (CodeAttributeDeclaration attribute in type.CustomAttributes)
            {
                if (attribute.Name == "System.Xml.Serialization.XmlTypeAttribute")
                {
                    foreach (CodeAttributeArgument argument in attribute.Arguments)
                    {
                        if (argument.Name == "Namespace")
                        {
                            if (schema.TargetNamespace.Equals(((CodePrimitiveExpression)argument.Value).Value))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region protedted methods



        protected static CodeTypeMember GetShouldSerializeMethod(string propertyName)
        {
            // ----------------------------------------------------------------------
            // /// <summary>
            // /// Test whether PropertyName should be serialized
            // /// </summary>
            // public bool ShouldSerializePropertyName()
            // {
            //    return this.PropertyName.HasValue;
            // }
            // ----------------------------------------------------------------------
            var cloneMethod = new CodeMemberMethod
                                  {
                                      Attributes = MemberAttributes.Public,
                                      Name = string.Format("ShouldSerialize{0}", propertyName),
                                      ReturnType = new CodeTypeReference(typeof(bool))
                                  };

            CodeDomHelper.CreateSummaryComment(
                cloneMethod.Comments,
                string.Format("Test whether {0} should be serialized", propertyName));

            var hasValueStatment = new CodeFieldReferenceExpression(null, string.Format("{0}.HasValue", propertyName));
            var statement = new CodeMethodReturnStatement(hasValueStatment);
            cloneMethod.Statements.Add(statement);
            return cloneMethod;
        }

        /// <summary>
        /// Processes the class.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="schema">The input xsd schema.</param>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        protected virtual void ProcessClass(CodeNamespace codeNamespace, XmlSchema schema, CodeTypeDeclaration type)
        {
            bool addedToConstructor = false;
            bool newCTor = false;

            CodeConstructor ctor = GetConstructor(type, ref newCTor);
            ShouldSerializeFields.Clear();
            MemberFieldsListFields.Clear();
            PropertiesListFields.Clear();

            // Inherits from EntityBase
            bool inheritsFromClass = type.BaseTypes.Count > 0;
            if (GeneratorContext.GeneratorParams.GenericBaseClass.Enabled && !inheritsFromClass)
            {
                var ctr = new CodeTypeReference(GeneratorContext.GeneratorParams.GenericBaseClass.BaseClassName);
                ctr.TypeArguments.Add(new CodeTypeReference(type.Name));
                type.BaseTypes.Add(ctr);
            }
            else
            {
                if (!inheritsFromClass)
                {
                    if (GeneratorContext.GeneratorParams.EnableDataBinding)
                        type.BaseTypes.Add(typeof(INotifyPropertyChanged));
                }
            }

            // Generate WCF DataContract
            CreateDataContractAttribute(type, schema);
            SummaryCommentsHelper.CreateSummaryCommentFromSchema(type, schema);
            foreach (CodeTypeMember member in type.Members)
            {
                // Remove default remarks attribute
                if (!(member is CodeConstructor))
                {
                    member.Comments.Clear();
                }

                // Remove default .Net 2.0 XML attributes if disabled or silverlight project.
                // Fixes http://xsd2code.codeplex.com/workitem/11761
                if (!GeneratorContext.GeneratorParams.Serialization.GenerateXmlAttributes)
                {
                    RemoveDefaultXmlAttributes(member.CustomAttributes);
                }
                else if (GeneratorContext.GeneratorParams.TargetFramework == TargetFramework.Silverlight)
                {
                    RemoveNonSilverlightXmlAttributes(member.CustomAttributes);
                }


                //-------------------------------------------------------------------------------------------------------
                //These changes are to re-name all references to classes in fields that have been renamed to pascal case.
                //-------------------------------------------------------------------------------------------------------
                var codeMember = member as CodeMemberField;
                if (codeMember != null)
                {
                    // Change Base type Name to PascalCase.
                    if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                    {
                        if (TypeList.Contains(codeMember.Type.BaseType))
                            codeMember.Type.BaseType = CodeDomHelper.GetPascalCaseName(codeMember.Type.BaseType);

                        if (codeMember.Type.ArrayElementType != null)
                        {
                            if (TypeList.Contains(codeMember.Type.ArrayElementType.BaseType))
                                codeMember.Type.ArrayElementType.BaseType =
                                    CodeDomHelper.GetPascalCaseName(codeMember.Type.ArrayElementType.BaseType);
                        }
                    }
                    MemberFieldsListFields.Add(codeMember.Name);
                    ProcessFields(codeMember, ctor, codeNamespace, ref addedToConstructor);
                }

                var codeMemberProperty = member as CodeMemberProperty;
                if (codeMemberProperty != null)
                {
                    #region CamelCase, re-name all references to classes in properties that have been renamed to pascal case

                    if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                    {
                        if (TypeList.Contains(codeMemberProperty.Type.BaseType))
                            codeMemberProperty.Type.BaseType =
                                CodeDomHelper.GetPascalCaseName(codeMemberProperty.Type.BaseType);
                        if (codeMemberProperty.Type.ArrayElementType != null)
                        {
                            if (TypeList.Contains(codeMemberProperty.Type.ArrayElementType.BaseType))
                                codeMemberProperty.Type.ArrayElementType.BaseType =
                                    CodeDomHelper.GetPascalCaseName(codeMemberProperty.Type.ArrayElementType.BaseType);
                        }

                        // Rename typeof argument of System.Xml.Serialization.XmlArrayItemAttribute in PascalCase
                        if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                        {
                            foreach (CodeAttributeDeclaration attribute in codeMemberProperty.CustomAttributes)
                            {
                                if (attribute.Name == "System.Xml.Serialization.XmlArrayItemAttribute" ||
                                    attribute.Name == "System.Xml.Serialization.XmlElementAttribute")
                                {
                                    foreach (CodeAttributeArgument argument in attribute.Arguments)
                                    {
                                        if (argument.Value is CodeTypeOfExpression)
                                        {
                                            if (TypeList.Contains(((CodeTypeOfExpression)argument.Value).Type.BaseType))
                                            {
                                                ((CodeTypeOfExpression)argument.Value).Type.BaseType =
                                                    CodeDomHelper.GetPascalCaseName(
                                                        ((CodeTypeOfExpression)argument.Value).Type.BaseType);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        IEnumerable<CodeAttributeDeclaration> attrs =
                            codeMemberProperty.CustomAttributes.Cast<CodeAttributeDeclaration>();

                        //These changes are to ensure that the attributes have the correct camel case name reference in them.
                        CodeAttributeDeclaration xmlElAtt = null;
                        if (attrs.Any(att => att.Name == "System.Xml.Serialization.XmlElementAttribute"))
                        {
                            xmlElAtt = attrs.First(att => att.Name == "System.Xml.Serialization.XmlElementAttribute");
                            xmlElAtt.Arguments.Add(new CodeAttributeArgument("ElementName",
                                                                             new CodePrimitiveExpression(
                                                                                 codeMemberProperty.Name)));
                            //This is almost certainly the wrong place to address this issue
                            CodeDomHelper.FixBadTypeMapping(type, codeMemberProperty, xmlElAtt);
                        }
                        else if (!attrs.Any(att => att.Name == "System.Xml.Serialization.XmlAttributeAttribute") &&
                                 !attrs.Any(att => att.Name == "System.Xml.Serialization.XmlArrayAttribute"))
                        {
                            xmlElAtt = new CodeAttributeDeclaration("System.Xml.Serialization.XmlElementAttribute");
                            xmlElAtt.Arguments.Add(
                                new CodeAttributeArgument(new CodePrimitiveExpression(codeMemberProperty.Name)));
                            codeMemberProperty.CustomAttributes.Add(xmlElAtt);
                        }
                        if (attrs.Any(att => att.Name == "System.Xml.Serialization.XmlAttributeAttribute"))
                        {
                            CodeAttributeDeclaration xmlAttAtt =
                                attrs.First(att => att.Name == "System.Xml.Serialization.XmlAttributeAttribute");
                            xmlAttAtt.Arguments.Add(new CodeAttributeArgument("AttributeName",
                                                                              new CodePrimitiveExpression(
                                                                                  codeMemberProperty.Name)));
                            //This is almost certainly the wrong place to address this issue
                            CodeDomHelper.FixBadTypeMapping(type, codeMemberProperty, xmlAttAtt);
                        }

                        codeMemberProperty.Name = CodeDomHelper.GetPascalCaseName(codeMemberProperty.Name);
                    }

                    #endregion

                    PropertiesListFields.Add(codeMemberProperty.Name);
                    ProcessProperty(type, codeNamespace, codeMemberProperty, schema);
                }
            }

            //DCM: Moved From GeneraterFacade File based removal to CodeDom Style Attribute-based removal
            if (!GeneratorContext.GeneratorParams.Miscellaneous.DisableDebug)
            {
                RemoveDebugAttributes(type.CustomAttributes);
            }

            // If ctor contains fields initialized with enum, change enum name with pascal case.
            if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
            {
                foreach (var statement in ctor.Statements.Cast<CodeAssignStatement>())
                {
                    if (statement.Right is CodeFieldReferenceExpression)
                    {
                        var assignedElement =
                            ((CodeFieldReferenceExpression)statement.Right).TargetObject as CodeTypeReferenceExpression;
                        if (assignedElement != null)
                        {
                            if (TypeList.Contains(assignedElement.Type.BaseType))
                            {
                                assignedElement.Type.BaseType =
                                    CodeDomHelper.GetPascalCaseName(assignedElement.Type.BaseType);
                            }
                        }
                    }
                }
            }
            // Add new ctor if required
            if (addedToConstructor && newCTor)
            {
                type.Members.Add(ctor);
            }

            if (GeneratorContext.GeneratorParams.PropertyParams.GenerateShouldSerializeProperty)
            {
                foreach (string shouldSerialize in ShouldSerializeFields)
                {
                    CreateShouldSerializeMethod(type, shouldSerialize);
                }
            }

            // If don't use base class, generate all methods inside class
            if (!GeneratorContext.GeneratorParams.GenericBaseClass.Enabled)
            {
                if (!inheritsFromClass)
                {
                    if (GeneratorContext.GeneratorParams.EnableDataBinding)
                        CreateDataBinding(type);
                }
                if (GeneratorContext.GeneratorParams.Serialization.Enabled)
                {
                    CreateStaticSerializer(type);
                    CreateSerializeMethods(type);
                }

                if (GeneratorContext.GeneratorParams.GenerateCloneMethod)
                    CreateCloneMethod(type);

                // Add plublic ObjectChangeTracker property
                if (GeneratorContext.GeneratorParams.TrackingChanges.Enabled)
                    CreateChangeTrackerProperty(type);
            }

            if (GeneratorContext.GeneratorParams.PropertyParams.GeneratePropertyNameSpecified !=
                PropertyNameSpecifiedType.Default)
            {
                CodeDomHelper.GeneratePropertyNameSpecified(type, ref PropertiesListFields);
            }
            else
            {
                CodeDomHelper.GeneratePropertyNameSpecifiedNullable(type, ref PropertiesListFields);
            }
        }



        /// <summary>
        /// Creates the change tracker property.
        /// </summary>
        /// <param name="type">The type.</param>
        private void CreateChangeTrackerProperty(CodeTypeDeclaration type)
        {
            var changeTrackerPropertyPrivate = new CodeMemberField
                                                   {
                                                       Attributes = MemberAttributes.Final | MemberAttributes.Private,
                                                       Name = "changeTrackerField",
                                                       Type = new CodeTypeReference("ObjectChangeTracker")
                                                   };

            type.Members.Add(changeTrackerPropertyPrivate);

            var changeTrackerProperty = new CodeMemberProperty
                                            {
                                                Attributes = MemberAttributes.Final | MemberAttributes.Public,
                                                Name = "ChangeTracker",
                                                HasGet = true,
                                                Type = new CodeTypeReference("ObjectChangeTracker")
                                            };
            changeTrackerProperty.CustomAttributes.Add(new CodeAttributeDeclaration("XmlIgnore"));
            changeTrackerProperty.GetStatements.Add(CreateInstanceIfNotNull(changeTrackerPropertyPrivate.Name,
                                                                            changeTrackerProperty.Type,
                                                                            new CodeSnippetExpression("this")));
            changeTrackerProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeSnippetExpression("changeTrackerField")));
            type.Members.Add(changeTrackerProperty);
        }

        /// <summary>
        /// Create data binding
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateDataBinding(CodeTypeDeclaration type)
        {
            // -------------------------------------------------------------------------------
            // public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            // -------------------------------------------------------------------------------
            var propertyChangedEvent =
                new CodeMemberEvent
                    {
                        Attributes = MemberAttributes.Final | MemberAttributes.Public,
                        Name = "PropertyChanged",
                        Type =
                            new CodeTypeReference(typeof(PropertyChangedEventHandler))
                    };
            propertyChangedEvent.ImplementationTypes.Add(new CodeTypeReference("INotifyPropertyChanged"));
            type.Members.Add(propertyChangedEvent);

            CodeMemberMethod propertyChangedMethod = CodeDomHelper.CreatePropertyChangedMethod();

            type.Members.Add(propertyChangedMethod);
        }

        /// <summary>
        /// Creates the collection class.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="collName">Name of the coll.</param>
        protected virtual void CreateCollectionClass(CodeNamespace codeNamespace, string collName)
        {
            var ctd = new CodeTypeDeclaration(collName) { IsClass = true };
            ctd.BaseTypes.Add(new CodeTypeReference(GeneratorContext.GeneratorParams.CollectionBase,
                                                    new[] { new CodeTypeReference(CollectionTypes[collName]) }));

            ctd.IsPartial = true;

            bool newCTor = false;
            CodeConstructor ctor = GetConstructor(ctd, ref newCTor);

            ctd.Members.Add(ctor);
            codeNamespace.Types.Add(ctd);
        }

        /// <summary>
        /// Creates the clone method.
        /// </summary>
        /// <param name="codeTypeDeclaration">Represents a type declaration for a class, structure, interface, or enumeration.</param>
        protected virtual void CreateCloneMethod(CodeTypeDeclaration codeTypeDeclaration)
        {
            CodeTypeMember cloneMethod = CodeDomHelper.GetCloneMethod(codeTypeDeclaration);
            cloneMethod.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Clone method"));
            cloneMethod.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Clone method"));
            codeTypeDeclaration.Members.Add(cloneMethod);
        }

        /// <summary>
        /// Creates the serialize methods.
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration.</param>
        protected virtual void CreateSerializeMethods(CodeTypeDeclaration type)
        {
            // Serialize
            CodeMemberMethod ser = CreateSerializeMethod(type);
            ser.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Serialize/Deserialize"));
            type.Members.Add(ser);
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                type.Members.Add(GetOverrideSerializeMethod(type));
            }

            // Deserialize
            type.Members.AddRange(GetOverrideDeserializeMethods(type));
            type.Members.AddRange(GetDeserializeMethods(type));

            // SaveToFile
            type.Members.AddRange(GetOverrideSaveToFileMethods(type));
            type.Members.Add(GetSaveToFileMethod(type));

            // LoadFromFile
            type.Members.AddRange(GetOverrideLoadFromFileMethods(type));
            CodeMemberMethod lff = GetLoadFromFileMethod(type);
            lff.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Serialize/Deserialize"));
            type.Members.Add(lff);
        }

        /// <summary>
        /// Gets the serialize CodeDOM method.
        /// </summary>
        /// <param name="type">The type object to serilize.</param>
        /// <returns>return the CodeDOM serialize method</returns>
        protected virtual CodeMemberMethod CreateSerializeMethod(CodeTypeDeclaration type)
        {
            var serializeMethod = new CodeMemberMethod
                                      {
                                          Attributes = MemberAttributes.Public,
                                          Name = GeneratorContext.GeneratorParams.Serialization.SerializeMethodName
                                      };

            // TODO:Check if base class is used
            //if (type.BaseTypes.Count > 0)
            //    serializeMethod.Attributes |= MemberAttributes.Override;

            var tryStatmanentsCol = new CodeStatementCollection();
            var finallyStatmanentsCol = new CodeStatementCollection();

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                serializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                                                   typeof(Encoding), "encoding"));
            }
            // ------------------------------------------------------------
            // System.IO.StreamReader streamReader = null;
            // System.IO.MemoryStream memoryStream = null;
            // ------------------------------------------------------------
            serializeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(StreamReader)),
                    "streamReader",
                    new CodePrimitiveExpression(null)));

            serializeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(MemoryStream)),
                    "memoryStream",
                    new CodePrimitiveExpression(null)));

            tryStatmanentsCol.Add(new CodeAssignStatement(
                                      new CodeVariableReferenceExpression("memoryStream"),
                                      CodeDomHelper.CreateInstance(typeof(MemoryStream))));


            var textWriterOrStreamRef = new CodeTypeReferenceExpression("memoryStream");

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                tryStatmanentsCol.Add(
                    new CodeVariableDeclarationStatement(
                        new CodeTypeReference(typeof(XmlWriterSettings)),
                        "xmlWriterSettings",
                        CodeDomHelper.CreateInstance(typeof(XmlWriterSettings))));

                tryStatmanentsCol.Add(new CodeAssignStatement(
                                          new CodeVariableReferenceExpression(
                                              "xmlWriterSettings.Encoding"),
                                          new CodeArgumentReferenceExpression("encoding")));

                CodeMethodInvokeExpression createXmlWriter = CodeDomHelper.GetInvokeMethod("XmlWriter", "Create",
                                                                                           new CodeExpression[]
                                                                                               {
                                                                                                   new CodeArgumentReferenceExpression
                                                                                                       ("memoryStream"),
                                                                                                   new CodeArgumentReferenceExpression
                                                                                                       ("xmlWriterSettings")
                                                                                               });

                textWriterOrStreamRef = new CodeTypeReferenceExpression("xmlWriter");
                tryStatmanentsCol.Add(
                    new CodeVariableDeclarationStatement(
                        new CodeTypeReference(typeof(XmlWriter)),
                        "xmlWriter",
                        createXmlWriter));
            }

            // --------------------------------------------------------------------------
            // Serializer.Serialize(memoryStream, this);
            // --------------------------------------------------------------------------

            tryStatmanentsCol.Add(
                CodeDomHelper.GetInvokeMethod(
                    "Serializer",
                    "Serialize",
                    new CodeExpression[]
                        {
                            textWriterOrStreamRef,
                            new CodeThisReferenceExpression()
                        }));

            // ---------------------------------------------------------------------------
            // memoryStream.Seek(0, SeekOrigin.Begin);
            // System.IO.StreamReader streamReader = new System.IO.StreamReader(memoryStream);
            // ---------------------------------------------------------------------------
            tryStatmanentsCol.Add(
                CodeDomHelper.GetInvokeMethod(
                    "memoryStream",
                    "Seek",
                    new CodeExpression[]
                        {
                            new CodePrimitiveExpression(0),
                            new CodeTypeReferenceExpression("System.IO.SeekOrigin.Begin")
                        }));

            string[] streamReaderParams = GeneratorContext.GeneratorParams.Serialization.EnableEncoding
                                              ? new[] { "memoryStream", "encoding" }
                                              : new[] { "memoryStream" };
            tryStatmanentsCol.Add(new CodeAssignStatement(
                                      new CodeVariableReferenceExpression("streamReader"),
                                      CodeDomHelper.CreateInstance(typeof(StreamReader),
                                                                   streamReaderParams)));

            CodeMethodInvokeExpression readToEnd = CodeDomHelper.GetInvokeMethod("streamReader", "ReadToEnd");
            tryStatmanentsCol.Add(new CodeMethodReturnStatement(readToEnd));

            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("streamReader"));
            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("memoryStream"));

            var tryfinallyStmt = new CodeTryCatchFinallyStatement(tryStatmanentsCol.ToArray(),
                                                                  new CodeCatchClause[0],
                                                                  finallyStatmanentsCol.ToArray());
            serializeMethod.Statements.Add(tryfinallyStmt);

            serializeMethod.ReturnType = new CodeTypeReference(typeof(string));

            // --------
            // Comments
            // --------
            serializeMethod.Comments.AddRange(
                CodeDomHelper.GetSummaryComment(
                    string.Format("Serializes current {0} object into an XML document", type.Name)));

            serializeMethod.Comments.Add(CodeDomHelper.GetReturnComment("string XML value"));
            return serializeMethod;
        }

        /// <summary>
        /// Gets the serialize CodeDOM method.
        /// </summary>
        /// <param name="type">The type object to serilize.</param>
        /// <returns>return the CodeDOM serialize method</returns>
        protected virtual CodeMemberMethod GetOverrideSerializeMethod(CodeTypeDeclaration type)
        {
            var serializeMethod = new CodeMemberMethod
                                      {
                                          Attributes = MemberAttributes.Public,
                                          Name = GeneratorContext.GeneratorParams.Serialization.SerializeMethodName
                                      };

            var serializeMethodInvoke = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(null,
                                                  GeneratorContext.GeneratorParams.Serialization.SerializeMethodName),
                new CodeExpression[]
                    {
                        new CodeArgumentReferenceExpression(
                            GeneratorContext.GeneratorParams.Serialization.GetEncoderString())
                    });

            serializeMethod.Statements.Add(new CodeMethodReturnStatement(serializeMethodInvoke));
            serializeMethod.ReturnType = new CodeTypeReference(typeof(string));
            return serializeMethod;
        }

        /// <summary>
        /// Get Deserialize method
        /// </summary>
        /// <param name="type">represent a type declaration of class</param>
        /// <returns>Deserialize CodeMemberMethod</returns>
        protected virtual CodeMemberMethod[] GetDeserializeMethods(CodeTypeDeclaration type)
        {
            var methods = new CodeMemberMethod[2];
            string deserializeTypeName = GeneratorContext.GeneratorParams.GenericBaseClass.Enabled ? "T" : type.Name;

            // ---------------------------------------
            // public static T Deserialize(string xml)
            // ---------------------------------------
            var deserializeMethod = new CodeMemberMethod
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                            Name = GeneratorContext.GeneratorParams.Serialization.DeserializeMethodName
                                        };

            if (type.BaseTypes.Count > 0)
                deserializeMethod.Attributes |= MemberAttributes.New;

            deserializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "xml"));
            deserializeMethod.ReturnType = new CodeTypeReference(deserializeTypeName);

            deserializeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(StringReader)),
                    "stringReader",
                    new CodePrimitiveExpression(null)));

            var tryStatmanentsCol = new CodeStatementCollection();
            var finallyStatmanentsCol = new CodeStatementCollection();

            // ------------------------------------------------------
            // stringReader = new StringReader(xml);
            // ------------------------------------------------------
            var deserializeStatmanents = new CodeStatementCollection();

            tryStatmanentsCol.Add(new CodeAssignStatement(
                                      new CodeVariableReferenceExpression("stringReader"),
                                      new CodeObjectCreateExpression(
                                          new CodeTypeReference(typeof(StringReader)),
                                          new CodeExpression[] { new CodeArgumentReferenceExpression("xml") })));

            // ----------------------------------------------------------
            // obj = (ClassName)serializer.Deserialize(xmlReader);
            // return true;
            // ----------------------------------------------------------
            CodeMethodInvokeExpression deserialize = CodeDomHelper.GetInvokeMethod(
                "Serializer",
                "Deserialize",
                new CodeExpression[]
                    {
                        CodeDomHelper.GetInvokeMethod(
                            "System.Xml.XmlReader",
                            "Create",
                            new CodeExpression[] {new CodeVariableReferenceExpression("stringReader")})
                    });

            var castExpr = new CodeCastExpression(deserializeTypeName, deserialize);
            var returnStmt = new CodeMethodReturnStatement(castExpr);

            tryStatmanentsCol.Add(returnStmt);
            tryStatmanentsCol.AddRange(deserializeStatmanents);

            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("stringReader"));

            var tryfinallyStmt = new CodeTryCatchFinallyStatement(tryStatmanentsCol.ToArray(), new CodeCatchClause[0],
                                                                  finallyStatmanentsCol.ToArray());
            deserializeMethod.Statements.Add(tryfinallyStmt);

            methods[0] = deserializeMethod;

            // ---------------------------------------
            // public static T Deserialize(Stream s)
            // ---------------------------------------
            var deserializeFromStreamMethod = new CodeMemberMethod
                                                  {
                                                      Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                                      Name =
                                                          GeneratorContext.GeneratorParams.Serialization.
                                                          DeserializeMethodName
                                                  };
            // TODO:Check if base class is used
            //if (type.BaseTypes.Count > 0)
            //    deserializeFromStreamMethod.Attributes |= MemberAttributes.New;

            deserializeFromStreamMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Stream), "s"));
            deserializeFromStreamMethod.ReturnType = new CodeTypeReference(deserializeTypeName);


            // ----------------------------------------------------------
            // obj = (ClassName)serializer.Deserialize(xmlReader);
            // return true;
            // ----------------------------------------------------------
            deserialize = CodeDomHelper.GetInvokeMethod(
                "Serializer",
                "Deserialize",
                new CodeExpression[]
                    {
                        new CodeArgumentReferenceExpression("s")
                    });

            castExpr = new CodeCastExpression(deserializeTypeName, deserialize);
            returnStmt = new CodeMethodReturnStatement(castExpr);
            deserializeFromStreamMethod.Statements.Add(returnStmt);

            methods[1] = deserializeFromStreamMethod;

            return methods;
        }

        /// <summary>
        /// Get Deserialize method
        /// </summary>
        /// <param name="type">represent a type declaration of class</param>
        /// <returns>Deserialize CodeMemberMethod</returns>
        protected virtual CodeMemberMethod[] GetOverrideDeserializeMethods(CodeTypeDeclaration type)
        {
            var deserializeMethodList = new List<CodeMemberMethod>();
            string deserializeTypeName = GeneratorContext.GeneratorParams.GenericBaseClass.Enabled ? "T" : type.Name;

            // -------------------------------------------------------------------------------------
            // public static bool Deserialize(string xml, out T obj, out System.Exception exception)
            // -------------------------------------------------------------------------------------
            var deserializeMethod = new CodeMemberMethod
                                        {
                                            Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                            Name = GeneratorContext.GeneratorParams.Serialization.DeserializeMethodName
                                        };

            deserializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "xml"));

            var param = new CodeParameterDeclarationExpression(deserializeTypeName, "obj") { Direction = FieldDirection.Out };
            deserializeMethod.Parameters.Add(param);

            param = new CodeParameterDeclarationExpression(typeof(Exception), "exception") { Direction = FieldDirection.Out };

            deserializeMethod.Parameters.Add(param);

            deserializeMethod.ReturnType = new CodeTypeReference(typeof(bool));

            // -----------------
            // exception = null;
            // -----------------
            deserializeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("exception"),
                    new CodePrimitiveExpression(null)));

            // -----------------
            // obj = default(T);
            // -----------------
            deserializeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("obj"),
                    new CodeDefaultValueExpression(new CodeTypeReference(deserializeTypeName))
                    ));

            // ---------------------
            // try {...} catch {...}
            // ---------------------
            var tryStatmanentsCol = new CodeStatementCollection();

            // Call Desrialize method
            var deserializeInvoke =
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.
                                                          DeserializeMethodName),
                    new CodeExpression[] { new CodeArgumentReferenceExpression("xml") });

            tryStatmanentsCol.Add(
                new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("obj"),
                    deserializeInvoke));

            tryStatmanentsCol.Add(CodeDomHelper.GetReturnTrue());

            // catch
            CodeCatchClause[] catchClauses = CodeDomHelper.GetCatchClause();

            var trycatch = new CodeTryCatchFinallyStatement(tryStatmanentsCol.ToArray(), catchClauses);
            deserializeMethod.Statements.Add(trycatch);

            // --------
            // Comments
            // --------
            deserializeMethod.Comments.AddRange(
                CodeDomHelper.GetSummaryComment(string.Format("Deserializes workflow markup into an {0} object",
                                                              type.Name)));

            deserializeMethod.Comments.Add(CodeDomHelper.GetParamComment("xml", "string workflow markup to deserialize"));
            deserializeMethod.Comments.Add(CodeDomHelper.GetParamComment("obj",
                                                                         string.Format("Output {0} object", type.Name)));
            deserializeMethod.Comments.Add(CodeDomHelper.GetParamComment("exception",
                                                                         "output Exception value if deserialize failed"));

            deserializeMethod.Comments.Add(
                CodeDomHelper.GetReturnComment("true if this XmlSerializer can deserialize the object; otherwise, false"));
            deserializeMethodList.Add(deserializeMethod);

            // -----------------------------------------------------
            // public static bool Deserialize(string xml, out T obj)
            // -----------------------------------------------------
            deserializeMethod = new CodeMemberMethod
                                    {
                                        Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                        Name = GeneratorContext.GeneratorParams.Serialization.DeserializeMethodName
                                    };
            deserializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "xml"));
            deserializeMethod.ReturnType = new CodeTypeReference(typeof(bool));

            param = new CodeParameterDeclarationExpression(deserializeTypeName, "obj") { Direction = FieldDirection.Out };
            deserializeMethod.Parameters.Add(param);

            // ---------------------------
            // Exception exception = null;
            // ---------------------------
            deserializeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(typeof(Exception), "exception", new CodePrimitiveExpression(null)));

            // ------------------------------------------------
            // return Deserialize(xml, out obj, out exception);
            // ------------------------------------------------
            var xmlStringParam = new CodeArgumentReferenceExpression("xml");
            var objParam = new CodeDirectionExpression(
                FieldDirection.Out, new CodeFieldReferenceExpression(null, "obj"));

            var expParam = new CodeDirectionExpression(
                FieldDirection.Out, new CodeFieldReferenceExpression(null, "exception"));

            deserializeInvoke =
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.
                                                          DeserializeMethodName),
                    new CodeExpression[] { xmlStringParam, objParam, expParam });

            var returnStmt = new CodeMethodReturnStatement(deserializeInvoke);
            deserializeMethod.Statements.Add(returnStmt);
            deserializeMethodList.Add(deserializeMethod);
            return deserializeMethodList.ToArray();
        }

        /// <summary>
        /// Gets the save to file code DOM method.
        /// </summary>
        /// <returns>
        /// return the save to file code DOM method statment
        /// </returns>
        protected virtual CodeMemberMethod GetSaveToFileMethod(CodeTypeDeclaration type)
        {
            // -----------------------------------------------
            // public virtual void SaveToFile(string fileName)
            // -----------------------------------------------
            var saveToFileMethod = new CodeMemberMethod
                                       {
                                           Attributes = MemberAttributes.Public,
                                           Name = GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName
                                       };
            // TODO:Check if base class is used
            //if (type.BaseTypes.Count > 0)
            //    saveToFileMethod.Attributes |= MemberAttributes.Override;

            saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Encoding), "encoding"));
            }

            saveToFileMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(StreamWriter)),
                    "streamWriter",
                    new CodePrimitiveExpression(null)));

            // ------------------------
            // try {...} finally {...}
            // -----------------------
            var tryExpression = new CodeStatementCollection();

            // ---------------------------------------
            // string xmlString = Serialize(encoding);
            // ---------------------------------------

            CodeMethodInvokeExpression serializeMethodInvoke;
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                serializeMethodInvoke = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.SerializeMethodName),
                    new CodeExpression[] { new CodeArgumentReferenceExpression("encoding") });
            }
            else
            {
                serializeMethodInvoke = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.SerializeMethodName));
            }


            var xmlString = new CodeVariableDeclarationStatement(
                new CodeTypeReference(typeof(string)), "xmlString", serializeMethodInvoke);

            tryExpression.Add(xmlString);

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                // ----------------------------------------------------------------
                // streamWriter = new StreamWriter(fileName, false, Encoding.UTF8);
                // ----------------------------------------------------------------
                tryExpression.Add(new CodeAssignStatement(
                                      new CodeVariableReferenceExpression("streamWriter"),
                                      new CodeObjectCreateExpression(
                                          typeof(StreamWriter),
                                          new CodeExpression[]
                                              {
                                                  new CodeSnippetExpression("fileName"),
                                                  new CodeSnippetExpression("false"),
                                                  new CodeSnippetExpression(
                                                      GeneratorContext.GeneratorParams.Serialization.GetEncoderString())
                                              })));
            }
            else
            {
                // --------------------------------------------------------------
                // System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                // --------------------------------------------------------------
                tryExpression.Add(CodeDomHelper.CreateObject(typeof(FileInfo), "xmlFile", new[] { "fileName" }));

                // ----------------------------------------
                // StreamWriter Tex = xmlFile.CreateText();
                // ----------------------------------------
                CodeMethodInvokeExpression createTextMethodInvoke = CodeDomHelper.GetInvokeMethod("xmlFile",
                                                                                                  "CreateText");

                tryExpression.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("streamWriter"),
                        createTextMethodInvoke));
            }
            // ----------------------------------
            // streamWriter.WriteLine(xmlString);
            // ----------------------------------
            CodeMethodInvokeExpression writeLineMethodInvoke =
                CodeDomHelper.GetInvokeMethod(
                    "streamWriter",
                    "WriteLine",
                    new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("xmlString")
                        });

            tryExpression.Add(writeLineMethodInvoke);
            CodeMethodInvokeExpression closeMethodInvoke = CodeDomHelper.GetInvokeMethod("streamWriter", "Close");

            tryExpression.Add(closeMethodInvoke);

            var finallyStatmanentsCol = new CodeStatementCollection();
            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("streamWriter"));

            var trycatch = new CodeTryCatchFinallyStatement(tryExpression.ToArray(), new CodeCatchClause[0],
                                                            finallyStatmanentsCol.ToArray());
            saveToFileMethod.Statements.Add(trycatch);

            return saveToFileMethod;
        }

        /// <summary>
        /// Gets the save to file code DOM method.
        /// </summary>
        /// <param name="type">CodeTypeDeclaration type.</param>
        /// <returns>
        /// return the save to file code DOM method statment
        /// </returns>
        protected virtual CodeMemberMethod[] GetOverrideSaveToFileMethods(CodeTypeDeclaration type)
        {
            var saveToFileMethodList = new List<CodeMemberMethod>();
            var saveToFileMethod = new CodeMemberMethod
                                       {
                                           Attributes = MemberAttributes.Public,
                                           Name = GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName
                                       };
            // TODO:Check if base class is used
            //if (type.BaseTypes.Count > 0)
            //    saveToFileMethod.Attributes |= MemberAttributes.Override;

            saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Encoding), "encoding"));
            }

            var paramException = new CodeParameterDeclarationExpression(
                typeof(Exception), "exception")
                                     {
                                         Direction = FieldDirection.Out
                                     };

            saveToFileMethod.Parameters.Add(paramException);

            saveToFileMethod.ReturnType = new CodeTypeReference(typeof(bool));

            saveToFileMethod.Statements.Add(
                new CodeAssignStatement(new CodeArgumentReferenceExpression("exception"),
                                        new CodePrimitiveExpression(null)));

            // ---------------------
            // try {...} catch {...}
            // ---------------------
            var tryExpression = new CodeStatementCollection();

            // ---------------------
            // SaveToFile(fileName);
            // ---------------------
            var xmlStringParam = new CodeArgumentReferenceExpression("fileName");

            CodeMethodInvokeExpression saveToFileInvoke;
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                saveToFileInvoke =
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(null,
                                                          GeneratorContext.GeneratorParams.Serialization.
                                                              SaveToFileMethodName),
                        new CodeExpression[] { xmlStringParam, new CodeArgumentReferenceExpression("encoding") });
            }
            else
            {
                saveToFileInvoke =
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(null,
                                                          GeneratorContext.GeneratorParams.Serialization.
                                                              SaveToFileMethodName),
                        new CodeExpression[] { xmlStringParam });
            }

            tryExpression.Add(saveToFileInvoke);
            tryExpression.Add(CodeDomHelper.GetReturnTrue());

            // -----------
            // Catch {...}
            // -----------
            var catchstmts = new CodeStatementCollection();
            catchstmts.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression("exception"),
                                                   new CodeVariableReferenceExpression("e")));

            catchstmts.Add(CodeDomHelper.GetReturnFalse());
            var codeCatchClause = new CodeCatchClause("e", new CodeTypeReference(typeof(Exception)),
                                                      catchstmts.ToArray());

            var codeCatchClauses = new[] { codeCatchClause };

            var trycatch = new CodeTryCatchFinallyStatement(tryExpression.ToArray(), codeCatchClauses);
            saveToFileMethod.Statements.Add(trycatch);

            saveToFileMethod.Comments.AddRange(
                CodeDomHelper.GetSummaryComment(string.Format("Serializes current {0} object into file", type.Name)));

            saveToFileMethod.Comments.Add(CodeDomHelper.GetParamComment("fileName", "full path of outupt xml file"));
            saveToFileMethod.Comments.Add(CodeDomHelper.GetParamComment("exception", "output Exception value if failed"));
            saveToFileMethod.Comments.Add(
                CodeDomHelper.GetReturnComment("true if can serialize and save into file; otherwise, false"));

            saveToFileMethodList.Add(saveToFileMethod);

            //--------------------------------------------------------------------------------
            // public virtual bool SaveToFile(string fileName, out System.Exception exception)
            //--------------------------------------------------------------------------------
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                saveToFileMethod = new CodeMemberMethod
                                       {
                                           Attributes = MemberAttributes.Public,
                                           Name = GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName
                                       };

                // TODO:Check if base class is used
                //if (type.BaseTypes.Count > 0)
                //    saveToFileMethod.Attributes |= MemberAttributes.Override;

                CodeExpression[] encodeingArgs;
                encodeingArgs = new CodeExpression[]
                                    {
                                        xmlStringParam,
                                        new CodeArgumentReferenceExpression(
                                            GeneratorContext.GeneratorParams.Serialization.GetEncoderString()),
                                        new CodeDirectionExpression(FieldDirection.Out,
                                                                    new CodeSnippetExpression("exception"))
                                    };

                var saveToFileMethodInvoke = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        null,
                        GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName), encodeingArgs);

                saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));
                saveToFileMethod.Parameters.Add(paramException);
                saveToFileMethod.Statements.Add(new CodeMethodReturnStatement(saveToFileMethodInvoke));
                saveToFileMethod.ReturnType = new CodeTypeReference(typeof(bool));

                saveToFileMethodList.Add(saveToFileMethod);
            }
            //--------------------------------------------------------------------------------
            // public virtual void SaveToFile(string fileName)
            //--------------------------------------------------------------------------------
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                saveToFileMethod = new CodeMemberMethod
                                       {
                                           Attributes = MemberAttributes.Public,
                                           Name = GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName
                                       };

                // TODO:Check if base class is used
                //if (type.BaseTypes.Count > 0)
                //    saveToFileMethod.Attributes |= MemberAttributes.Override;

                var encodeingArgs = new CodeExpression[]
                                        {
                                            xmlStringParam,
                                            new CodeArgumentReferenceExpression(
                                                GeneratorContext.GeneratorParams.Serialization.GetEncoderString())
                                        };

                var saveToFileMethodInvoke = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        null,
                        GeneratorContext.GeneratorParams.Serialization.SaveToFileMethodName), encodeingArgs);

                saveToFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));
                saveToFileMethod.Statements.Add(saveToFileMethodInvoke);
                saveToFileMethodList.Add(saveToFileMethod);
            }
            return saveToFileMethodList.ToArray();
        }

        /// <summary>
        /// Gets the load from file CodeDOM method.
        /// </summary>
        /// <param name="type">The type CodeTypeDeclaration.</param>
        /// <returns>return the codeDom LoadFromFile method</returns>
        protected virtual CodeMemberMethod GetLoadFromFileMethod(CodeTypeDeclaration type)
        {
            string typeName = GeneratorContext.GeneratorParams.GenericBaseClass.Enabled ? "T" : type.Name;

            // ---------------------------------------------
            // public static T LoadFromFile(string fileName)
            // ---------------------------------------------
            var loadFromFileMethod = new CodeMemberMethod
                                         {
                                             Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                             Name =
                                                 GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName
                                         };

            if (type.BaseTypes.Count > 0)
                loadFromFileMethod.Attributes |= MemberAttributes.New;

            loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Encoding), "encoding"));
            }

            loadFromFileMethod.ReturnType = new CodeTypeReference(typeName);

            loadFromFileMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(FileStream)),
                    "file",
                    new CodePrimitiveExpression(null)));

            loadFromFileMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(StreamReader)),
                    "sr",
                    new CodePrimitiveExpression(null)));

            var tryStatmanentsCol = new CodeStatementCollection();
            var finallyStatmanentsCol = new CodeStatementCollection();

            // ---------------------------------------------------------------------------
            // file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            // sr = new StreamReader(file);
            // ---------------------------------------------------------------------------
            tryStatmanentsCol.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("file"),
                    new CodeObjectCreateExpression(
                        typeof(FileStream),
                        new CodeExpression[]
                            {
                                new CodeArgumentReferenceExpression("fileName"),
                                CodeDomHelper.GetEnum("FileMode", "Open"),
                                CodeDomHelper.GetEnum("FileAccess", "Read")
                            })));

            CodeExpression[] codeParamExpression;
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                codeParamExpression = new CodeExpression[]
                                          {
                                              new CodeVariableReferenceExpression("file"),
                                              new CodeVariableReferenceExpression("encoding")
                                          };
            }
            else
            {
                codeParamExpression = new CodeExpression[] { new CodeVariableReferenceExpression("file") };
            }
            tryStatmanentsCol.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("sr"),
                    new CodeObjectCreateExpression(
                        typeof(StreamReader), codeParamExpression
                        )));
            // ----------------------------------
            // string xmlString = sr.ReadToEnd();
            // ----------------------------------
            CodeMethodInvokeExpression readToEndInvoke = CodeDomHelper.GetInvokeMethod("sr", "ReadToEnd");

            var xmlString = new CodeVariableDeclarationStatement(
                new CodeTypeReference(typeof(string)), "xmlString", readToEndInvoke);

            tryStatmanentsCol.Add(xmlString);
            tryStatmanentsCol.Add(CodeDomHelper.GetInvokeMethod("sr", "Close"));
            tryStatmanentsCol.Add(CodeDomHelper.GetInvokeMethod("file", "Close"));

            // ------------------------------------------------------
            // return Deserialize(xmlString, out obj, out exception);
            // ------------------------------------------------------            
            var fileName = new CodeVariableReferenceExpression("xmlString");

            var deserializeInvoke =
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.
                                                          DeserializeMethodName),
                    new CodeExpression[] { fileName });

            var rstmts = new CodeMethodReturnStatement(deserializeInvoke);
            tryStatmanentsCol.Add(rstmts);

            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("file"));
            finallyStatmanentsCol.Add(CodeDomHelper.GetDispose("sr"));

            var tryfinally = new CodeTryCatchFinallyStatement(
                CodeDomHelper.CodeStmtColToArray(tryStatmanentsCol), new CodeCatchClause[0],
                CodeDomHelper.CodeStmtColToArray(finallyStatmanentsCol));

            loadFromFileMethod.Statements.Add(tryfinally);

            return loadFromFileMethod;
        }

        /// <summary>
        /// Gets the load from file CodeDOM method.
        /// </summary>
        /// <param name="type">The type CodeTypeDeclaration.</param>
        /// <returns>return the codeDom LoadFromFile method</returns>
        protected virtual CodeMemberMethod[] GetOverrideLoadFromFileMethods(CodeTypeDeclaration type)
        {
            string typeName = GeneratorContext.GeneratorParams.GenericBaseClass.Enabled ? "T" : type.Name;

            var teeType = new CodeTypeReference(typeName);
            var loadFromFileMethodList = new List<CodeMemberMethod>();
            var loadFromFileMethod = new CodeMemberMethod
                                         {
                                             Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                             Name =
                                                 GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName
                                         };

            loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Encoding), "encoding"));
            }

            var param = new CodeParameterDeclarationExpression(typeName, "obj") { Direction = FieldDirection.Out };
            loadFromFileMethod.Parameters.Add(param);

            param = new CodeParameterDeclarationExpression(typeof(Exception), "exception") { Direction = FieldDirection.Out };

            loadFromFileMethod.Parameters.Add(param);
            loadFromFileMethod.ReturnType = new CodeTypeReference(typeof(bool));

            // -----------------
            // exception = null;
            // obj = null;
            // -----------------
            loadFromFileMethod.Statements.Add(
                new CodeAssignStatement(new CodeArgumentReferenceExpression("exception"),
                                        new CodePrimitiveExpression(null)));

            loadFromFileMethod.Statements.Add(
                new CodeAssignStatement(new CodeArgumentReferenceExpression("obj"),
                                        new CodeDefaultValueExpression(teeType)));

            var tryStatmanentsCol = new CodeStatementCollection();

            // Call LoadFromFile method

            CodeExpression[] codeParamExpression;
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                codeParamExpression = new CodeExpression[]
                                          {
                                              new CodeArgumentReferenceExpression("fileName"),
                                              new CodeArgumentReferenceExpression("encoding")
                                          };
            }
            else
            {
                codeParamExpression = new CodeExpression[]
                                          {
                                              new CodeArgumentReferenceExpression("fileName")
                                          };
            }

            var loadFromFileInvoke =
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.
                                                          LoadFromFileMethodName), codeParamExpression);

            tryStatmanentsCol.Add(
                new CodeAssignStatement(
                    new CodeArgumentReferenceExpression("obj"),
                    loadFromFileInvoke));

            tryStatmanentsCol.Add(CodeDomHelper.GetReturnTrue());

            var trycatch = new CodeTryCatchFinallyStatement(
                CodeDomHelper.CodeStmtColToArray(tryStatmanentsCol), CodeDomHelper.GetCatchClause());

            loadFromFileMethod.Statements.Add(trycatch);

            loadFromFileMethod.Comments.AddRange(
                CodeDomHelper.GetSummaryComment(
                    string.Format("Deserializes xml markup from file into an {0} object", type.Name)));

            loadFromFileMethod.Comments.Add(CodeDomHelper.GetParamComment("fileName",
                                                                          "string xml file to load and deserialize"));
            loadFromFileMethod.Comments.Add(CodeDomHelper.GetParamComment("obj",
                                                                          string.Format("Output {0} object", type.Name)));
            loadFromFileMethod.Comments.Add(CodeDomHelper.GetParamComment("exception",
                                                                          "output Exception value if deserialize failed"));

            loadFromFileMethod.Comments.Add(
                CodeDomHelper.GetReturnComment("true if this XmlSerializer can deserialize the object; otherwise, false"));

            loadFromFileMethodList.Add(loadFromFileMethod);

            //-----------------------------------------------------
            // 
            //-----------------------------------------------------
            var fileName = new CodeArgumentReferenceExpression("fileName");
            var encoder =
                new CodeArgumentReferenceExpression(GeneratorContext.GeneratorParams.Serialization.GetEncoderString());
            var objParam = new CodeDirectionExpression(FieldDirection.Out, new CodeFieldReferenceExpression(null, "obj"));
            var expParam = new CodeDirectionExpression(FieldDirection.Out,
                                                       new CodeFieldReferenceExpression(null, "exception"));

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                loadFromFileMethod = new CodeMemberMethod
                                         {
                                             Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                             Name =
                                                 GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName
                                         };

                loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));

                param = new CodeParameterDeclarationExpression(typeName, "obj") { Direction = FieldDirection.Out };
                loadFromFileMethod.Parameters.Add(param);
                param = new CodeParameterDeclarationExpression(typeof(Exception), "exception") { Direction = FieldDirection.Out };
                loadFromFileMethod.Parameters.Add(param);
                loadFromFileMethod.ReturnType = new CodeTypeReference(typeof(bool));
                var loadFromFileMethodInvoke =
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(null,
                                                          GeneratorContext.GeneratorParams.Serialization.
                                                              LoadFromFileMethodName),
                        new CodeExpression[] { fileName, encoder, objParam, expParam });

                loadFromFileMethod.Statements.Add(new CodeMethodReturnStatement(loadFromFileMethodInvoke));
                loadFromFileMethodList.Add(loadFromFileMethod);
            }

            // ------------------------------------------------------
            // public static bool LoadFromFile(string xml, out T obj)
            // ------------------------------------------------------
            loadFromFileMethod = new CodeMemberMethod
                                     {
                                         Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                         Name = GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName
                                     };
            loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));
            loadFromFileMethod.ReturnType = new CodeTypeReference(typeof(bool));

            param = new CodeParameterDeclarationExpression(typeName, "obj") { Direction = FieldDirection.Out };
            loadFromFileMethod.Parameters.Add(param);

            // ---------------------------
            // Exception exception = null;
            // ---------------------------
            loadFromFileMethod.Statements.Add(
                new CodeVariableDeclarationStatement(typeof(Exception), "exception", new CodePrimitiveExpression(null)));

            // ------------------------------------------------------
            // return LoadFromFile(fileName, out obj, out exception);
            // ------------------------------------------------------
            var loadFromFileMethodInvok =
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null,
                                                      GeneratorContext.GeneratorParams.Serialization.
                                                          LoadFromFileMethodName),
                    new CodeExpression[] { fileName, objParam, expParam });

            var returnStmt = new CodeMethodReturnStatement(loadFromFileMethodInvok);
            loadFromFileMethod.Statements.Add(returnStmt);
            loadFromFileMethodList.Add(loadFromFileMethod);
            //-----------------------------------------------------------
            // public static [TypeOfObject] LoadFromFile(string fileName)
            //-----------------------------------------------------------
            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                loadFromFileMethod = new CodeMemberMethod
                                         {
                                             Attributes = MemberAttributes.Public | MemberAttributes.Static,
                                             Name =
                                                 GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName
                                         };
                loadFromFileMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "fileName"));
                loadFromFileMethod.ReturnType = new CodeTypeReference(typeName);

                var loadFromFileMethodInvoke = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        null,
                        GeneratorContext.GeneratorParams.Serialization.LoadFromFileMethodName),
                    new CodeArgumentReferenceExpression("fileName"),
                    new CodeArgumentReferenceExpression(
                        GeneratorContext.GeneratorParams.Serialization.GetEncoderString()));

                returnStmt = new CodeMethodReturnStatement(loadFromFileMethodInvoke);
                loadFromFileMethod.Statements.Add(returnStmt);
                loadFromFileMethodList.Add(loadFromFileMethod);
            }
            return loadFromFileMethodList.ToArray();
        }

        /// <summary>
        /// Import namespaces
        /// </summary>
        /// <param name="code">Code namespace</param>
        protected virtual void ImportNamespaces(CodeNamespace code)
        {
            code.Imports.Add(new CodeNamespaceImport("System"));
            code.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            code.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
            code.Imports.Add(new CodeNamespaceImport("System.Collections"));
            code.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
            code.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));

            if (GeneratorContext.GeneratorParams.CustomUsings != null)
            {
                foreach (NamespaceParam item in GeneratorContext.GeneratorParams.CustomUsings)
                    code.Imports.Add(new CodeNamespaceImport(item.NameSpace));
            }

            // Tracking changes
            if (GeneratorContext.GeneratorParams.Language == GenerationLanguage.CSharp &&
                GeneratorContext.GeneratorParams.TrackingChanges.Enabled)
            {
                if (GeneratorContext.GeneratorParams.TrackingChanges.GenerateTrackingClasses)
                {
                    code.Imports.Add(new CodeNamespaceImport("System.Collections.Specialized"));
                    code.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization"));
                    code.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
                    code.Imports.Add(new CodeNamespaceImport("System.Reflection"));
                }
            }

            if (GeneratorContext.GeneratorParams.Serialization.Enabled)
            {
                code.Imports.Add(new CodeNamespaceImport("System.IO"));
                code.Imports.Add(new CodeNamespaceImport("System.Text"));
            }

            if (GeneratorContext.GeneratorParams.Serialization.EnableEncoding)
            {
                code.Imports.Add(new CodeNamespaceImport("System.Xml"));
            }
            switch (GeneratorContext.GeneratorParams.CollectionObjectType)
            {
                case CollectionType.IList:
                case CollectionType.List:
                    code.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                    break;
                case CollectionType.ObservableCollection:
                    code.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
                    break;
                default:
                    break;
            }

            code.Name = GeneratorContext.GeneratorParams.NameSpace;
        }

        /// <summary>
        /// Create data contract attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        /// <param name="schema">XML schema</param>
        protected virtual void CreateDataContractAttribute(CodeTypeDeclaration type, XmlSchema schema)
        {
            // abstract
        }

        /// <summary>
        /// Creates the data member attribute.
        /// </summary>
        /// <param name="prop">Represents a declaration for a property of a type.</param>
        protected virtual void CreateDataMemberAttribute(CodeMemberProperty prop)
        {
            // abstract
        }

        /// <summary>
        /// Field process.
        /// </summary>
        /// <param name="member">CodeTypeMember member</param>
        /// <param name="ctor">CodeMemberMethod constructor</param>
        /// <param name="ns">CodeNamespace XSD</param>
        /// <param name="addedToConstructor">Indicates if create a new constructor</param>
        protected virtual void ProcessFields(
            CodeTypeMember member,
            CodeMemberMethod ctor,
            CodeNamespace ns,
            ref bool addedToConstructor)
        {
            var field = (CodeMemberField)member;

            // ---------------------------------------------
            // [EditorBrowsable(EditorBrowsableState.Never)]
            // ---------------------------------------------
            if (member.Attributes == MemberAttributes.Private)
            {
                if (GeneratorContext.GeneratorParams.Miscellaneous.HidePrivateFieldInIde)
                {
                    var attributeType = new CodeTypeReference(
                        typeof(EditorBrowsableAttribute).Name.Replace("Attribute", string.Empty));

                    var argument = new CodeAttributeArgument
                                       {
                                           Value = CodeDomHelper.GetEnum(typeof(EditorBrowsableState).Name, "Never")
                                       };

                    field.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, new[] { argument }));
                }
            }

            // ------------------------------------------
            // protected virtual  List <Actor> nameField;
            // ------------------------------------------
            bool thisIsCollectionType = field.Type.ArrayElementType != null;
            if (thisIsCollectionType)
            {
                field.Type = GetCollectionType(field.Type);

                // PC, 2012-08-11 : Fix 15483 : With PascalCaseProperty, type of collection not renamed
                if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                {
                    foreach (CodeTypeReference typeArgument in field.Type.TypeArguments)
                    {
                        if (TypeList.Contains(typeArgument.BaseType))
                        {
                            typeArgument.BaseType = CodeDomHelper.GetPascalCaseName(typeArgument.BaseType);
                        }
                    }
                }
            }

            // ---------------------------------------
            // if ((this.nameField == null))
            // {
            //    this.nameField = new List<Name>();
            // }
            // ---------------------------------------
            switch (GeneratorContext.GeneratorParams.InitializeFields)
            {
                case InitializeFieldsType.Collections:
                    if (!thisIsCollectionType)
                    {
                        return;
                    }

                    if (GeneratorContext.GeneratorParams.CollectionObjectType == CollectionType.Array)
                    {
                        return;
                    }

                    break;
                case InitializeFieldsType.All:
                    if (thisIsCollectionType)
                    {
                        if (GeneratorContext.GeneratorParams.CollectionObjectType == CollectionType.Array)
                        {
                            return;
                        }
                    }
                    else
                    {
                        bool finded;
                        CodeTypeDeclaration declaration = FindTypeInNamespace(field.Type.BaseType, ns, out finded);
                        if ((declaration == null) || !declaration.IsClass ||
                            ((declaration.TypeAttributes & TypeAttributes.Abstract) == TypeAttributes.Abstract))
                        {
                            return;
                        }
                    }

                    break;
                default:
                    return;
            }


            if (GeneratorContext.GeneratorParams.PropertyParams.EnableLazyLoading)
            {
                if (field.Type.BaseType != typeof(byte).FullName)
                {
                    LazyLoadingFields.Add(field.Name);
                }
            }
            else
            {
                if (field.Type.BaseType != typeof(byte).FullName)
                {
                    ctor.Statements.Insert(0, CreateInstance(field.Name, field.Type));
                    addedToConstructor = true;
                }
            }
        }

        /// <summary>
        /// Create a Class Constructor
        /// </summary>
        /// <param name="type">type of declaration</param>
        /// <returns>return CodeConstructor</returns>
        protected virtual CodeConstructor CreateClassConstructor(CodeTypeDeclaration type)
        {
            var ctor = new CodeConstructor { Attributes = MemberAttributes.Public, Name = type.Name };
            return ctor;
        }

        /// <summary>
        /// Create new instance of object
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="type">CodeTypeReference Type</param>
        /// <returns>return instance CodeConditionStatement</returns>
        protected virtual CodeConditionStatement CreateInstanceIfNotNull(string name, CodeTypeReference type,
                                                                         params CodeExpression[] parameters)
        {
            CodeAssignStatement statement;
            if (type.BaseType.Equals("System.String") && type.ArrayRank == 0)
            {
                statement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeDomHelper.GetStaticField(typeof(String), "Empty"));
            }
            else
            {
                statement = CodeDomHelper.CollectionInitilializerStatement(name, type, parameters);
            }

            return
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    new CodeStatement[] { statement });
        }

        /// <summary>
        /// Create new instance of object
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="type">CodeTypeReference Type</param>
        /// <returns>return instance CodeConditionStatement</returns>
        protected virtual CodeAssignStatement CreateInstance(string name, CodeTypeReference type)
        {
            CodeAssignStatement statement;
            if (type.BaseType.Equals("System.String") && type.ArrayRank == 0)
            {
                statement =
                    new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                                            CodeDomHelper.GetStaticField(typeof(String), "Empty"));
            }
            else
            {
                statement = CodeDomHelper.CollectionInitilializerStatement(name, type);
            }
            return statement;
        }

        /// <summary>
        /// Recherche le CodeTypeDeclaration d'un objet en fonction de son type de base (nom de classe)
        /// </summary>
        /// <param name="typeName">Search name</param>
        /// <param name="ns">Seach into</param>
        /// <param name="finded">if set to <c>true</c> [finded].</param>
        /// <returns>CodeTypeDeclaration found</returns>
        protected virtual CodeTypeDeclaration FindTypeInNamespace(string typeName, CodeNamespace ns, out bool finded)
        {
            finded = false;
            foreach (CodeTypeDeclaration declaration in ns.Types)
            {
                if (declaration.Name == typeName)
                {
                    finded = true;
                    return declaration;
                }
            }
            return null;
        }

        /// <summary>
        /// Property process
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        /// <param name="ns">The ns.</param>
        /// <param name="member">Type members include fields, methods, properties, constructors and nested types</param>
        /// <param name="schema">XML Schema</param>
        protected virtual void ProcessProperty(CodeTypeDeclaration type, CodeNamespace ns, CodeTypeMember member,
                                               XmlSchema schema)
        {
            SummaryCommentsHelper.ProcessPropertyComments(member, schema);
            var prop = (CodeMemberProperty)member;

            if (prop.Type.ArrayElementType != null)
            {
                prop.Type = this.GetCollectionType(prop.Type);
                CollectionTypesFields.Add(prop.Name);

                // PC, 2012-08-11 : Fix 15483 : With PascalCaseProperty, type of collection not renamed
                if (GeneratorContext.GeneratorParams.PropertyParams.PascalCaseProperty)
                {
                    foreach (CodeTypeReference typeArgument in prop.Type.TypeArguments)
                    {
                        if (TypeList.Contains(typeArgument.BaseType))
                        {
                            typeArgument.BaseType = CodeDomHelper.GetPascalCaseName(typeArgument.BaseType);
                        }
                    }

                }
            }

            if (GeneratorContext.GeneratorParams.PropertyParams.EnableVirtualProperties)
            {
                prop.Attributes ^= MemberAttributes.Final;
            }

            if (prop.Type.BaseType.Contains("System.Nullable"))
            {
                ShouldSerializeFields.Add(prop.Name);
            }

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                CreateDataMemberAttribute(prop);
            }

            if (GeneratorContext.GeneratorParams.InitializeFields != InitializeFieldsType.None)
            {
                var propReturnStatment = prop.GetStatements[0] as CodeMethodReturnStatement;
                if (propReturnStatment != null)
                {
                    var field = propReturnStatment.Expression as CodeFieldReferenceExpression;
                    if (field != null)
                    {
                        if (LazyLoadingFields.IndexOf(field.FieldName) != -1)
                        {
                            prop.GetStatements.Insert(0, CreateInstanceIfNotNull(field.FieldName, prop.Type));
                        }
                    }
                }
            }

            // Add OnPropertyChanged in setter
            if (GeneratorContext.GeneratorParams.EnableDataBinding)
            {
                if (type.BaseTypes.IndexOf(new CodeTypeReference(typeof(CollectionBase))) == -1)
                {
                    // -----------------------------
                    // if (handler != null) {
                    //    OnPropertyChanged("PropertyName");
                    // -----------------------------
                    CodeExpression[] propertyChangeParams = null;
                    if (GeneratorContext.GeneratorParams.TrackingChanges.Enabled &&
                        GeneratorContext.GeneratorParams.Language == GenerationLanguage.CSharp)
                    {
                        propertyChangeParams = new CodeExpression[]
                                                   {
                                                       new CodePrimitiveExpression(prop.Name),
                                                       new CodeArgumentReferenceExpression(("value"))
                                                   };
                    }
                    else
                    {
                        propertyChangeParams = new CodeExpression[] { new CodePrimitiveExpression(prop.Name) };
                    }

                    var propChange =
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnPropertyChanged"),
                            propertyChangeParams);

                    if (prop.HasSet)
                    {
                        var propAssignStatment = prop.SetStatements[0] as CodeAssignStatement;
                        if (propAssignStatment != null)
                        {
                            var cfreL = propAssignStatment.Left as CodeFieldReferenceExpression;
                            var cfreR = propAssignStatment.Right as CodePropertySetValueReferenceExpression;

                            if (cfreL != null)
                            {
                                var setValueCondition = new CodeStatementCollection { propAssignStatment, propChange };

                                // ---------------------------------------------
                                // (this.descriptionField == null)
                                // ---------------------------------------------
                                var exprFieldEqualsNull = new CodeBinaryOperatorExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeThisReferenceExpression(),
                                        cfreL.FieldName),
                                    CodeBinaryOperatorType.IdentityEquality,
                                    new CodePrimitiveExpression(null));

                                // ---------------------------------------------
                                // (xxxField.Equals(value) != true)
                                // ---------------------------------------------
                                var exprFieldNotEqualsValue = new CodeBinaryOperatorExpression(
                                    new CodeMethodInvokeExpression(
                                        new CodeFieldReferenceExpression(
                                            null,
                                            cfreL.FieldName),
                                        "Equals",
                                        cfreR),
                                    CodeBinaryOperatorType.IdentityInequality,
                                    new CodePrimitiveExpression(true));

                                CodeStatement[] setValueStatements = CodeDomHelper.CodeStmtColToArray(setValueCondition);

                                var property = member as CodeMemberProperty;
                                if (property != null)
                                {
                                    if (property.Type.BaseType != new CodeTypeReference(typeof(long)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(DateTime)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(float)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(double)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(int)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(bool)).BaseType &&
                                        property.Type.BaseType != new CodeTypeReference(typeof(decimal)).BaseType &&
                                        property.Type.BaseType !=
                                        new CodeTypeReference("System.Numerics.BigInteger").BaseType &&
                                        enumListField.IndexOf(property.Type.BaseType) == -1)
                                    {
                                        // ---------------------------------------------
                                        // if ((this.descriptionField == null) || (xxxField.Equals(value) != true)) { ... }
                                        // ---------------------------------------------
                                        var condStatmentCondNullOrNotEquals = new CodeConditionStatement(
                                            new CodeBinaryOperatorExpression(
                                                exprFieldEqualsNull,
                                                CodeBinaryOperatorType.BooleanOr, exprFieldNotEqualsValue
                                                ),
                                            setValueStatements);

                                        prop.SetStatements[0] = condStatmentCondNullOrNotEquals;
                                    }
                                    else
                                    {
                                        // ---------------------------------------------
                                        // if ((xxxField.Equals(value) != true)) { ... }
                                        // ---------------------------------------------
                                        var condStatmentFieldEquals = new CodeConditionStatement(
                                            exprFieldNotEqualsValue,
                                            setValueStatements);

                                        prop.SetStatements[0] = condStatmentFieldEquals;
                                    }
                                }
                            }
                            else
                                prop.SetStatements.Add(propChange);
                        }
                    }
                }
            }
        }

        private void CreateShouldSerializeMethod(CodeTypeDeclaration type, string propertyName)
        {
            type.Members.Add(GetShouldSerializeMethod(propertyName));
        }

        /// <summary>
        /// Determines whether [is complex type] [the specified code type reference].
        /// </summary>
        /// <param name="codeTypeReference">The code type reference.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="finded">if set to <c>true</c> [finded].</param>
        /// <returns>
        /// true if type is complex type (class, List, etc.)"/&gt;
        /// </returns>
        protected bool IsComplexType(CodeTypeReference codeTypeReference, CodeNamespace ns, out bool finded)
        {
            CodeTypeDeclaration declaration = FindTypeInNamespace(codeTypeReference.BaseType, ns, out finded);
            if (!finded)
            {
                return false;
            }
            return ((declaration != null) && declaration.IsClass) &&
                   ((declaration.TypeAttributes & TypeAttributes.Abstract) != TypeAttributes.Abstract);
        }

        /// <summary>
        /// Removes the default XML attributes.
        /// </summary>
        /// <param name="customAttributes">
        /// The custom Attributes.
        /// </param>
        protected virtual void RemoveDefaultXmlAttributes(CodeAttributeDeclarationCollection customAttributes)
        {
            var codeAttributes = new List<CodeAttributeDeclaration>();
            foreach (var attribute in customAttributes)
            {
                var attrib = attribute as CodeAttributeDeclaration;
                if (attrib == null)
                {
                    continue;
                }

                //Since we can get in here if the TargetFramework is Silverlight, we need to check again to see
                //if the GenerateXmlAttributes is actually True. Is so, Silverlight needs the other attributes to properly 
                //serialize XML so it can be compatible with generated XML from a regular C#, .Net (2.0+) application 
                //(that also use the Serialization.GenerateXmlAttributes support).
                //Silverlight doesn't support the two I left in, so we remove those. 
                //If we aren't wanting to GenerateXMLAttributes when TF=Silverlight, then we remove all the attrib. markers.
                //Fixes http://xsd2code.codeplex.com/workitem/14052
                if (GeneratorContext.GeneratorParams.Serialization.GenerateXmlAttributes &&
                    GeneratorContext.GeneratorParams.TargetFramework == TargetFramework.Silverlight)
                {
                    if (
                        attrib.Name == "System.SerializableAttribute" ||
                        attrib.Name == "System.ComponentModel.DesignerCategoryAttribute"
                        )
                    {
                        codeAttributes.Add(attrib);
                    }
                }
                else
                {
                    if (
                        attrib.Name == "System.Xml.Serialization.XmlAttributeAttribute" ||
                    attrib.Name == "System.Xml.Serialization.XmlTypeAttribute" ||
                    attrib.Name == "System.Xml.Serialization.XmlElementAttribute" ||
                    attrib.Name == "System.CodeDom.Compiler.GeneratedCodeAttribute" ||
                    attrib.Name == "System.SerializableAttribute" ||
                    attrib.Name == "System.ComponentModel.DesignerCategoryAttribute" ||
                        attrib.Name == "System.Xml.Serialization.XmlRootAttribute"
                        )
                    {
                        codeAttributes.Add(attrib);
                    }
                }

            }

            foreach (var item in codeAttributes)
            {
                customAttributes.Remove(item);
            }
        }

        protected virtual void RemoveNonSilverlightXmlAttributes(CodeAttributeDeclarationCollection customAttributes)
        {
            var codeAttributes = new List<CodeAttributeDeclaration>();
            foreach (object attribute in customAttributes)
            {
                var attrib = attribute as CodeAttributeDeclaration;
                if (attrib == null)
                {
                    continue;
                }

                if (attrib.Name == "System.SerializableAttribute" ||
                    attrib.Name == "System.ComponentModel.DesignerCategoryAttribute")
                {
                    codeAttributes.Add(attrib);
                }
            }

            foreach (CodeAttributeDeclaration item in codeAttributes)
            {
                customAttributes.Remove(item);
            }
        }

        /// <summary>
        /// Removes the debug attributes.
        /// </summary>
        /// <param name="customAttributes">The custom attributes Collection.</param>
        protected virtual void RemoveDebugAttributes(CodeAttributeDeclarationCollection customAttributes)
        {
            var codeAttributes = new List<CodeAttributeDeclaration>();
            foreach (object attribute in customAttributes)
            {
                var attrib = attribute as CodeAttributeDeclaration;
                if (attrib == null)
                {
                    continue;
                }

                if (attrib.Name == "System.Diagnostics.DebuggerStepThroughAttribute")
                {
                    codeAttributes.Add(attrib);
                }
            }
            //DCM: OK not sure why it in this loop other than its like a transaction.
            //Not going to touch it now.
            foreach (CodeAttributeDeclaration item in codeAttributes)
            {
                customAttributes.Remove(item);
            }
        }

        /// <summary>
        /// Get CodeTypeReference for collection
        /// </summary>
        /// <param name="codeType">The code Type.</param>
        /// <returns>return array of or genereric collection</returns>
        protected virtual CodeTypeReference GetCollectionType(CodeTypeReference codeType)
        {
            CodeTypeReference collectionType = codeType;
            if (codeType.BaseType == typeof(byte).FullName)
            {
                // Never change byte[] to List<byte> etc.
                // Fix : when translating hexBinary and base64Binary 
                return codeType;
            }

            switch (GeneratorContext.GeneratorParams.CollectionObjectType)
            {
                case CollectionType.List:
                    collectionType = new CodeTypeReference("List", new[] { new CodeTypeReference(codeType.BaseType) });
                    break;

                case CollectionType.IList:
                    collectionType = new CodeTypeReference("IList", new[] { new CodeTypeReference(codeType.BaseType) });
                    break;

                case CollectionType.BindingList:
                    collectionType = new CodeTypeReference("BindingList",
                                                           new[] { new CodeTypeReference(codeType.BaseType) });
                    break;

                case CollectionType.ObservableCollection:
                    collectionType = new CodeTypeReference("ObservableCollection",
                                                           new[] { new CodeTypeReference(codeType.BaseType) });
                    break;

                case CollectionType.DefinedType:
                    string typname = codeType.BaseType.Replace(".", string.Empty) + "Collection";

                    if (!CollectionTypes.Keys.Contains(typname))
                        CollectionTypes.Add(typname, codeType.BaseType);

                    collectionType = new CodeTypeReference(typname);
                    break;
                default:
                    {
                        // If not use generics, remove multiple array Ex. string[][] => string[]
                        // Fix : http://xsd2code.codeplex.com/WorkItem/View.aspx?WorkItemId=7269
                        if (codeType.ArrayElementType.ArrayRank > 0)
                            collectionType.ArrayElementType.ArrayRank = 0;
                    }

                    break;
            }

            return collectionType;
        }

        /// <summary>
        /// Search defaut constructor. If not exist, create a new ctor.
        /// </summary>
        /// <param name="type">CodeTypeDeclaration type</param>
        /// <param name="newCTor">Indicates if new constructor</param>
        /// <returns>return current or new CodeConstructor</returns>
        protected virtual CodeConstructor GetConstructor(CodeTypeDeclaration type, ref bool newCTor)
        {
            CodeConstructor ctor = null;
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeConstructor)
                {
                    ctor = member as CodeConstructor;
                    ctor.Name = type.Name;
                    ctor.Comments.Clear();
                    break;
                }
            }

            if (ctor == null)
            {
                newCTor = true;
                ctor = CreateClassConstructor(type);
            }

            if (GeneratorContext.GeneratorParams.Miscellaneous.EnableSummaryComment)
                CodeDomHelper.CreateSummaryComment(ctor.Comments, string.Format("{0} class constructor", ctor.Name));

            return ctor;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates the static serializer.
        /// </summary>
        /// <param name="classType">Type of the class.</param>
        private static void CreateStaticSerializer(CodeTypeDeclaration classType)
        {
            string typeName = GeneratorContext.GeneratorParams.GenericBaseClass.Enabled ? "T" : classType.Name;


            //VB is not Case Sensitive
            string fieldName = GeneratorContext.GeneratorParams.Language == GenerationLanguage.VisualBasic
                                   ? "sSerializer"
                                   : "serializer";

            // -----------------------------------------------------------------
            // private static System.Xml.Serialization.XmlSerializer serializer;
            // -----------------------------------------------------------------
            var serializerfield = new CodeMemberField(typeof(XmlSerializer), fieldName);
            serializerfield.Attributes = MemberAttributes.Static | MemberAttributes.Private;

            classType.Members.Add(serializerfield);

            var typeRef = new CodeTypeReference(typeName);
            var typeofValue = new CodeTypeOfExpression(typeRef);

            // private static System.Xml.Serialization.XmlSerializer Serializer { get {...} }
            var serializerProperty = new CodeMemberProperty
                                         {
                                             Type = new CodeTypeReference(typeof(XmlSerializer)),
                                             Name = "Serializer",
                                             HasSet = false,
                                             HasGet = true,
                                             Attributes = MemberAttributes.Static | MemberAttributes.Private
                                         };

            var statments = new CodeStatementCollection();

            statments.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression(fieldName),
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(XmlSerializer)), new CodeExpression[] { typeofValue })));


            serializerProperty.GetStatements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression(fieldName),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    statments.ToArray()));


            serializerProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeVariableReferenceExpression(fieldName)));

            classType.Members.Add(serializerProperty);
        }

        /// <summary>
        /// Generates the base class.
        /// </summary>
        /// <returns>Return base class codetype declaration</returns>
        private CodeTypeDeclaration GenerateBaseClass()
        {
            var baseClass = new CodeTypeDeclaration(GeneratorContext.GeneratorParams.GenericBaseClass.BaseClassName)
                                {
                                    IsClass = true,
                                    IsPartial = true,
                                    TypeAttributes = TypeAttributes.Public
                                };

            baseClass.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Base entity class"));
            baseClass.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Base entity class"));

            if (GeneratorContext.GeneratorParams.EnableDataBinding)
                baseClass.BaseTypes.Add(typeof(INotifyPropertyChanged));

            baseClass.TypeParameters.Add(new CodeTypeParameter("T"));

            if (GeneratorContext.GeneratorParams.EnableDataBinding)
                CreateDataBinding(baseClass);

            // Add plublic ObjectChangeTracker property
            if (GeneratorContext.GeneratorParams.TrackingChanges.Enabled)
                CreateChangeTrackerProperty(baseClass);

            if (GeneratorContext.GeneratorParams.Serialization.Enabled)
            {
                CreateStaticSerializer(baseClass);
                CreateSerializeMethods(baseClass);
            }

            if (GeneratorContext.GeneratorParams.GenerateCloneMethod)
                CreateCloneMethod(baseClass);

            return baseClass;
        }

        #endregion
    }
}