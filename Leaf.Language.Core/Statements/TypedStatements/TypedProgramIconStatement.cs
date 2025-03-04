using Assembler.Core;
using Leaf.Language.Core.Statements.BaseStatements;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.TypedStatements;

public class TypedProgramIconStatement : TypedStatement
{
    public Token IconFilePath { get; set; }
    public TypedProgramIconStatement(StatementBase originalStatement, Token iconFilePath) : base(originalStatement)
    {
        IconFilePath = iconFilePath;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        //Icon resources not directly supported in Assembler.Core

        //asm.SetProgramIcon(IconFilePath.Lexeme);
        
    }
}