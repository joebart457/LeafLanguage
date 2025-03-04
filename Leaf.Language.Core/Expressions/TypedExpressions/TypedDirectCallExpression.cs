using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using System.Runtime.InteropServices;
using Tokenizer.Core.Models;


namespace Leaf.Language.TypedExpressions;

public class TypedDirectCallExpression : TypedExpression
{
    public Token OriginalToken { get; private set; }
    public ITypedFunctionInfo CallTarget { get; private set; }
    public List<TypedExpression> Arguments { get; private set; }
    public TypedDirectCallExpression(TypeInfo typeInfo, ExpressionBase originalExpression, Token originalToken, ITypedFunctionInfo callTarget, List<TypedExpression> arguments) : base(typeInfo, originalExpression)
    {
        OriginalToken = originalToken;
        CallTarget = callTarget;
        Arguments = arguments;
    }

    public override void Compile(X86AssemblyContext asm)
    {

        bool returnsFloat = CallTarget.ReturnType.Is(IntrinsicType.Float);
        for (int i = Arguments.Count - 1; i >= 0; i--)
        {
            Arguments[i].Compile(asm);
        }
        if (CallTarget.IsImported)
        {
            asm.Call(CallTarget.GetDecoratedFunctionIdentifier(), true);
        }else
        {
            asm.Call(CallTarget.GetDecoratedFunctionIdentifier(), false);
        }

        if (CallTarget.CallingConvention == CallingConvention.Cdecl) asm.Add(X86Register.esp, Arguments.Count * 4);
        else if (CallTarget.CallingConvention != CallingConvention.StdCall) throw new InvalidOperationException($"Unsupported calling convention {CallTarget.CallingConvention}");
        if (returnsFloat)
        {
            asm.Sub(X86Register.esp, 4);
            asm.Fstp(Offset.Create(X86Register.esp, 0));
        }
        else if (!CallTarget.ReturnType.Is(IntrinsicType.Void)) asm.Push(X86Register.eax);
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        foreach (var argument in Arguments)
        {
            if (argument.TryGetContainingExpression(line, column, out containingExpression)) return true;
        }
        if (Contains(OriginalToken, line, column))
        {
            containingExpression = new TypedIdentifierExpression(CallTarget.GetFunctionPointerType(), new IdentifierExpression(OriginalToken), OriginalToken);
            return true;
        }
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }

    private bool Contains(Token token, int line, int column)
    {
        if (line == token.Start.Line && line == token.End.Line) return token.Start.Column <= column && column <= token.End.Column;
        if (line == token.Start.Line) return token.Start.Column <= column;
        if (line == token.End.Line) return token.End.Column >= column;
        return token.Start.Line <= line && token.End.Line >= line;
    }
}