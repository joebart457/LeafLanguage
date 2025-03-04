using Leaf.Language.TypedExpressions;
using Tokenizer.Core.Models;

namespace Leaf.Language.Api.Models;

public class ExpressionContext
{
    public TypedExpression Expression { get; set; }
    public FunctionContext FunctionContext { get; set; }
    public ExpressionContext(TypedExpression expression, FunctionContext functionContext)
    {
        Expression = expression;
        FunctionContext = functionContext;
    }
    public Location Start => Expression.OriginalExpression.StartToken.Start;
    public Location End => Expression.OriginalExpression.EndToken.End;

    public bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (Expression.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return false;
    }

}