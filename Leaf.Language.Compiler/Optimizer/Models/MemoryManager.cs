using Assembler.Core.Constants;
using Assembler.Core.Instructions;
using Assembler.Core.Interfaces;
using Assembler.Core.Models;
using Leaf.Language.Compiler.Optimizer.Extensions;


namespace Leaf.Language.Compiler.Optimizer.Models;


internal class MemoryManager
{
    private Dictionary<X86Register, ImmediateValue> _registerMap = new();
    private Dictionary<RegisterOffset, ImmediateValue> _registerOffsetMap = new();

    public void Reset()
    {
        _registerMap = new();
        _registerOffsetMap = new();
    }

    public X86Instruction TrackInstruction(X86Instruction instruction)
    {
        TrackRegister(instruction);
        TrackRegisterOffset(instruction);
        TrackCall(instruction);
        TrackLabel(instruction);
        return instruction;
    }

    public bool TryGetImmediate(X86Register register, out ImmediateValue? immediateValue)
    {
        return _registerMap.TryGetValue(register, out immediateValue);
    }

    public bool TryGetImmediate(RegisterOffset registerOffset, out ImmediateValue? immediateValue)
    {
        return _registerOffsetMap.TryGetValue(registerOffset, out immediateValue);
    }

    private void TrackRegister(X86Instruction instruction)
    {
        if (instruction is IRegister_Destination register_Destination)
        {
            X86Register register = register_Destination.Destination;
            _registerMap.TryGetValue(register, out var currentImm);
            ImmediateValue? imm = currentImm;
            if (instruction is Mov_Register_Register mov_Register_Register) _registerMap.TryGetValue(mov_Register_Register.Source, out imm);
            else if (instruction is Mov_Register_RegisterOffset mov_Register_RegisterOffset) _registerOffsetMap.TryGetValue(mov_Register_RegisterOffset.Source, out imm);
            else if (instruction is Mov_Register_Immediate mov_Register_Immediate) imm = ImmediateValue.Create(mov_Register_Immediate.ImmediateValue);
            else if (instruction is Mov_Register_Address mov_Register_Address) imm = ImmediateValue.Create(mov_Register_Address.Address);
            else if (instruction is Inc_Register inc_Register && currentImm?.IsInt == true) imm!.IntValue = imm.IntValue + 1;
            else if (instruction is Dec_Register dec_Register && currentImm?.IsInt == true) imm!.IntValue = imm.IntValue - 1;
            else imm = null;
            if (imm != null) _registerMap[register] = imm;
            else Clear(register);
        }
    }

    private void TrackRegisterOffset(X86Instruction instruction)
    {
        if (instruction is IRegisterOffset_Destination registerOffset_Destination)
        {
            RegisterOffset registerOffset = registerOffset_Destination.Destination;
            _registerOffsetMap.TryGetValue(registerOffset, out var currentImm);
            ImmediateValue? imm = currentImm;
            if (instruction is Mov_RegisterOffset_Register mov_RegisterOffset_Register) _registerMap.TryGetValue(mov_RegisterOffset_Register.Source, out imm);
            else if (instruction is Mov_RegisterOffset_Immediate mov_RegisterOffset_Immediate) imm = ImmediateValue.Create(mov_RegisterOffset_Immediate.ImmediateValue);
            else if (instruction is Mov_RegisterOffset_Address mov_RegisterOffset_Address) imm = ImmediateValue.Create(mov_RegisterOffset_Address.Address);
            else if (instruction is Inc_RegisterOffset inc_RegisterOffset && currentImm?.IsInt == true) imm!.IntValue = imm.IntValue + 1;
            else if (instruction is Dec_RegisterOffset dec_RegisterOffset && currentImm?.IsInt == true) imm!.IntValue = imm.IntValue - 1;
            else imm = null;
            if (imm != null) _registerOffsetMap[registerOffset] = imm;
            else Clear(registerOffset);
        }
    }

    public void Clear(X86Register register)
    {
        _registerMap.Remove(register);
    }
    public void Clear(RegisterOffset registerOffset)
    {
        _registerOffsetMap.Remove(registerOffset);
    }

    public void TrackCall(X86Instruction instruction)
    {
        if (instruction is ICall)
        {
            _registerOffsetMap.Clear();
            foreach (var key in _registerMap.Keys)
            {
                if (!key.IsRegisterPreservedThroughCall()) _registerMap.Remove(key);
            }
        }
        
    }

    public void TrackLabel(X86Instruction instruction)
    {
        if (instruction is Label) Reset();
    }
}