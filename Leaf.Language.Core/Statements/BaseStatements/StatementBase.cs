using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public abstract class StatementBase
{
    public Token Token { get; set; }
    private Token? _startToken = null;
    private Token? _endToken = null;
    public Token StartToken { get => _startToken ?? throw new NullReferenceException(nameof(StartToken)); set => _startToken = value; }
    public Token EndToken { get => _endToken ?? throw new NullReferenceException(nameof(EndToken)); set => _endToken = value; }
    public StatementBase(Token token)
    {
        Token = token;
    }

    public abstract void GatherSignature(TypeResolver typeResolver);
    public abstract TypedStatement Resolve(TypeResolver typeResolver);
}