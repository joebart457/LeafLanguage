using Leaf.Language.Api.Models;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Parser;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Exceptions;

namespace Leaf.Language.Api;
public class LanguageInformationEngine
{
    private Dictionary<NamespaceSymbol, ProgramContext> _workspaceContext = new();
    private Dictionary<string, NamespaceSymbol> _filePathNamespaces = new();

    public Dictionary<NamespaceSymbol, ProgramContext> WorkspaceContext => _workspaceContext;
    public Dictionary<string, NamespaceSymbol> FilePathNamespaces => _filePathNamespaces;
    public string ProjectDirectory { get; set; }

    public LanguageInformationEngine(string projectDirectory)
    {
        ProjectDirectory = projectDirectory;
    }

    public string? FullAnalyze(out List<ParsingException> parsingErrors)
    {
        _workspaceContext = new();
        _filePathNamespaces = new();
        var parser = new ProgramParser();
        var parsingResults = new List<ParsingResult>();
        parsingErrors = new List<ParsingException>();
        if (!Directory.Exists(ProjectDirectory)) return $"unable to open directory {ProjectDirectory}";
        foreach (var file in Directory.EnumerateFiles(ProjectDirectory, "*.leaf", SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(file);
            var parsingResult = parser.ParseFile(fullPath, out var errors);
            if (parsingResult != null)
            {
                _filePathNamespaces[fullPath] = parsingResult.NamespaceSymbol;
                parsingResults.Add(parsingResult);
            }

            parsingErrors.AddRange(errors);
        }
        var flattenedResults = FlattenNamespaces(parsingResults);
        var namespaces = new Dictionary<NamespaceSymbol, TypeResolver>();
        foreach (var result in flattenedResults)
        {
            var typeResolver = new LanguageInformationResolver(result, namespaces);
            namespaces[result.NamespaceSymbol] = typeResolver;
            typeResolver.GatherSignatures();
        }     
        foreach(var kv in namespaces)
            _workspaceContext[kv.Key] = ((LanguageInformationResolver)kv.Value).ResolveWithTryCatch();
        return null;
    }

    public bool TryGetContextForFile(string filePath, out ProgramContext? context)
    {
        context = null;
        filePath = Path.GetFullPath(filePath);
        if (!_filePathNamespaces.TryGetValue(filePath, out var @namespace)) return false;
        if (!_workspaceContext.TryGetValue(@namespace, out context)) return false;
        return true;
    }

    public ProgramContext? PartialAnalyze(string filePath, out List<ParsingException> parsingErrors)
    {
        _workspaceContext = new();
        _filePathNamespaces = new();
        filePath = Path.GetFullPath(filePath);
        var parser = new ProgramParser();
        var parsingResults = new List<ParsingResult>();
        parsingErrors = new List<ParsingException>();
        NamespaceSymbol? namespaceOfFilePath = null;
        foreach (var file in Directory.EnumerateFiles(ProjectDirectory, "*.leaf", SearchOption.AllDirectories))
        {
            var parsingResult = parser.ParseFile(file, out var errors);
            if (parsingResult != null) 
            {
                if (Path.GetFullPath(file) == filePath) namespaceOfFilePath = parsingResult.NamespaceSymbol;
                parsingResults.Add(parsingResult);
            }
            
            parsingErrors.AddRange(errors);
        }
        if (namespaceOfFilePath == null) return null;
        var flattenedResults = FlattenNamespaces(parsingResults);
        var namespaces = new Dictionary<NamespaceSymbol, TypeResolver>();

        foreach (var result in flattenedResults)
        {
            var typeResolver = new LanguageInformationResolver(result, namespaces);
            namespaces[result.NamespaceSymbol] = typeResolver;
            typeResolver.GatherSignatures();
        }
        if (namespaces.TryGetValue(namespaceOfFilePath, out var resolver) && resolver is LanguageInformationResolver languageResolver)
            return languageResolver.ResolveWithTryCatch();
        return null;
    }


    private List<ParsingResult> FlattenNamespaces(List<ParsingResult> parsingResults)
    {
        var flattenedResults = new List<ParsingResult>();
        foreach (var parsingResult in parsingResults)
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