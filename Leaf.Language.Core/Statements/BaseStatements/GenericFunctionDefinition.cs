using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using System.Runtime.InteropServices;
using Tokenizer.Core.Exceptions;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public class GenericFunctionDefinition : StatementBase
{
    public Token FunctionName { get; set; }
    public List<GenericTypeSymbol> GenericTypeParameters { get; set; }
    public TypeSymbol ReturnType { get; set; }
    public List<Parameter> Parameters { get; set; }
    public List<ExpressionBase> BodyStatements { get; set; }
    public CallingConvention CallingConvention { get; set; }
    public bool IsExported { get; set; }

    public GenericFunctionDefinition(Token functionName, List<GenericTypeSymbol> genericTypeParameters, TypeSymbol returnType, List<Parameter> parameters, List<ExpressionBase> bodyStatements) : base(functionName)
    {
        FunctionName = functionName;
        GenericTypeParameters = genericTypeParameters;
        ReturnType = returnType;
        Parameters = parameters;
        BodyStatements = bodyStatements;
        CallingConvention = CallingConvention.StdCall;
        IsExported = false;
    }

    public string GetDecoratedFunctionIdentifier()
    {
        if (CallingConvention == CallingConvention.Cdecl) return $"_{FunctionName.Lexeme}";
        if (CallingConvention == CallingConvention.StdCall) return $"_{FunctionName.Lexeme}@{Parameters.Count * 4}";
        throw new NotImplementedException();
    }

    public FunctionDefinition ToFunctionDefinition(List<TypeSymbol> typeArguments)
    {
        var unresolvedTypeParameter = typeArguments.Find(x => x.IsGenericTypeSymbol || x.ContainsGenericTypeSymbol);
        if (unresolvedTypeParameter != null)
            throw new ParsingException(FunctionName, $"invalid generic type arguments: unresolved generic type parameter {unresolvedTypeParameter}");
        if (typeArguments.Count != GenericTypeParameters.Count)
            throw new ParsingException(FunctionName, $"expected {GenericTypeParameters.Count} type arguments but got {typeArguments.Count}");

        var genericToConcreteTypeMap = new Dictionary<GenericTypeSymbol, TypeSymbol>();
        for (int i = 0; i < GenericTypeParameters.Count; i++)
        {
            if (genericToConcreteTypeMap.ContainsKey(GenericTypeParameters[i]))
                throw new ParsingException(GenericTypeParameters[i].TypeName, $"redefinition of generic type parameter {GenericTypeParameters[i].TypeName.Lexeme}");
            genericToConcreteTypeMap[GenericTypeParameters[i]] = typeArguments[i];
        }

        var newParameters = Parameters.Select(x => new Parameter(x.Name, x.TypeSymbol)).ToList();
        foreach (var parameter in newParameters)
        {
            parameter.TypeSymbol = parameter.TypeSymbol.ReplaceGenericTypeParameter(genericToConcreteTypeMap);
        }
        unresolvedTypeParameter = newParameters.Select(x => x.TypeSymbol).FirstOrDefault(x => x.IsGenericTypeSymbol || x.ContainsGenericTypeSymbol);
        if (unresolvedTypeParameter != null)
            throw new ParsingException(FunctionName, $"invalid generic type arguments: unresolved generic type parameter {unresolvedTypeParameter}");
        var newBodyStatements = BodyStatements.Select(x => x.ReplaceGenericTypeSymbols(genericToConcreteTypeMap)).ToList();

        var instantiatedFunctionName = $"{FunctionName.Lexeme}!{string.Join('_', typeArguments.Select(x => x.GetFlattenedName()))}";
        var instantiatedFunctionNameToken = new Token(FunctionName.Type, instantiatedFunctionName, FunctionName.Start, FunctionName.End);

        var returnType = ReturnType.ReplaceGenericTypeParameter(genericToConcreteTypeMap);

        return new FunctionDefinition(instantiatedFunctionNameToken, returnType, newParameters, newBodyStatements, CallingConvention, IsExported, instantiatedFunctionNameToken)
        {
            StartToken = StartToken,
            EndToken = EndToken,
        };

    }

    public override void GatherSignature(TypeResolver typeResolver)
    {
        typeResolver.GatherSignature(this);
    }

    public override TypedStatement Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public IEnumerable<LocalVariableExpression> ExtractLocalVariableExpressions()
    {
        return BodyStatements.SelectMany(e => ExtractLocalVariableExpressionsHelper(e));
    }

    private List<LocalVariableExpression> ExtractLocalVariableExpressionsHelper(ExpressionBase expression)
    {
        var ls = new List<LocalVariableExpression>();
        if (expression is CallExpression ce)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(ce.CallTarget));
            foreach (var arg in ce.Arguments) ls.AddRange(ExtractLocalVariableExpressionsHelper(arg));
        }
        else if (expression is CompilerIntrinsic_GetExpression ci_get) ls.AddRange(ExtractLocalVariableExpressionsHelper(ci_get.ContextPointer));
        else if (expression is CompilerIntrinsic_SetExpression ci_set) ls.AddRange(ExtractLocalVariableExpressionsHelper(ci_set.ContextPointer));
        else if (expression is GetExpression get)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(get.Instance));
        }
        else if (expression is IdentifierExpression id) { }
        else if (expression is InlineAssemblyExpression asm) { }
        else if (expression is LiteralExpression le) { }
        else if (expression is SetExpression tse)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(tse.AssignmentTarget));
            ls.AddRange(ExtractLocalVariableExpressionsHelper(tse.ValueToAssign));
        }
        else if (expression is LocalVariableExpression lve) ls.Add(lve);
        else if (expression is ReturnExpression tre)
        {
            if (tre.ReturnValue != null) ls.AddRange(ExtractLocalVariableExpressionsHelper(tre.ReturnValue));
        }
        else throw new InvalidOperationException($"unsupported expression type {expression.GetType().Name}");
        return ls;
    }
}