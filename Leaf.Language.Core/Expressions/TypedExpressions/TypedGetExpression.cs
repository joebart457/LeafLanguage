using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Tokenizer.Core.Models;


namespace Leaf.Language.TypedExpressions;

public class TypedGetExpression : TypedExpression
{
    public TypedExpression Instance { get; private set; }
    public Token TargetField { get; private set; }
    public bool ShortCircuitOnNull { get; private set; }    
    public TypedGetExpression(TypeInfo typeInfo, ExpressionBase originalExpression, TypedExpression instance, Token targetField,  bool shortCircuitOnNull): base(typeInfo, originalExpression)
    {
        Instance = instance;
        TargetField = targetField;
        ShortCircuitOnNull = shortCircuitOnNull;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        var offset = Instance.TypeInfo.GenericTypeArgument!.GetFieldOffset(TargetField);
        Instance.Compile(asm);
        asm.Pop(X86Register.esi);
        var fieldOffset = Offset.Create(X86Register.esi, offset);
        asm.Push(fieldOffset);
    }

    public RegisterOffset CompileAndReturnMemoryOffset(X86AssemblyContext asm)
    {
        var offset = Instance.TypeInfo.GenericTypeArgument!.GetFieldOffset(TargetField);
        Instance.Compile(asm);
        asm.Pop(X86Register.esi);
        var fieldOffset = Offset.Create(X86Register.esi, offset);
        return fieldOffset;
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (Instance.TryGetContainingExpression(line, column, out containingExpression)) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}