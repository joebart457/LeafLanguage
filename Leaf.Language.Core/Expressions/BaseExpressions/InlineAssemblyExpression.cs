using Assembler.Core.Models;
using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Models;


namespace Leaf.Language.Core.Expressions.BaseExpressions;

public class InlineAssemblyExpression : ExpressionBase
{
    public X86Instruction AssemblyInstruction { get; set; }
    public InlineAssemblyExpression(Token token, X86Instruction assemblyInstruction) : base(token)
    {
        AssemblyInstruction = assemblyInstruction;
    }

    public override TypedExpression Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }

    public override ExpressionBase ReplaceGenericTypeSymbols(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        return new InlineAssemblyExpression(Token, AssemblyInstruction).CopyStartAndEndTokens(this);
    }
}
