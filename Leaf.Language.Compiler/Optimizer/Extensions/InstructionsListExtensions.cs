using Assembler.Core.Models;
using Leaf.Language.Compiler.Optimizer.Models;


namespace Leaf.Language.Compiler.Optimizer.Extensions;


internal static class InstructionsListExtensions
{
    public static void AddAndTrack(this List<X86Instruction> x86Instructions, MemoryManager memoryManager, X86Instruction instruction)
    {
        x86Instructions.Add(memoryManager.TrackInstruction(instruction));
    }
}