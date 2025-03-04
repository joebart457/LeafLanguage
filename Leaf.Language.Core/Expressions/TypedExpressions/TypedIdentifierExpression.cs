
using Assembler.Core;
using Assembler.Core.Models;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Tokenizer.Core.Models;

namespace Leaf.Language.TypedExpressions;
public class TypedIdentifierExpression : TypedExpression
{
    public Token Token { get; set; }
    public TypedIdentifierExpression(TypeInfo typeInfo, ExpressionBase originalExpression, Token identifier) : base(typeInfo, originalExpression)
    {
        Token = identifier;
    }

    public override void Compile(X86AssemblyContext asm)
    {
        var offset = asm.GetIdentifierOffset(Token.Lexeme);
        asm.Push(offset);
    }

    public RegisterOffset GetMemoryOffset(X86AssemblyContext asm) => asm.GetIdentifierOffset(Token.Lexeme);
}
