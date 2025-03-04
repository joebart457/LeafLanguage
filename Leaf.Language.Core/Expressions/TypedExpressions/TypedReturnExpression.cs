using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;


namespace Leaf.Language.TypedExpressions;


public class TypedReturnExpression : TypedExpression
{
    public TypedExpression? ReturnValue { get; set; }
    public TypedReturnExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression? returnValue) : base(typeInfo, originalExpression)
    {
        ReturnValue = returnValue;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        if (ReturnValue != null)
        {
            ReturnValue.Compile(asm);

            if (ReturnValue.TypeInfo.Is(IntrinsicType.Float))
            {
                asm.Fld(Offset.Create(X86Register.esp, 0));
                asm.Add(X86Register.esp, 4);
            }
            else
            {
                asm.Pop(X86Register.eax);
            }
        }
        asm.TeardownStackFrame();
        asm.Return();
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (ReturnValue?.TryGetContainingExpression(line, column, out containingExpression) == true) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}