using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Expressions;
using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.TypedStatements;

public class TypedFunctionDefinition : TypedStatement, ITypedFunctionInfo
{
    public NamespaceSymbol Namespace { get; set; }
    public Token FunctionName { get; set; }
    public TypeInfo ReturnType { get; set; }
    public List<TypedParameter> Parameters { get; set; }
    public List<TypedExpression> BodyStatements { get; set; }
    public CallingConvention CallingConvention { get; set; }
    public bool IsExported { get; set; }
    public Token ExportedSymbol { get; set; }
    public bool IsImported => false;
    public Token FunctionSymbol => ExportedSymbol;

    public TypedFunctionDefinition(NamespaceSymbol @namespace, StatementBase originalStatement, Token functionName, TypeInfo returnType, List<TypedParameter> parameters, List<TypedExpression> bodyStatements, CallingConvention callingConvention, bool isExported, Token exportedSymbol) : base(originalStatement)
    {
        Namespace = @namespace;
        FunctionName = functionName;
        ReturnType = returnType;
        Parameters = parameters;
        BodyStatements = bodyStatements;
        CallingConvention = callingConvention;
        IsExported = isExported;
        ExportedSymbol = exportedSymbol;
    }


    public IEnumerable<TypedLocalVariableExpression> ExtractLocalVariableExpressions()
    {
        return BodyStatements.SelectMany(e => ExtractLocalVariableExpressionsHelper(e));
    }

    private List<TypedLocalVariableExpression> ExtractLocalVariableExpressionsHelper(TypedExpression expression)
    {
        var ls = new List<TypedLocalVariableExpression>();
        if (expression is TypedCallExpression ce)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(ce.CallTarget));
            foreach (var arg in ce.Arguments) ls.AddRange(ExtractLocalVariableExpressionsHelper(arg));
        }
        else if (expression is TypedCompilerIntrinsic_GetExpression ci_get) ls.AddRange(ExtractLocalVariableExpressionsHelper(ci_get.ContextPointer));
        else if (expression is TypedCompilerIntrinsic_SetExpression ci_set) ls.AddRange(ExtractLocalVariableExpressionsHelper(ci_set.ContextPointer));
        else if (expression is TypedGetExpression get)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(get.Instance));
        }
        else if (expression is TypedIdentifierExpression id) { }
        else if (expression is TypedInlineAssemblyExpression asm) { }
        else if (expression is TypedLiteralExpression le) { }
        else if (expression is TypedFunctionPointerExpression fpe) { }
        else if (expression is TypedSetExpression tse)
        {
            ls.AddRange(ExtractLocalVariableExpressionsHelper(tse.AssignmentTarget));
            ls.AddRange(ExtractLocalVariableExpressionsHelper(tse.ValueToAssign));
        }
        else if (expression is TypedLocalVariableExpression lve) ls.Add(lve);
        else if (expression is TypedReturnExpression tre)
        {
            if (tre.ReturnValue != null) ls.AddRange(ExtractLocalVariableExpressionsHelper(tre.ReturnValue));
        }
        else if (expression is TypedDirectCallExpression tdce) foreach (var arg in tdce.Arguments) ls.AddRange(ExtractLocalVariableExpressionsHelper(arg));
        else throw new InvalidOperationException($"unsupported expression type {expression.GetType().Name}");
        return ls;
    }

    public string GetDecoratedFunctionIdentifier()
    {
        if (CallingConvention == CallingConvention.Cdecl) return $"_{Namespace}.{FunctionName.Lexeme}";
        if (CallingConvention == CallingConvention.StdCall) return $"_{Namespace}.{FunctionName.Lexeme}@{Parameters.Count * 4}";
        throw new NotImplementedException($"No compiler support for calling convention {CallingConvention}");
    }

    public override void Compile(X86AssemblyContext asm)
    {
        asm.EnterFunction(new 
            X86Function(
                CallingConvention, 
                $"{Namespace}.{FunctionName.Lexeme}", 
                Parameters.Select(x => new X86FunctionLocalData(x.Name.Lexeme, x.TypeInfo.StackSize())).ToList(),
                ExtractLocalVariableExpressions().Select(x => new X86FunctionLocalData(x.Identifier.Lexeme, x.VariableType.StackSize())).ToList(),
                IsExported,
                ExportedSymbol.Lexeme));

        asm.SetupStackFrame();

        foreach (var statement in BodyStatements)
        {
            statement.Compile(asm);
            if (!statement.TypeInfo.Is(IntrinsicType.Void))
                asm.Pop(X86Register.eax); // Discard unused result of expression statement
        }

        asm.ExitFunction();
    }

    public FunctionPtrTypeInfo GetFunctionPointerType()
    {
        var intrinsicType = IntrinsicType.Func;
        if (CallingConvention == CallingConvention.Cdecl) intrinsicType = IntrinsicType.CFunc;
        var typeArguments = Parameters.Select(x => x.TypeInfo).ToList();
        typeArguments.Add(ReturnType);
        return new FunctionPtrTypeInfo(intrinsicType, typeArguments);
    }
}