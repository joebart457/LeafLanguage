using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public class ProgramIconStatement : StatementBase
{
    public Token IconFilePath { get; set; }

    public ProgramIconStatement(Token iconFilePath) : base(iconFilePath)
    {
        IconFilePath = iconFilePath;
    }

    public override void GatherSignature(TypeResolver typeResolver)
    {
        typeResolver.GatherSignature(this);
    }

    public override TypedStatement Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }
}