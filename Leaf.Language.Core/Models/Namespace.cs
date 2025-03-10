using Leaf.Language.Core.Statements.BaseStatements;
using Leaf.Language.Core.Statements.TypedStatements;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class Namespace
{    
    public Token Name { get; set; }
    public Dictionary<string, TypedFunctionDefinition> Functions { get; set; } = new();
    public Dictionary<string, TypedImportedFunctionDefinition> ImportedFunctions { get; set; } = new();
    public Dictionary<string, TypedImportLibraryDefinition> ImportLibraries { get; set; } = new();
    public Dictionary<TypeSymbol, StructTypeInfo> UserTypes { get; set; } = new();
    public Dictionary<string, GenericFunctionDefinition> GenericFunctions { get; set; } = new();
    public Dictionary<string, GenericTypeDefinition> GenericTypes { get; set; } = new();

    public Namespace? EnclosingNamespace { get; set; } = null;
    public Dictionary<Token, Namespace> ChildNamespaces { get; set; } = new();
    public Namespace(Namespace? enclosingNamespace, Token name)
    {
        EnclosingNamespace = enclosingNamespace;
        Name = name;
    }
}