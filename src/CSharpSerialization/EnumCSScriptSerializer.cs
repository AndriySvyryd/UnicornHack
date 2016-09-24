using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class EnumCSScriptSerializer : CSScriptSerializer
    {
        public EnumCSScriptSerializer(Type type)
            : base(type)
        {
        }

        public override ExpressionSyntax GetCreation(object obj)
        {
            var name = Enum.GetName(Type, obj);
            return name == null
                ? GetCompositeValue((Enum)obj)
                : GetSimpleValue(name);
        }

        protected virtual ExpressionSyntax GetCompositeValue(Enum obj)
        {
            var simpleValues = new List<ExpressionSyntax>();
            var defaultValue = Enum.ToObject(Type, value: 0);
            foreach (Enum currValue in Enum.GetValues(Type))
            {
                if (currValue.Equals(defaultValue))
                {
                    continue;
                }

                if (obj.HasFlag(currValue))
                {
                    simpleValues.Add(GetSimpleValue(Enum.GetName(Type, currValue)));
                }
            }

            return simpleValues.Aggregate((previous, current) =>
                SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, previous, current));
        }

        protected virtual ExpressionSyntax GetSimpleValue(string name)
            => SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(Type.Name),
                SyntaxFactory.IdentifierName(name));
    }
}