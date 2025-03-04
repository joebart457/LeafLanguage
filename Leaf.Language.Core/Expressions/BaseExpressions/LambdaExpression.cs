using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class LambdaExpression : ExpressionBase
{
    public FunctionDefinition FunctionDefinition { get; private set; }
    public LambdaExpression(Token token, FunctionDefinition functionDefinition) : base(token)
    {
        FunctionDefinition = functionDefinition;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new LambdaExpression(Token, FunctionDefinition.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).CopyStartAndEndTokens(this);
    }
}