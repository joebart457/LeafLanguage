using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Expressions.BaseExpressions;


public class GenericFunctionReferenceExpression : ExpressionBase
{
    public Token Identifier { get; set; }
    public List<TypeSymbol> TypeArguments { get; set; }
    public GenericFunctionReferenceExpression(Token identifier, List<TypeSymbol> typeArguments) : base(identifier)
    {
        Identifier = identifier;
        TypeArguments = typeArguments;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new GenericFunctionReferenceExpression(Identifier, TypeArguments.Select(x => x.ReplaceGenericTypeParameter(genericToConcreteTypeMap)).ToList()).CopyStartAndEndTokens(this);
    }

}