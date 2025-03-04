using Assembler.Core;
using Assembler.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Tokenizer.Core.Models;

namespace Leaf.Language.TypedExpressions;
public class TypedLocalVariableExpression : TypedExpression
{
    public Token Identifier { get; set; }
    public TypeInfo VariableType { get; set; }
    public TypedExpression? Initializer { get; set; }
    public TypedLocalVariableExpression(TypeInfo typeInfo, ExpressionBase originalExpression, Token identifier, TypeInfo variableType, TypedExpression? initializer) : base(typeInfo, originalExpression)
    {
        Identifier = identifier;
        VariableType = variableType;
        Initializer = initializer;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        if (Initializer != null)
        {
            Initializer.Compile(asm);
            // Since all local variables are 4 bytes logic can be simple
            var offset = asm.GetIdentifierOffset(Identifier.Lexeme);
            asm.Pop(X86Register.eax);
            asm.Mov(offset, X86Register.eax);
        }        
    }

    public override bool TryGetContainingExpression(int line, int column, out TypedExpression? containingExpression)
    {
        if (Initializer?.TryGetContainingExpression(line, column, out containingExpression) == true) return true;
        return base.TryGetContainingExpression(line, column, out containingExpression);
    }
}