using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Statements.BaseStatements;

public class ImportLibraryDefinition : StatementBase
{
    public Token LibraryAlias { get; set; }
    public Token LibraryPath { get; set; }
    public ImportLibraryDefinition(Token libraryAlias, Token libraryPath) : base(libraryAlias)
    {
        LibraryAlias = libraryAlias;
        LibraryPath = libraryPath;
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