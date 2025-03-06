using Assembler.Core.Constants;


namespace Leaf.Language.Compiler.Optimizer.Extensions;


internal static class X86RegisterExtensions
{
    public static bool IsRegisterPreservedThroughCall(this X86Register register)
    {
        return register == X86Register.ebx
            || register == X86Register.esi
            || register == X86Register.edi
            || register == X86Register.esp
            || register == X86Register.ebp;
    }
}