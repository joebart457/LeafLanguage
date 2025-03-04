using Assembler.Core;
using Assembler.Core.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;
using static Assembler.Core.X86AssemblyContext;

namespace Leaf.Language.Core.Statements.TypedStatements;

public class TypedImportedFunctionDefinition : TypedStatement, ITypedFunctionInfo
{
    public Token FunctionName { get; set; }
    public TypeInfo ReturnType { get; set; }
    public List<TypedParameter> Parameters { get; set; }
    public CallingConvention CallingConvention { get; set; }
    public Token LibraryAlias { get; set; }
    public Token FunctionSymbol { get; set; }
    public bool IsImported => true;
    public bool IsExported => false;
    public Token ExportedSymbol => throw new InvalidOperationException($"function {FunctionName.Lexeme} cannot be exported: export forwarding not supported");
    public TypedImportedFunctionDefinition(StatementBase originalStatement, Token functionName, TypeInfo returnType, List<TypedParameter> parameters, CallingConvention callingConvention, Token libraryAlias, Token functionSymbol) : base(originalStatement)
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

    public override void Compile(X86AssemblyContext asm)
    {
        asm.AddImportedFunction(
            new ImportedFunction(
                CallingConvention,
                LibraryAlias.Lexeme,
                FunctionSymbol.Lexeme,
                GetDecoratedFunctionIdentifier(),
                Parameters.Select(x => new X86FunctionLocalData(x.Name.Lexeme, x.TypeInfo.StackSize())).ToList()));
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