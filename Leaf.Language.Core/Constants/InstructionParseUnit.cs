
namespace Leaf.Language.Core.Constants;


// For inline assembly parsing, added for fun and ease of inline assembly parsing/checking
public enum InstructionParseUnit
{
    GeneralRegister32,
    XmmRegister,
    RegisterOffset,
    SymbolOffset,
    Symbol,
    Immediate,
    ByteRegister,
    RegisterOffset_Byte,
    SymbolOffset_Byte,
}