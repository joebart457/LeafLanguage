﻿using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class CastExpression : ExpressionBase
{
    public TypeSymbol TypeSymbol { get; private set; }
    public ExpressionBase Expression { get; private set; }
    public CastExpression(Token token, TypeSymbol typeSymbol, ExpressionBase expression) : base(token)
    {
        TypeSymbol = typeSymbol;
        Expression = expression;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new CastExpression(Token, TypeSymbol.ReplaceGenericTypeParameter(genericToConcreteTypeMap), Expression.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (Expression.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}