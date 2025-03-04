using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;

namespace Leaf.Language.TypedExpressions;

public class TypedCompilerIntrinsic_GetExpression : TypedExpression
{
    public TypedExpression ContextPointer { get; set; }
    public int MemberOffset { get; set; }
    public TypedCompilerIntrinsic_GetExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression contextPointer, int memberOffset) : base(typeInfo, originalExpression)
    {
        ContextPointer = contextPointer;
        MemberOffset = memberOffset;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        ContextPointer.Compile(asm);
        asm.Pop(X86Register.esi);
        var contextOffset = Offset.Create(X86Register.esi, MemberOffset);
        asm.Push(contextOffset);
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (ContextPointer.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}