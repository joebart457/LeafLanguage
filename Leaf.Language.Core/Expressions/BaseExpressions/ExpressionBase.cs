
using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;

public abstract class ExpressionBase
{
    public Token Token { get; private set; }

    private Token? _startToken = null;
    private Token? _endToken = null;
    public Token StartToken { get => _startToken ?? throw new NullReferenceException($"{nameof(StartToken)}, Expression type: {GetType().Name}"); set => _startToken = value; }
    public Token EndToken { get => _endToken ?? throw new NullReferenceException($"{nameof(EndToken)}, Expression type: {GetType().Name}"); set => _endToken = value; }

    protected ExpressionBase(Token token)
    {
        Token = token;
    }

    public abstract TypedExpression Resolve(TypeResolver typeResolver);
    public abstract ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap);
    public ExpressionBase CopyStartAndEndTokens(ExpressionBase copyFrom)
    {
        _startToken = copyFrom.StartToken;
        _endToken = copyFrom.EndToken;
        return this;
    }

    public bool Contains(int line, int column)
    {
        if (line == StartToken.Start.Line && line == EndToken.End.Line) return StartToken.Start.Column <= column && EndToken.End.Column >= column;
        if (line == StartToken.Start.Line) return StartToken.Start.Column <= column;
        if (line == EndToken.End.Line) return EndToken.End.Column >= column;
        return StartToken.Start.Line <= line && EndToken.End.Line >= line;
    }

    public virtual bool TryGetContainingExpression(int line, int column, out ExpressionBase? containingExpression)
    {
        if (Contains(line, column))
        {
            containingExpression = this;
            return true;
        }
        containingExpression = null;
        return false;
    }

}