using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class ReturnExpression : ExpressionBase
{
    public ExpressionBase? ReturnValue { get; set; }
    public ReturnExpression(Token token, ExpressionBase? returnValue) : base(token)
    {
        ReturnValue = returnValue;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new ReturnExpression(Token, ReturnValue?.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (ReturnValue?.TryGetContainingExpression(line, column, out containingExpression) == true) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}