using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;

namespace Leaf.Language.Core.StaticAnalysis;

public class TypeResolverResult
{
    public NamespaceSymbol NamespaceSymbol { get; set; }

    public TypeResolverResult(NamespaceSymbol namespaceSymbol)
    {
        NamespaceSymbol = namespaceSymbol;
    }

    public List<TypedImportLibraryDefinition> ImportLibraries { get; set; } = new();
    public List<TypedImportedFunctionDefinition> ImportedFunctions { get; set; } = new();
    public List<TypedFunctionDefinition> Functions { get; set; } = new();
}