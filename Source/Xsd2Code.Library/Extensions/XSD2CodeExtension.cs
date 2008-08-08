namespace Xsd2Code.Library.Extensions
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Xml.Schema;
    using Xsd2Code;

    /// <summary>
    /// Convertion des array properties en collection
    /// </summary>
    public class CustomExtension : ICodeExtension
    {
        /// <summary>
        /// Process method for cs or vb CodeDom generation
        /// </summary>
        /// <param name="code">CodeNamespace generated</param>
        /// <param name="schema">XmlSchema to generate</param>
        public void Process(CodeNamespace code, XmlSchema schema)
        {
            CodeTypeDeclaration[] types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            foreach (CodeTypeDeclaration type in types)
            {
                if (type.IsClass || type.IsStruct)
                {
                    bool addedToConstructor = false;
                    bool isNewctor = false;
                    CodeConstructor ctor = GetConstructor(type, ref isNewctor);

                    foreach (CodeTypeMember member in type.Members)
                    {
                        #region traitement des membres privés
                        if (member is CodeMemberField)
                        {
                            ProcessField(member, ctor, code, ref addedToConstructor);
                        }
                        #endregion
                    }

                    foreach (CodeTypeMember member in type.Members)
                    {
                        #region Traitement des Property
                        if (member is CodeMemberProperty)
                        {
                            ProcessProperty(type, member);
                        }
                        #endregion
                    }

                    // Add new ctor if required
                    if (addedToConstructor && isNewctor)
                    {
                        type.Members.Add(ctor);
                    }

                    if (GenrationContext.EnableDataBinding)
                    {
                        #region Ajout de public event PropertyChangedEventHandler PropertyChanged = null;
                        CodeMemberEvent cme = new CodeMemberEvent();
                        cme.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                        cme.Name = "PropertyChanged";
                        cme.Type = new CodeTypeReference(typeof(PropertyChangedEventHandler));
                        cme.ImplementationTypes.Add(typeof(INotifyPropertyChanged));
                        type.Members.Add(cme);

                        // Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
                        #endregion

                        #region Ajout de private void OnPropertyChanged(string info) {...}
                        CodeMemberMethod PropChanged = new CodeMemberMethod();
                        PropChanged.Name = "OnPropertyChanged";
                        PropChanged.Parameters.Add(new CodeParameterDeclarationExpression(typeof(String), "info"));

                        if (GenrationContext.Language == GenerateCodeType.CSharp)
                        {
                            PropChanged.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("PropertyChangedEventHandler handler = PropertyChanged")));
                            CodeExpressionStatement cs1 = new CodeExpressionStatement(new CodeSnippetExpression("handler(this, new PropertyChangedEventArgs(info))"));
                            CodeStatement[] statements = new CodeExpressionStatement[] { cs1 };
                            PropChanged.Statements.Add(new CodeConditionStatement(new CodeSnippetExpression("handler != null"), statements));
                        }
                        else
                        {
                            PropChanged.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))")));
                        }

                        type.Members.Add(PropChanged);
                        #endregion
                    }
                }
            }
        }
        /// <summary>
        /// Search defaut constructor. If not exist, create a new ctor.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private CodeConstructor GetConstructor(CodeTypeDeclaration type, ref bool IsNewctor)
        {
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
                IsNewctor = true;
                ctor = ProcessClass(type);
            }
            ctor.Comments.Add(new CodeCommentStatement("Constructor", false));
            return ctor;
        }
        /// <summary>
        /// Ajout de l'implémentation de INotifyPropertyChanged et ajout d'un constructeur
        /// </summary>
        /// <param name="type">type of declaration</param>
        /// <returns>CodeConstructor</returns>
        private static CodeConstructor ProcessClass(CodeTypeDeclaration type)
        {
            if (GenrationContext.EnableDataBinding)
            {
                type.BaseTypes.Add(typeof(INotifyPropertyChanged));
            }

            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            return ctor;
        }
        /// <summary>
        /// Traitement des Fields. Ajout attribut non visible dans l'IDE, Change Array par type générique
        /// </summary>
        private static void ProcessField(CodeTypeMember member, CodeMemberMethod ctor, CodeNamespace ns, ref bool AddedToConstructor)
        {
            CodeMemberField field = (CodeMemberField)member;

            #region Add EditorBrowsable.Never for private Attribute
            // [EditorBrowsable(EditorBrowsableState.Never)]
            if (member.Attributes == MemberAttributes.Private)
            {
                if (GenrationContext.HidePrivateFieldInIde)
                {
                    CodeTypeReference attributeType = new CodeTypeReference(typeof(EditorBrowsableAttribute).Name.Replace("Attribute", string.Empty));
                    CodeAttributeArgument argument = new CodeAttributeArgument();
                    argument.Value = new CodePropertyReferenceExpression(new CodeSnippetExpression(typeof(EditorBrowsableState).Name), "Never");
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, new CodeAttributeArgument[] { argument }));
                }
            }
            #endregion

            #region Change les types Array par un type générique et allocation dans constructeur
            bool isCollectionType = field.Type.ArrayElementType != null;
            if (isCollectionType)
            {
                switch (GenrationContext.CollectionObjectType)
                {
                    case CollectionType.List:
                        if (GenrationContext.Language == GenerateCodeType.CSharp)
                        {
                            field.Type = new CodeTypeReference("List <" + field.Type.BaseType + ">");
                        }
                        else
                        {
                            field.Type = new CodeTypeReference("List (Of " + field.Type.BaseType + ")");
                        }
                        break;
                    case CollectionType.ObservableCollection:
                        if (GenrationContext.Language == GenerateCodeType.CSharp)
                        {
                            field.Type = new CodeTypeReference("ObservableCollection <" + field.Type.BaseType + ">");
                        }
                        else
                        {
                            field.Type = new CodeTypeReference("ObservableCollection (Of " + field.Type.BaseType + ")");
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion

            #region Allocation des objets de type classe ou de type collection
            if (GenrationContext.CollectionObjectType != CollectionType.Array)
            {
                CodeTypeDeclaration declaration = FindTypeInNamespace(field.Type.BaseType, ns);
                if ((isCollectionType ||/*(!isCollectionType || */(((declaration != null) && declaration.IsClass) && ((declaration.TypeAttributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))))
                {
                    ctor.Statements.Insert(0, CreateLazyInitializationCodeStatements(field.Name, field.Type));
                    AddedToConstructor = true;
                }
            }
            #endregion
        }

        /// <summary>
        /// Génération de code pour l'allocation d'un objet
        /// </summary>
        /// <returns></returns>
        private static CodeConditionStatement CreateLazyInitializationCodeStatements(string Name, CodeTypeReference Type)
        {
            CodeAssignStatement statement;
            statement = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), Name), new CodeObjectCreateExpression(Type, new CodeExpression[0]));
            return new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), Name), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)), new CodeStatement[] { statement });
        }
        /// <summary>
        /// Recherche le CodeTypeDeclaration d'un objet en fonction de son type de base (nom de classe)
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
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
        private static void ProcessProperty(CodeTypeDeclaration type, CodeTypeMember member)
        {
            CodeMemberProperty prop = (CodeMemberProperty)member;
            if (prop.Type.ArrayElementType != null)
            {
                switch (GenrationContext.CollectionObjectType)
                {
                    case CollectionType.List:
                        if (GenrationContext.Language == GenerateCodeType.CSharp)
                        {
                            prop.Type = new CodeTypeReference("List <" + prop.Type.BaseType + ">");
                        }
                        else
                        {
                            prop.Type = new CodeTypeReference("List (Of " + prop.Type.BaseType + ")");
                        }
                        break;
                    case CollectionType.ObservableCollection:
                        if (GenrationContext.Language == GenerateCodeType.CSharp)
                        {
                            prop.Type = new CodeTypeReference("ObservableCollection <" + prop.Type.BaseType + ">");
                        }
                        else
                        {
                            prop.Type = new CodeTypeReference("ObservableCollection (Of " + prop.Type.BaseType + ")");
                        }
                        break;
                    default:
                        break;
                }
            }

            // Add OnPropertyChanged in setter 
            if (GenrationContext.EnableDataBinding)
            {
                if (type.BaseTypes.IndexOf(new CodeTypeReference(typeof(CollectionBase))) == -1)
                    prop.SetStatements.Add(new CodeExpressionStatement(new CodeSnippetExpression("OnPropertyChanged(\"" + prop.Name + "\")")));
            }
        }
    }
}