using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class CallExpression : ExpressionBase
{
    public ExpressionBase CallTarget { get; private set; }
    public List<ExpressionBase> Arguments { get; private set; }
    public CallExpression(Token token, ExpressionBase callTarget, List<ExpressionBase> arguments) : base(token)
    {
        CallTarget = callTarget;
        Arguments = arguments;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new CallExpression(Token, CallTarget.ReplaceGenericTypeSymbols(genericToConcreteTypeMap), Arguments.Select(x => x.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).ToList()).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (CallTarget.TryGetContainingExpression(line, column, out containingExpression)) return true;
        foreach (var argument in Arguments)
        {
            if (argument.TryGetContainingExpression(line, column, out containingExpression)) return true;
        }
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}