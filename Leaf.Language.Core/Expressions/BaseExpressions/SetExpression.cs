﻿using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;


public class SetExpression : ExpressionBase
{
    public ExpressionBase AssignmentTarget { get; set; }
    public ExpressionBase ValueToAssign { get; set; }
    public SetExpression(Token token, ExpressionBase assignmentTarget, ExpressionBase valueToAssign) : base(token)
    {
        AssignmentTarget = assignmentTarget;
        ValueToAssign = valueToAssign;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new SetExpression(Token, AssignmentTarget.ReplaceGenericTypeSymbols(genericToConcreteTypeMap), ValueToAssign.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (AssignmentTarget.TryGetContainingExpression(line, column, out containingExpression)) return true;
        if (ValueToAssign.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}