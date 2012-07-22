// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Net30Extension.cs" company="Xsd2Code">
//   N/A
// </copyright>
// <summary>
//   Implements code generation extension for .Net Framework 3.0
// </summary>
// <remarks>
//  Updated 2010-01-20 Deerwood McCord Jr. Cleaned CodeSnippetStatements by replacing with specific CodeDom Expressions
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Xsd2Code.Library.Helpers;

namespace Xsd2Code.Library.Extensions
{
    /// <summary>
    /// Implements code generation extension for .Net Framework 3.0
    /// </summary>
    [CodeExtension(TargetFramework.Net30)]
    public class Net30Extension : CodeExtension
    {
        #region Protected methods

        /// <summary>
        /// Processes the class.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="schema">The input xsd schema.</param>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        protected override void ProcessClass(CodeNamespace codeNamespace, XmlSchema schema, CodeTypeDeclaration type)
        {
            RemoveFieldsForAutomaticProperties(type);

            base.ProcessClass(codeNamespace, schema, type);

            // generate automatic properties
            GenerateAutomaticProperties(type);
        }

        /// <summary>
        /// Create data contract attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        /// <param name="schema">XML schema</param>
        protected override void CreateDataContractAttribute(CodeTypeDeclaration type, XmlSchema schema)
        {
            base.CreateDataContractAttribute(type, schema);

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                var attributeType = new CodeTypeReference("System.Runtime.Serialization.DataContractAttribute");
                var codeAttributeArgument = new List<CodeAttributeArgument>
                                                {
                                                    new CodeAttributeArgument("Name",
                                                                              new CodePrimitiveExpression(type.Name))
                                                };

                if (!string.IsNullOrEmpty(schema.TargetNamespace))
                {
                    codeAttributeArgument.Add(new CodeAttributeArgument("Namespace",
                                                                        new CodePrimitiveExpression(
                                                                            schema.TargetNamespace)));
                }

                type.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, codeAttributeArgument.ToArray()));
            }
        }

        /// <summary>
        /// Creates the data member attribute.
        /// </summary>
        /// <param name="prop">Represents a declaration for a property of a type.</param>
        protected override void CreateDataMemberAttribute(CodeMemberProperty prop)
        {
            base.CreateDataMemberAttribute(prop);

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                var attrib = new CodeTypeReference("System.Runtime.Serialization.DataMemberAttribute");
                prop.CustomAttributes.Add(new CodeAttributeDeclaration(attrib));
            }
        }

        /// <summary>
        /// Import namespaces
        /// </summary>
        /// <param name="code">Code namespace</param>
        protected override void ImportNamespaces(CodeNamespace code)
        {
            base.ImportNamespaces(code);

            if (GeneratorContext.GeneratorParams.GenerateDataContracts)
            {
                code.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization"));
            }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Outputs the attribute argument.
        /// </summary>
        /// <param name="arg">Represents an argument used in a metadata attribute declaration.</param>
        /// <returns>transform attribute into srting</returns>
        private static string AttributeArgumentToString(CodeAttributeArgument arg)
        {
            var strWriter = new StringWriter();
            CodeDomProvider provider =
                Helpers.CodeDomProviderFactory.GetProvider(GeneratorContext.GeneratorParams.Language);

            if (!string.IsNullOrEmpty(arg.Name))
            {
                strWriter.Write(arg.Name);
                strWriter.Write("=");
            }

            provider.GenerateCodeFromExpression(arg.Value, strWriter, new CodeGeneratorOptions());
            return strWriter.ToString();
        }

        /// <summary>
        /// Outputs the attribute argument.
        /// </summary>
        /// <param name="arg">Represents an argument used in a metadata attribute declaration.</param>
        /// <returns>transform attribute into srting</returns>
        private static string ExpressionToString(CodeExpression arg)
        {
            var strWriter = new StringWriter();
            CodeDomProvider provider =
                Helpers.CodeDomProviderFactory.GetProvider(GeneratorContext.GeneratorParams.Language);
            provider.GenerateCodeFromExpression(arg, strWriter, new CodeGeneratorOptions());
            return strWriter.ToString();
        }

        private static void RemoveFieldsForAutomaticProperties(CodeTypeDeclaration type)
        {
            // Generate automatic properties.
            if (GeneratorContext.GeneratorParams.Language == GenerationLanguage.CSharp)
            {
                if (GeneratorContext.GeneratorParams.PropertyParams.AutomaticProperties
                    && !GeneratorContext.GeneratorParams.EnableDataBinding)
                {
                    var ctor = type.Members.OfType<CodeConstructor>().FirstOrDefault();
                    foreach (CodeMemberProperty member in type.Members.OfType<CodeMemberProperty>().ToList())
                    {
                        if (CodeDomHelper.IsPropertyNeedInstance(member))
                            continue;

                        var propReturnStatement = member.GetStatements[0] as CodeMethodReturnStatement;
                        if ((member.GetStatements.Count == 1) && (propReturnStatement != null) && member.HasSet)
                        {
                            // XmlSerializer doesn't handle automatic property with private set. Properties without setter aren't converted.
                            var field = propReturnStatement.Expression as CodeFieldReferenceExpression;
                            if (field != null)
                            {
                                CodeMemberField fieldToRemove = type.Members.OfType<CodeMemberField>().FirstOrDefault(currentField => currentField.Name.Equals(field.FieldName));
                                if (fieldToRemove != null)
                                {
                                    type.Members.Remove(fieldToRemove);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the automatic properties.
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration.</param>
        private static void GenerateAutomaticProperties(CodeTypeDeclaration type)
        {
            // Generate automatic properties.
            if (GeneratorContext.GeneratorParams.Language == GenerationLanguage.CSharp)
            {
                if (GeneratorContext.GeneratorParams.PropertyParams.AutomaticProperties
                    && !GeneratorContext.GeneratorParams.EnableDataBinding)
                {
                    var ctor = type.Members.OfType<CodeConstructor>().FirstOrDefault();
                    foreach (CodeMemberProperty member in type.Members.OfType<CodeMemberProperty>().ToList())
                    {
                        var propReturnStatement = member.GetStatements[0] as CodeMethodReturnStatement;
                        if ((member.GetStatements.Count == 1) && (propReturnStatement != null) && member.HasSet)
                        {
                            // XmlSerializer doesn't handle automatic property with private set. Properties without setter aren't converted.
                            var field = propReturnStatement.Expression as CodeFieldReferenceExpression;
                            if (field != null)
                            {
                                // property isn't lazy loaded, is convertible to automatic
                                var propertyText = new StringBuilder();
                                foreach (
                                    CodeAttributeDeclaration attribute in
                                        member.CustomAttributes.OfType<CodeAttributeDeclaration>())
                                {
                                    propertyText.AppendFormat("        [{0}(", attribute.Name);
                                    bool first = true;
                                    foreach (
                                        CodeAttributeArgument argument in
                                            attribute.Arguments.OfType<CodeAttributeArgument>())
                                    {
                                        if (!first)
                                        {
                                            propertyText.Append(", ");
                                        }

                                        first = false;
                                        propertyText.Append(AttributeArgumentToString(argument));
                                    }

                                    propertyText.AppendLine(")]");
                                }

                                propertyText.AppendFormat(
                                    "        public {0}{1} {2} {{ get; {3}set; }}",
                                    GeneratorContext.GeneratorParams.PropertyParams.EnableVirtualProperties
                                        ? "virtual "
                                        : string.Empty,
                                    ExpressionToString(new CodeTypeReferenceExpression(member.Type)),
                                    member.Name,
                                    member.HasSet ? string.Empty : "private ");

                                var codeSnippet = new CodeSnippetTypeMember { Text = propertyText.ToString() };
                                codeSnippet.Comments.AddRange(member.Comments);
                                type.Members.Insert(type.Members.IndexOf(member), codeSnippet);
                                type.Members.Remove(member);

                                // Check if private field need initialisation in ctor (defaut value).
                                if (ctor != null)
                                {
                                    CodeAssignStatement ctorInitAssignment =
                                        ctor.Statements.OfType<CodeAssignStatement>().FirstOrDefault(
                                            stat =>
                                            stat.Left is CodeFieldReferenceExpression
                                            &&
                                            ((CodeFieldReferenceExpression)stat.Left).FieldName.Equals(field.FieldName));

                                    if (ctorInitAssignment != null)
                                    {
                                        ((CodeFieldReferenceExpression)ctorInitAssignment.Left).FieldName = member.Name;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}