﻿
namespace Leaf.Language.Core.Constants;

public static class TokenTypes
{
    public const string LParen = "LParen";
    public const string RParen = "RParen";
    public const string LBracket = "LBracket";
    public const string RBracket = "RBracket";
    public const string Dot = "Dot";
    public const string NullDot = "NullDot";
    public const string Comma = "Comma";
    public const string Colon = "Colon";
    public const string CallingConvention = "CallingConvention";
    public const string IntrinsicType = "IntrinsicType";

    public const string True = "True";
    public const string False = "False";
    public const string Null = "Null";

    public const string DefineFunction = "DefineFunction";
    public const string Import = "Import";
    public const string Param = "Param";
    public const string Symbol = "Symbol";
    public const string Library = "Library";
    public const string Return = "Return";
    public const string InlineAssembly = "InlineAssembly";
    public const string CompilerIntrinsicSet = "CompilerIntrinsicSet";
    public const string CompilerIntrinsicGet = "CompilerIntrinsicGet";
    public const string Type = "Type";
    public const string Field = "Field";
    public const string Set = "Set";
    public const string Gen = "Gen";
    public const string Local = "Local";
    public const string Export = "Export";

    public const string Icon = "Icon";


    // The following token types are used purely for inline assembly parsing
    public const string AssemblyInstruction = "AssemblyInstruction";
    public const string GeneralRegister32 = "GeneralRegister32";
    public const string XmmRegister = "XmmRegister";
    public const string ByteRegister = "ByteRegister";
    public const string Byte = "Byte";

    public const string Plus = "Plus";
    public const string Minus = "Minus";
}