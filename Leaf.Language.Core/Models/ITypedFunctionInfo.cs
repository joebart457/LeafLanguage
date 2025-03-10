
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Models;

public interface ITypedFunctionInfo
{
    public NamespaceSymbol Namespace { get; }
    public Token FunctionName { get; }
    public Token FunctionSymbol { get; }
    public TypeInfo ReturnType { get; }
    public List<TypedParameter> Parameters { get; }
    public CallingConvention CallingConvention { get; }
    public bool IsImported { get; }
    public bool IsExported { get; }
    public Token ExportedSymbol { get; }
    public string GetDecoratedFunctionIdentifier();
    public FunctionPtrTypeInfo GetFunctionPointerType();
}