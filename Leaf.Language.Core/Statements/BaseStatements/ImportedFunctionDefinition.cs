using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public class ImportedFunctionDefinition : StatementBase
{
    public Token FunctionName { get; set; }
    public TypeSymbol ReturnType { get; set; }
    public List<Parameter> Parameters { get; set; }
    public CallingConvention CallingConvention { get; set; }
    public Token LibraryAlias { get; set; }
    public Token FunctionSymbol { get; set; }
    public ImportedFunctionDefinition(Token functionName, TypeSymbol returnType, List<Parameter> parameters, CallingConvention callingConvention, Token libraryAlias, Token functionSymbol) : base(functionName)
    {
        FunctionName = functionName;
        ReturnType = returnType;
        Parameters = parameters;
        CallingConvention = callingConvention;
        LibraryAlias = libraryAlias;
        FunctionSymbol = functionSymbol;
    }


    public string GetDecoratedFunctionIdentifier()
    {
        if (CallingConvention == CallingConvention.Cdecl) return $"_{FunctionName.Lexeme}";
        if (CallingConvention == CallingConvention.StdCall) return $"_{FunctionName.Lexeme}@{Parameters.Count * 4}";
        throw new NotImplementedException();
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