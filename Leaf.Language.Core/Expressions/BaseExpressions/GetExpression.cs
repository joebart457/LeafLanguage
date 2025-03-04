using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class GetExpression : ExpressionBase
{
    public ExpressionBase Instance { get; private set; }
    public Token TargetField { get; private set; }
    public bool ShortCircuitOnNull { get; private set; }
    public GetExpression(Token token, ExpressionBase instance, Token targetField, bool shortCircuitOnNull) : base(token)
    {
        Instance = instance;
        TargetField = targetField;
        ShortCircuitOnNull = shortCircuitOnNull;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new GetExpression(Token, Instance.ReplaceGenericTypeSymbols(genericToConcreteTypeMap), TargetField, ShortCircuitOnNull).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (Instance.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}