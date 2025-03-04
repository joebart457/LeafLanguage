using Leaf.Language.Core.Statements.TypedStatements;

namespace Leaf.Language.Core.StaticAnalysis;

public class TypeResolverResult
{
    public List<TypedImportLibraryDefinition> ImportLibraries { get; set; } = new();
    public List<TypedImportedFunctionDefinition> ImportedFunctions { get; set; } = new();
    public List<TypedFunctionDefinition> Functions { get; set; } = new();
    public TypedProgramIconStatement? ProgramIcon { get; set; } = null;
}