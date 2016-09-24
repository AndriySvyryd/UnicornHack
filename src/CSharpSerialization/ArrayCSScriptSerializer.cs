using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class ArrayCSScriptSerializer : CSScriptSerializer
    {
        public ArrayCSScriptSerializer(Type type)
            : base(type)
        {
        }

        public override ExpressionSyntax GetCreation(object obj)
            => SyntaxFactory.ArrayCreationExpression((ArrayTypeSyntax)GetTypeSyntax(Type))
                .WithInitializer(AddNewLine(
                    GetArrayInitializerExpression((Array)obj, startingDimension: 0, indices: ImmutableArray<int>.Empty)));

        private InitializerExpressionSyntax GetArrayInitializerExpression
            (Array array, int startingDimension, ImmutableArray<int> indices)
            => SyntaxFactory.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                    ToCommaSeparatedList(Enumerable.Range(
                        array.GetLowerBound(startingDimension),
                        array.GetUpperBound(startingDimension) - array.GetLowerBound(startingDimension) + 1)
                        .Select(i => array.Rank > startingDimension + 1
                            ? GetArrayInitializerExpression(array, startingDimension + 1, indices.Add(i))
                            : GetCreationExpression(array.GetValue(indices.Add(i).ToArray()))))));
    }
}