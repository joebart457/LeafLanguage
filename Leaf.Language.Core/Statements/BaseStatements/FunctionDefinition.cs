using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public class FunctionDefinition : StatementBase
{
    public Token FunctionName { get; set; }
    public TypeSymbol ReturnType { get; set; }
    public List<Parameter> Parameters { get; set; }
    public List<ExpressionBase> BodyStatements { get; set; }
    public CallingConvention CallingConvention { get; set; }
    public bool IsExported { get; set; }
    public Token ExportedSymbol { get; set; }

    public FunctionDefinition(Token functionName, TypeSymbol returnType, List<Parameter> parameters, List<ExpressionBase> bodyStatements) : base(functionName)
    {
        FunctionName = functionName;
        ReturnType = returnType;
        Parameters = parameters;
        BodyStatements = bodyStatements;
        CallingConvention = CallingConvention.StdCall;
        IsExported = false;
        ExportedSymbol = functionName;
    }


    public FunctionDefinition(Token functionName, TypeSymbol returnType, List<Parameter> parameters, List<ExpressionBase> bodyStatements, CallingConvention callingConvention, bool isExported, Token exportedSymbol) : base(functionName)
    {
        FunctionName = functionName;
        ReturnType = returnType;
        Parameters = parameters;
        BodyStatements = bodyStatements;
        CallingConvention = callingConvention;
        IsExported = isExported;
        ExportedSymbol = exportedSymbol;
    }

    public override void GatherSignature(TypeResolver typeResolver)
    {
        typeResolver.GatherSignature(this);
    }

    public override TypedStatement Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public FunctionDefinition ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        var returnType = ReturnType.ReplaceGenericTypeParameter(genericToConcreteTypeMap);
        var parameters = Parameters.Select(x => new Parameter(x.Name, x.TypeSymbol.ReplaceGenericTypeParameter(genericToConcreteTypeMap))).ToList();
        var bodyStatements = BodyStatements.Select(x => x.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).ToList();

        return new FunctionDefinition(FunctionName, returnType, parameters, bodyStatements, CallingConvention, IsExported, ExportedSymbol)
        {
            StartToken = StartToken,
            EndToken = EndToken,
        };
    }

}