using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Schema;

namespace Xsd2Code.Library.Extensions
{
    /// <summary>
    /// Convertion des array properties en collection
    /// </summary>
    public class CustomExtension : ICodeExtension
    {
        public void Process(CodeNamespace code, XmlSchema schema)
        {
            CodeTypeDeclaration[] types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            foreach (CodeTypeDeclaration type in types)
            {
                if (type.IsClass || type.IsStruct)
                {
                    bool AddedToConstructor = false;
                    CodeConstructor ctor = ProcessClass(type);
                    foreach (CodeTypeMember member in type.Members)
                    {
                        #region Traitement des Property
                        if (member is CodeMemberProperty)
                        {
                            ProcessProperty(type, member);
                        }
                        #endregion

                        #region traitement des membres privés

                        if (member is CodeMemberField)
                        {
                            ProcessField(member, ctor, code, ref AddedToConstructor);
                        }
                        #endregion
                    }
                    if (AddedToConstructor)
                        type.Members.Add(ctor);

                    #region Ajout de public event PropertyChangedEventHandler PropertyChanged = null;
                    CodeMemberEvent cme = new CodeMemberEvent();
                    cme.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                    cme.Name = "PropertyChanged";
                    cme.Type = new CodeTypeReference(typeof(PropertyChangedEventHandler));
                    type.Members.Add(cme);
                    #endregion

                    #region Ajout de private void OnPropertyChanged(string info) {...}
                    CodeMemberMethod PropChanged = new CodeMemberMethod();
                    PropChanged.Name = "OnPropertyChanged";
                    PropChanged.Parameters.Add(new CodeParameterDeclarationExpression(typeof(String), "info"));
                    PropChanged.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression("PropertyChangedEventHandler handler = PropertyChanged")));
                    CodeExpressionStatement cs1 = new CodeExpressionStatement(new CodeSnippetExpression("handler(this, new PropertyChangedEventArgs(info))"));
                    CodeStatement[] statements = new CodeExpressionStatement[] { cs1 };
                    PropChanged.Statements.Add(new CodeConditionStatement(new CodeSnippetExpression("handler != null"), statements));
                    type.Members.Add(PropChanged);
                    #endregion
                }

            }
        }
        /// <summary>
        /// Ajout de l'implémentation de INotifyPropertyChanged et ajout d'un constructeur
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static CodeConstructor ProcessClass(CodeTypeDeclaration type)
        {
            type.BaseTypes.Add(typeof(INotifyPropertyChanged));

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

            #region Ajout Attribut EditorBrowsable
            // Ajouter ici l'ajout d'un attribut pour marquer la propriété
            // privé non visible dans l'IDE.
            // [EditorBrowsable(EditorBrowsableState.Never)]
            CodeTypeReference attributeType = new CodeTypeReference(typeof(EditorBrowsableAttribute).Name.Replace("Attribute", string.Empty));
            CodeAttributeArgument argument = new CodeAttributeArgument();
            argument.Value = new CodePropertyReferenceExpression(new CodeSnippetExpression(typeof(EditorBrowsableState).Name), "Never");
            field.CustomAttributes.Add(new CodeAttributeDeclaration(attributeType, new CodeAttributeArgument[] { argument }));
            #endregion

            #region Change les types Array par un type générique et allocation dans constructeur
            bool isCollectionType = field.Type.ArrayElementType != null;
            if (isCollectionType)
            {
                field.Type = new CodeTypeReference("List <" + field.Type.BaseType + ">");
            }
            #endregion

            #region Allocation des objets de type classe ou de type collection
            CodeTypeDeclaration declaration = FindTypeInNamespace(field.Type.BaseType, ns);
            if ((isCollectionType ||/*(!isCollectionType || */(((declaration != null) && declaration.IsClass) && ((declaration.TypeAttributes & TypeAttributes.Abstract) != TypeAttributes.Abstract))))
            {
                ctor.Statements.Insert(0, CreateLazyInitializationCodeStatements(field.Name, field.Type));
                AddedToConstructor = true;
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
        /// Traitement des property. Change les Array par des types génériques.
        /// Ajout dans le get l'appel à OnPropertyChanged.
        /// </summary>
        private static void ProcessProperty(CodeTypeDeclaration type, CodeTypeMember member)
        {
            CodeMemberProperty prop = (CodeMemberProperty)member;

            // Change Data[] par un type générique
            if (prop.Type.ArrayElementType != null)
            {
                prop.Type = new CodeTypeReference("List<" + prop.Type.BaseType + ">");
            }

            // Ajoute au getter l'appel à OnPropertyChanged
            if (type.BaseTypes.IndexOf(new CodeTypeReference(typeof(CollectionBase))) == -1)
                prop.SetStatements.Add(new CodeExpressionStatement(new CodeSnippetExpression("OnPropertyChanged(\"" + prop.Name + "\")")));
        }
    }
}