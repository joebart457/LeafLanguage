using Assembler.Core;
using Assembler.Core.Constants;
using Assembler.Core.Extensions;
using Assembler.Core.Instructions;
using Assembler.Core.Interfaces;
using Assembler.Core.Models;
using Leaf.Language.Compiler.Optimizer.Extensions;
using Leaf.Language.Compiler.Optimizer.Models;

namespace Leaf.Language.Compiler.Optimizer;


public static class X86AssemblyOptimizer
{

    public static void Optimize(X86AssemblyContext context)
    {
        foreach(var function in context.FunctionData)
        {
            Optimize(function);
        }
    }

    public static void Optimize(X86Function x86Function)
    {
        var memoryManager = new MemoryManager();
        var instructions = x86Function.Instructions;
        var optimizedInstructions = new List<X86Instruction>();
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            var nextInstruction = i + 1 < instructions.Count? instructions[i+1] : null;
            if (instruction is IRegister_Destination register_Destination)
            {
                if (!(instruction is IPop) && !IsRegisterReferenced(instructions, i + 1, register_Destination.Destination)) continue; // skip the instruction if the destination register is unused
            }
            else if (instruction is Pop_Register pop_Register)
            {
                if (!IsRegisterReferenced(instructions, i + 1, pop_Register.Destination))
                {
                    // change to adding esp to enable further optimizations next pass
                    optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Add(X86Register.esp, 4));
                    continue;
                }
            }
            else if (instruction is IPush && IsAddEsp4(nextInstruction))
            {
                i += 1;
                continue;
            }
            else if (instruction is Push_Register pushRegister && nextInstruction is Pop_Register popRegister1)
            {
                optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(popRegister1.Destination, pushRegister.Source));
                i += 1;
                continue;
            }
            else if (instruction is Push_Immediate push_Immediate && nextInstruction is Pop_Register popRegister2)
            {
                optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(popRegister2.Destination, push_Immediate.Immediate));
                i += 1;
                continue;
            }
            else if (instruction is Push_RegisterOffset push_RegisterOffset && nextInstruction is Pop_Register popRegister3)
            {
                optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(popRegister3.Destination, push_RegisterOffset.Source));
                i += 1;
                continue;
            }
            else if (instruction is Push_SymbolOffset push_SymbolOffset && nextInstruction is Pop_Register popRegister4)
            {
                optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(popRegister4.Destination, push_SymbolOffset.Source));
                i += 1;
                continue;
            }
            else if (instruction is Push_Address push_Address && nextInstruction is Pop_Register popRegister5)
            {
                optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(popRegister5.Destination, push_Address.Address));
                i += 1;
                continue;
            }
            else if (instruction is Mov_Register_Register mov_Register_Register)
            {
                if (mov_Register_Register.Destination == mov_Register_Register.Source) continue; // same register, so skip the mov
                if (!IsRegisterReferenced(instructions, i + 1, mov_Register_Register.Destination)) continue; // Register is unused so skip the mov
                if (memoryManager.TryGetImmediate(mov_Register_Register.Source, out var imm))
                {
                    if (imm!.IsAddress)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_Register_Register.Destination, imm!.AddressValue!));
                    else if (imm!.IsInt)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_Register_Register.Destination, imm!.IntValue!.Value));
                    else optimizedInstructions.AddAndTrack(memoryManager, instruction);
                    continue;
                }
            }
            else if (instruction is Mov_Register_RegisterOffset mov_Register_RegisterOffset)
            {
                if (!IsRegisterReferenced(instructions, i + 1, mov_Register_RegisterOffset.Destination)) continue; // Register is unused so skip the mov
                if (memoryManager.TryGetImmediate(mov_Register_RegisterOffset.Source, out var imm))
                {
                    if (imm!.IsAddress)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_Register_RegisterOffset.Destination, imm!.AddressValue!));
                    else if (imm!.IsInt)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_Register_RegisterOffset.Destination, imm!.IntValue!.Value));
                    else optimizedInstructions.AddAndTrack(memoryManager, instruction);
                    continue;
                }
            }
            else if (instruction is Mov_RegisterOffset_Register mov_RegisterOffset_Register)
            {
                if (memoryManager.TryGetImmediate(mov_RegisterOffset_Register.Source, out var imm))
                {
                    if (imm!.IsAddress)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_RegisterOffset_Register.Destination, imm!.AddressValue!));
                    else if (imm!.IsInt)
                        optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Mov(mov_RegisterOffset_Register.Destination, imm!.IntValue!.Value));
                    else optimizedInstructions.AddAndTrack(memoryManager, instruction);
                    continue;
                }
            }
            else if (instruction is Mov_Register_Immediate mov_Register_Immediate)
            {
                if (mov_Register_Immediate.ImmediateValue == 0)
                {
                    // test for 
                    // mov eax, 0
                    // optimization:
                    // xor eax, eax       (xor register, register faster than mov register, 0)
                    optimizedInstructions.AddAndTrack(memoryManager, X86Instructions.Xor(mov_Register_Immediate.Destination, mov_Register_Immediate.Destination));
                    continue;
                }
            }
            else if (instruction is Neg_Register neg_Register1 && nextInstruction is Neg_Register neg_Register2)
            {
                if (neg_Register1.Destination == neg_Register2.Destination)
                {
                    i += 1;
                    continue;
                }
            }
            else if (instruction is Neg_RegisterOffset neg_RegisterOffset1 && nextInstruction is Neg_RegisterOffset neg_RegisterOffset2)
            {
                if (neg_RegisterOffset1.Destination.Equals(neg_RegisterOffset2.Destination))
                {
                    i += 1;
                    continue;
                }
            }
            else if (instruction is Not_Register not_Register1 && nextInstruction is Not_Register not_Register2)
            {
                if (not_Register1.Destination == not_Register2.Destination)
                {
                    i += 1;
                    continue;
                }
            }
            else if (instruction is Not_RegisterOffset not_RegisterOffset1 && nextInstruction is Not_RegisterOffset not_RegisterOffset2)
            {
                if (not_RegisterOffset1.Destination.Equals(not_RegisterOffset2.Destination))
                {
                    i += 1;
                    continue;
                }
            }

            optimizedInstructions.AddAndTrack(memoryManager, instruction);
        }
        x86Function.Instructions = optimizedInstructions;
    }

    #region Helpers

    private static bool IsAddEsp4(X86Instruction? instruction) => instruction is Add_Register_Immediate add_Register_Immediate && add_Register_Immediate.Destination == X86Register.esp && add_Register_Immediate.ImmediateValue == 4;

    private static bool IsRegisterReferenced(List<X86Instruction> instructions, int index, X86Register x86Register)
    {
        if (x86Register == X86Register.esp) return true; // since esp holds the stack pointer, operations on it may be required even if it is not directly referenced
        // Determine if register is referenced before it is overwritten
        return TestInstructions((i, r) => IsRegisterReferencedBeforeOverwrite(i, r), instructions, index, instructions.Count, x86Register, new());
    }

    private static bool IsRegisterAltered(List<X86Instruction> instructions, int index, X86Register x86Register)
    {
        return TestInstructions((i, r) => IsRegisterAltered(i, r), instructions, index, instructions.Count, x86Register, new());
    }

    private static bool TestInstructions<T>(Func<X86Instruction, T, bool?> testFunction, List<X86Instruction> instructions, int startIndex, int endIndex, T testOperand, HashSet<string> exploredLabels)
    {
        for (var i = startIndex; i < endIndex; i++)
        {
            var testResult = testFunction(instructions[i], testOperand);
            if (testResult != null) return testResult.Value; // null means continue search
            else if (instructions[i] is IRet) return false;
            else if (instructions[i] is Label label) exploredLabels.Add(label.Text);
            else if (instructions[i] is IJmp jmp && !exploredLabels.Contains(jmp.Label))
            {
                var labelIndex = FindLabelIndex(instructions, jmp);
                if (labelIndex == -1) return true; // If the label is not referenced we must assume it is somewhere else where the testOperand (register or memory location) has the possibility of being referenced
                exploredLabels.Add(jmp.Label);
                var labelEndIndex = labelIndex > startIndex ? endIndex : startIndex;
                if (TestInstructions(testFunction, instructions, labelIndex, labelEndIndex, testOperand, exploredLabels)) return true;
            }
        }
        return false;
    }

    private static bool? IsRegisterReferencedBeforeOverwrite(X86Instruction instruction, X86Register register)
    {
        if (instruction is IRegister_Destination register_Destination1 && register_Destination1.Destination == register)
        {
            if (instruction is Mov_Register_Register) return false;
            if (instruction is Mov_Register_RegisterOffset) return false;
            if (instruction is Mov_Register_Immediate) return false;
            if (instruction is Mov_Register_SymbolOffset) return false;
            if (instruction is Mov_Register_Address) return false;
            return true;
        }
        if (instruction is IRegister_Source register_Source && register_Source.Source == register) return true;
        if (instruction is IRegister_Destination register_Destination2 && register_Destination2.Destination == register) return true;
        if (instruction is IByteRegister_Source byteRegister_Source && byteRegister_Source.Source.ToFullRegister() == register) return true;
        if (instruction is IByteRegister_Destination byteRegister_Destination && byteRegister_Destination.Destination.ToFullRegister() == register) return true;
        if (instruction is IRegisterOffset_Source registerOffset_Source && registerOffset_Source.Source.Register == register) return true;
        if (instruction is IRegisterOffset_Destination registerOffset_Destination && registerOffset_Destination.Destination.Register == register) return true;
        
        if (instruction is ICall && !register.IsRegisterPreservedThroughCall()) return false;
        if (instruction is IRet && register == X86Register.eax) return true;
        return null; // return null means continue search
    }

    private static bool? IsRegisterAltered(X86Instruction instruction, X86Register register)
    {
        if (instruction is IRegister_Destination register_Destination && register_Destination.Destination == register) return true;
        if (instruction is IByteRegister_Destination byteRegister_Destination && byteRegister_Destination.Destination.ToFullRegister() == register) return true;
        if (instruction is ICall && !register.IsRegisterPreservedThroughCall()) return false;
        return null; // return null means continue search
    }

    private static int FindLabelIndex(List<X86Instruction> instructions, IJmp jmp)
    {
        return instructions.FindIndex(x => x is Label label && label.Text == jmp.Label);
    }

    #endregion

}