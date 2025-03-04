
using Assembler.Core;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;


namespace Leaf.Language.TypedExpressions;


public class TypedLiteralExpression : TypedExpression
{
    public object? Value { get; private set; }
    public TypedLiteralExpression(TypeInfo typeInfo, ExpressionBase originalExpression, object? value): base(typeInfo, originalExpression)
    {
        Value = value;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        if (Value is string str)
        {
            var label = asm.AddStringData(str);
            asm.Push(label);
        }
        else if (Value == null)
        {
            asm.Push(0);
        }
        else if (Value is int i)
        {
            asm.Push(i);
        } 
        else if (Value is float f)
        {
            var label = asm.AddSinglePrecisionFloatingPointData(f);
            asm.Push(Offset.CreateSymbolOffset(label, 0));
        }
        else
        {
            throw new NotImplementedException($"literals are not implemented for type {Value.GetType()}");
        }
    }

}