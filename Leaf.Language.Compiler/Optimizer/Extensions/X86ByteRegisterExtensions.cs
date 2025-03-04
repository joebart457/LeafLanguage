using Assembler.Core.Constants;

namespace Leaf.Language.Compiler.Optimizer.Extensions;

public static class X86ByteRegisterExtensions
{
    public static X86Register ToFullRegister(this X86ByteRegister byteRegister)
    {
        if (byteRegister == X86ByteRegister.al) return X86Register.eax;
        if (byteRegister == X86ByteRegister.bl) return X86Register.ebx;
        throw new InvalidOperationException();
    }
}