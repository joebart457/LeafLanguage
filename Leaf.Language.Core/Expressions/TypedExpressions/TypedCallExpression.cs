
using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using System.Runtime.InteropServices;

namespace Leaf.Language.TypedExpressions;

public class TypedCallExpression : TypedExpression
{
    public TypedExpression CallTarget { get; private set; }
    public List<TypedExpression> Arguments { get; private set; }
    public TypedCallExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression callTarget, List<TypedExpression> arguments) : base(typeInfo, originalExpression)
    {
        CallTarget = callTarget;
        Arguments = arguments;
    }

    public override void Compile(X86AssemblyContext asm)
    {

        if (!CallTarget.TypeInfo.IsFunctionPtr) throw new InvalidOperationException($"expect call to type fnptr<t> but got {CallTarget.TypeInfo}");
        bool returnsFloat = CallTarget.TypeInfo.FunctionReturnType.Is(IntrinsicType.Float);
        var callingConvention = CallTarget.TypeInfo.CallingConvention;
        for (int i = Arguments.Count - 1; i >= 0; i--)
        {
            Arguments[i].Compile(asm);
        }

        if (CallTarget is TypedIdentifierExpression idExpr)
        {
            var offset = asm.GetIdentifierOffset(idExpr.Token.Lexeme);
            asm.Call(offset);
        } else
        {
            CallTarget.Compile(asm);
            asm.Pop(X86Register.eax);
            asm.Call(X86Register.eax.ToString(), false);
        }

        if (callingConvention == CallingConvention.Cdecl) asm.Add(X86Register.esp, Arguments.Count * 4);
        else if (callingConvention != CallingConvention.StdCall) throw new InvalidOperationException($"Unsupported calling convention {callingConvention}");
        if (returnsFloat)
        {
            asm.Sub(X86Register.esp, 4);
            asm.Fstp(Offset.Create(X86Register.esp, 0));
        }
        else if (!CallTarget.TypeInfo.FunctionReturnType.Is(IntrinsicType.Void)) asm.Push(X86Register.eax);
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (CallTarget.TryGetContainingExpression(line, column, out containingExpression)) return true;
        foreach (var argument in Arguments)
        {
            if (argument.TryGetContainingExpression(line, column, out containingExpression)) return true;
        }
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}