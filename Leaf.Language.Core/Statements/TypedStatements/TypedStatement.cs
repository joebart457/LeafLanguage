using Assembler.Core;
using Leaf.Language.Core.Statements.BaseStatements;

namespace Leaf.Language.Core.Statements.TypedStatements;

public abstract class TypedStatement
{
    public StatementBase OriginalStatement { get; set; }

    protected TypedStatement(StatementBase originalStatement)
    {
        OriginalStatement = originalStatement;
    }

    public abstract void Compile(X86AssemblyContext asm);
}