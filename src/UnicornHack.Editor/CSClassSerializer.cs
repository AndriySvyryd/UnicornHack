using System;
using System.Linq;
using CSharpScriptSerialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using UnicornHack.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnicornHack.Editor
{
    public class CSClassSerializer : CSScriptSerializer
    {
        public CSClassSerializer(Type type) : base(type)
        {
        }

        public override ExpressionSyntax GetCreation(object obj) => throw new NotImplementedException();

        public static string Serialize(object obj, string targetPropertyName, string targetNamespace,
            string targetClassName)
        {
            var expression = CompilationUnit()
                .WithUsings(List(CSScriptLoaderBase.Namespaces.Select(n => UsingDirective(ParseName(n)))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(ParseName(targetNamespace))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(ClassDeclaration(targetClassName)
                                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                        Token(SyntaxKind.StaticKeyword),
                                        Token(SyntaxKind.PartialKeyword)))
                                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                        FieldDeclaration(
                                                VariableDeclaration(GetTypeSyntax(obj.GetType()))
                                                    .WithVariables(SingletonSeparatedList(
                                                        VariableDeclarator(Identifier(
                                                                CSScriptLoaderBase.GenerateIdentifier(
                                                                    targetPropertyName)))
                                                            .WithInitializer(
                                                                EqualsValueClause(GetCreationExpression(obj))))))
                                            .WithModifiers(
                                                TokenList(Token(SyntaxKind.PublicKeyword),
                                                    Token(SyntaxKind.StaticKeyword),
                                                    Token(SyntaxKind.ReadOnlyKeyword)))))))));

            using (var workspace = new AdhocWorkspace())
            {
                return Formatter.Format(
                        expression,
                        workspace,
                        workspace.Options)
                    .ToFullString();
            }
        }
    }
}