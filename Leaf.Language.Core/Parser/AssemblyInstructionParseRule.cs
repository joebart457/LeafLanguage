using Assembler.Core.Models;
using Leaf.Language.Core.Constants;


namespace Leaf.Language.Core.Parser;

public class AssemblyInstructionParsingRule
{
    public AssemblyInstructions AssemblyInstruction { get; set; }
    public List<InstructionParseUnit> ParseUnits { get; set; }
    public Func<ProgramParser, X86Instruction> InstructionParsingFunction { get; set; }

    public AssemblyInstructionParsingRule(AssemblyInstructions assemblyInstruction, List<InstructionParseUnit> parseUnits, Func<ProgramParser, X86Instruction> instructionParsingFunction)
    {
        AssemblyInstruction = assemblyInstruction;
        InstructionParsingFunction = instructionParsingFunction;
        ParseUnits = parseUnits;
    }

    public bool CanParse(ProgramParser programParser)
    {
        int tokenOffset = 0;
        if (!programParser.CanParse(AssemblyInstruction, ref tokenOffset)) return false;
        foreach (var parseUnit in ParseUnits)
        {
            if (!programParser.CanParse(parseUnit, ref tokenOffset)) return false;
        }
        return true;
    }

    public X86Instruction Parse(ProgramParser programParser)
    {
        return InstructionParsingFunction(programParser);
    }
}