﻿using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;


public class LiteralExpression : ExpressionBase
{
    public object? Value { get; private set; }
    public LiteralExpression(Token token, object? value) : base(token)
    {
        Value = value;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new LiteralExpression(Token, Value).CopyStartAndEndTokens(this);
    }
}