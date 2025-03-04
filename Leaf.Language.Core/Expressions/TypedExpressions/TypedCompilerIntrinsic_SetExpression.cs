using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;

namespace Leaf.Language.Expressions;


public class TypedCompilerIntrinsic_SetExpression : TypedExpression
{
    public TypedExpression ContextPointer { get; set; }
    public int AssignmentOffset { get; set; }
    public TypedExpression ValueToAssign { get; set; }
    public TypedCompilerIntrinsic_SetExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression contextPointer, int assignmentOffset, TypedExpression valueToAssign) : base(typeInfo, originalExpression)
    {
        ContextPointer = contextPointer;
        AssignmentOffset = assignmentOffset;
        ValueToAssign = valueToAssign;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        ContextPointer.Compile(asm);
        ValueToAssign.Compile(asm);
        asm.Pop(X86Register.eax);
        asm.Pop(X86Register.esi);
        var contextOffset = Offset.Create(X86Register.esi, AssignmentOffset);
        asm.Mov(contextOffset, X86Register.eax);
        asm.Push(X86Register.eax);
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (ContextPointer.TryGetContainingExpression(line, column, out containingExpression)) return true;
        if (ValueToAssign.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}