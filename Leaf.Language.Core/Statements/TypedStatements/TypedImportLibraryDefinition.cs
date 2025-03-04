using Assembler.Core;
using Leaf.Language.Core.Statements.BaseStatements;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.TypedStatements;


public class TypedImportLibraryDefinition : TypedStatement
{
    public Token LibraryAlias { get; set; }
    public Token LibraryPath { get; set; }
    public TypedImportLibraryDefinition(StatementBase originalStatement, Token libraryAlias, Token libraryPath) : base(originalStatement)
    {
        LibraryAlias = libraryAlias;
        LibraryPath = libraryPath;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        asm.AddImportLibrary(new X86AssemblyContext.ImportLibrary(LibraryPath.Lexeme, LibraryAlias.Lexeme));
    }
}