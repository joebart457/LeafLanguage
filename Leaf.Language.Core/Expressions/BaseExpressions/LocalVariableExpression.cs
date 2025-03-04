using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class LocalVariableExpression : ExpressionBase
{
    public TypeSymbol TypeSymbol { get; set; }
    public Token Identifier { get; set; }
    public ExpressionBase? Initializer { get; set; }
    public LocalVariableExpression(Token token, TypeSymbol typeSymbol, Token identifier, ExpressionBase? initializer) : base(token)
    {
        TypeSymbol = typeSymbol;
        Identifier = identifier;
        Initializer = initializer;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new LocalVariableExpression(Token, TypeSymbol.ReplaceGenericTypeParameter(genericToConcreteTypeMap), Identifier, Initializer?.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).CopyStartAndEndTokens(this);
    }

    public override bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (Initializer?.TryGetContainingExpression(line, column, out containingExpression) == true) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}