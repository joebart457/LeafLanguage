
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Parser;
using Tokenizer.Core.Exceptions;

namespace Leaf.Language.Core.StaticAnalysis;

public class ProjectAnalyzer
{
    public List<TypeResolverResult> Analyze(string projectDirectory)
    {
        var parser = new ProgramParser();
        var parsingResults = new List<ParsingResult>();
        var parsingErrors = new List<ParsingException>();
        foreach(var file in Directory.EnumerateFiles(projectDirectory, "*.leaf", SearchOption.AllDirectories))
        {
            var parsingResult = parser.ParseFile(file, out var errors);
           
            if (parsingResult != null) parsingResults.Add(parsingResult);
            parsingErrors.AddRange(parsingErrors);
        }
        var flattenedResults = FlattenNamespaces(parsingResults);
        var namespaces = new Dictionary<NamespaceSymbol, TypeResolver>();
        foreach(var result in flattenedResults)
        {
            var typeResolver = new TypeResolver(result, namespaces);
            namespaces[result.NamespaceSymbol] = typeResolver;
            typeResolver.GatherSignatures();
        }
        var typeResolvedResults = namespaces.Values.Select(x => x.ResolveDefinitions()).ToList();
        return typeResolvedResults;
    }

    private List<ParsingResult> FlattenNamespaces(List<ParsingResult> parsingResults)
    {
        var flattenedResults = new List<ParsingResult>();
        foreach(var parsingResult in parsingResults)
        {
            var existingParsingResult = flattenedResults.Find(x => x.NamespaceSymbol.Equals(parsingResult.NamespaceSymbol));
            if (existingParsingResult == null) flattenedResults.Add(parsingResult);
            else
            {
                existingParsingResult.Concat(parsingResult);
            }
        }
        return flattenedResults;
    }
}