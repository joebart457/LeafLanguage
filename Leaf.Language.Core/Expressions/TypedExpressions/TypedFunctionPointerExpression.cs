using Assembler.Core;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;

namespace Leaf.Language.TypedExpressions;

public class TypedFunctionPointerExpression : TypedExpression
{
    public ITypedFunctionInfo FunctionInfo { get; private set; }
    public TypedFunctionPointerExpression(TypeInfo typeInfo, ExpressionBase originalExpression, ITypedFunctionInfo functionInfo) : base(typeInfo, originalExpression)
    {
        FunctionInfo = functionInfo;

    }

    public override void Compile(X86AssemblyContext asm)
    {
        if (FunctionInfo.IsImported)
            asm.Push(Offset.CreateSymbolOffset(FunctionInfo.GetDecoratedFunctionIdentifier(), 0));
        else asm.Push(FunctionInfo.GetDecoratedFunctionIdentifier());
    }
}