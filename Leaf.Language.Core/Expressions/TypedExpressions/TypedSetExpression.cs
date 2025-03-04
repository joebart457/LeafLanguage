using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;

namespace Leaf.Language.TypedExpressions
{
    public class TypedSetExpression : TypedExpression
    {
        public TypedExpression AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public TypedSetExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression assignmentTarget, TypedExpression valueToAssign) : base(typeInfo, originalExpression)
        {
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override void Compile(X86AssemblyContext asm)
        {
            ValueToAssign.Compile(asm);
            RegisterOffset assignmentTargetOffset;
            if (AssignmentTarget is TypedIdentifierExpression typedIdentifierExpression)
            {
                assignmentTargetOffset = typedIdentifierExpression.GetMemoryOffset(asm);
            }
            else if (AssignmentTarget is TypedGetExpression typedGetExpression)
            {
                assignmentTargetOffset = typedGetExpression.CompileAndReturnMemoryOffset(asm);
            }
            else throw new InvalidOperationException($"assignment target can only be identifier or field");
            var register = GetFreeRegister(assignmentTargetOffset.Register);
            asm.Pop(register);
            asm.Mov(assignmentTargetOffset, register);
        }

        private X86Register GetFreeRegister(X86Register register)
        {
            List<X86Register> volatileRegisters = [X86Register.eax, X86Register.ebx, X86Register.ecx, X86Register.edx];
            return volatileRegisters.First(x => x != register);
        }

        public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
        {
            if (AssignmentTarget.TryGetContainingExpression(line, column, out containingExpression)) return true;
            if (ValueToAssign.TryGetContainingExpression(line, column, out containingExpression)) return true;
            return base.TryGetContainingExpression(line, column, out containingExpression);
        }
    }
}
