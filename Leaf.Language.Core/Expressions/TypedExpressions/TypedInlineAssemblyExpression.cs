using Assembler.Core;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;

namespace Leaf.Language.TypedExpressions;

public class TypedInlineAssemblyExpression : TypedExpression
{
    public X86Instruction AssemblyInstruction { get; set; }
    public TypedInlineAssemblyExpression(TypeInfo typeInfo, ExpressionBase originalExpression, X86Instruction assemblyInstruction) : base(typeInfo, originalExpression)
    {
        AssemblyInstruction = assemblyInstruction;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        asm.AddInstruction(AssemblyInstruction);
    }
}