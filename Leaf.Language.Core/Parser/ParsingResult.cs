using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;

namespace Leaf.Language.Core.Parser;

public class ParsingResult
{
    public NamespaceSymbol NamespaceSymbol { get; set; }
    public List<FunctionDefinition> FunctionDefinitions { get; set; } = new();
    public List<ImportedFunctionDefinition> ImportedFunctionDefinitions { get; set; } = new();
    public List<ImportLibraryDefinition> ImportLibraryDefinitions { get; set; } = new();
    public List<GenericTypeDefinition> GenericTypeDefinitions { get; set; } = new();
    public List<GenericFunctionDefinition> GenericFunctionDefinitions { get; set; } = new();
    public List<TypeDefinition> TypeDefinitions { get; set; } = new();
    public ParsingResult(NamespaceSymbol namespaceSymbol)
    {
        NamespaceSymbol = namespaceSymbol;   
    }


    public void Concat(ParsingResult parsingResult)
    {
        FunctionDefinitions.AddRange(parsingResult.FunctionDefinitions);
        ImportedFunctionDefinitions.AddRange(parsingResult.ImportedFunctionDefinitions);
        ImportLibraryDefinitions.AddRange(parsingResult.ImportLibraryDefinitions);
        GenericTypeDefinitions.AddRange(parsingResult.GenericTypeDefinitions);
        GenericFunctionDefinitions.AddRange(parsingResult.GenericFunctionDefinitions);
        TypeDefinitions.AddRange(parsingResult.TypeDefinitions);
    }
}