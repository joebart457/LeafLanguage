

using Assembler.Core.Constants;
using Assembler.Core.Instructions;
using Assembler.Core.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using System.Runtime.InteropServices;
using Tokenizer.Core;
using Tokenizer.Core.Constants;
using Tokenizer.Core.Exceptions;
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Parser;


public class ProgramParser : TokenParser
{
    private TextTokenizer _tokenizer;
    private Dictionary<string, GenericTypeSymbol> _validScopedGenericTypeParameters = new();
    private List<AssemblyInstructionParsingRule> _assemblyParsingRules = new()
    {
                        new(AssemblyInstructions.Cdq, [],
                        (p) => {
                                return X86Instructions.Cdq();
                        }),
                new(AssemblyInstructions.Push, [InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var register = p.ParseGeneralRegister32();
                                return X86Instructions.Push(register);
                        }),
                new(AssemblyInstructions.Push, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var offset = p.ParseRegisterOffset();
                                return X86Instructions.Push(offset);
                        }),
                new(AssemblyInstructions.Push, [InstructionParseUnit.Symbol],
                        (p) => {
                                var address = p.ParseSymbol();
                                return X86Instructions.Push(address);
                        }),
                new(AssemblyInstructions.Push, [InstructionParseUnit.Immediate],
                        (p) => {
                                var immediateValue = p.ParseImmediate();
                                return X86Instructions.Push(immediateValue);
                        }),
                new(AssemblyInstructions.Push, [InstructionParseUnit.SymbolOffset],
                        (p) => {
                                var offset = p.ParseSymbolOffset();
                                return X86Instructions.Push(offset);
                        }),
                new(AssemblyInstructions.Lea, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Lea(destination,source);
                        }),
                new(AssemblyInstructions.Lea, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.SymbolOffset],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseSymbolOffset();
                                return X86Instructions.Lea(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.RegisterOffset,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.RegisterOffset,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                var immediate = p.ParseImmediate();
                                return X86Instructions.Mov(destination,immediate);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var immediate = p.ParseImmediate();
                                return X86Instructions.Mov(destination,immediate);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.SymbolOffset,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseSymbolOffset();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.SymbolOffset,InstructionParseUnit.ByteRegister],
                        (p) => {
                                var destination = p.ParseSymbolOffset();
                                var source = p.ParseByteRegister();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.SymbolOffset,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseSymbolOffset();
                                var immediateValue = p.ParseImmediate();
                                return X86Instructions.Mov(destination,immediateValue);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.SymbolOffset_Byte,InstructionParseUnit.ByteRegister],
                        (p) => {
                                var destination = p.ParseSymbolOffset_Byte();
                                var source = p.ParseByteRegister();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.RegisterOffset_Byte,InstructionParseUnit.ByteRegister],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                var source = p.ParseByteRegister();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Mov, [InstructionParseUnit.RegisterOffset,InstructionParseUnit.ByteRegister],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                var source = p.ParseByteRegister();
                                return X86Instructions.Mov(destination,source);
                        }),
                new(AssemblyInstructions.Movsx, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset_Byte],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Movsx(destination,source);
                        }),
                new(AssemblyInstructions.Movsx, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.SymbolOffset_Byte],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseSymbolOffset_Byte();
                                return X86Instructions.Movsx(destination,source);
                        }),
                new(AssemblyInstructions.Sub, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var valueToSubtract = p.ParseImmediate();
                                return X86Instructions.Sub(destination,valueToSubtract);
                        }),
                new(AssemblyInstructions.Sub, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Sub(destination,source);
                        }),
                new(AssemblyInstructions.Add, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var value = p.ParseImmediate();
                                return X86Instructions.Add(destination,value);
                        }),
                new(AssemblyInstructions.Add, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Add(destination,source);
                        }),
                new(AssemblyInstructions.And, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.And(destination,source);
                        }),
                new(AssemblyInstructions.Or, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Or(destination,source);
                        }),
                new(AssemblyInstructions.Xor, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.Xor(destination,source);
                        }),
                new(AssemblyInstructions.Pop, [InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                return X86Instructions.Pop(destination);
                        }),
                new(AssemblyInstructions.Neg, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                return X86Instructions.Neg(destination);
                        }),
                new(AssemblyInstructions.Not, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                return X86Instructions.Not(destination);
                        }),
                new(AssemblyInstructions.Inc, [InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                return X86Instructions.Inc(destination);
                        }),
                new(AssemblyInstructions.Dec, [InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                return X86Instructions.Dec(destination);
                        }),
                new(AssemblyInstructions.Inc, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                return X86Instructions.Inc(destination);
                        }),
                new(AssemblyInstructions.Dec, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                return X86Instructions.Dec(destination);
                        }),
                new(AssemblyInstructions.IDiv, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var divisor = p.ParseRegisterOffset();
                                return X86Instructions.IDiv(divisor);
                        }),
                new(AssemblyInstructions.IMul, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseGeneralRegister32();
                                return X86Instructions.IMul(destination,source);
                        }),
                new(AssemblyInstructions.IMul, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.Immediate],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var immediate = p.ParseImmediate();
                                return X86Instructions.IMul(destination,immediate);
                        }),
                new(AssemblyInstructions.Add, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Add(destination,source);
                        }),
                new(AssemblyInstructions.Jmp, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jmp(label);
                        }),
                new(AssemblyInstructions.JmpGt, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpGt(label);
                        }),
                new(AssemblyInstructions.JmpGte, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpGte(label);
                        }),
                new(AssemblyInstructions.JmpLt, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpLt(label);
                        }),
                new(AssemblyInstructions.JmpLte, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpLte(label);
                        }),
                new(AssemblyInstructions.JmpEq, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpEq(label);
                        }),
                new(AssemblyInstructions.JmpNeq, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.JmpNeq(label);
                        }),
                new(AssemblyInstructions.Jz, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jz(label);
                        }),
                new(AssemblyInstructions.Jnz, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jnz(label);
                        }),
                new(AssemblyInstructions.Js, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Js(label);
                        }),
                new(AssemblyInstructions.Jns, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jns(label);
                        }),
                new(AssemblyInstructions.Ja, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Ja(label);
                        }),
                new(AssemblyInstructions.Jae, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jae(label);
                        }),
                new(AssemblyInstructions.Jb, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jb(label);
                        }),
                new(AssemblyInstructions.Jbe, [InstructionParseUnit.Symbol],
                        (p) => {
                                var label = p.ParseSymbol();
                                return X86Instructions.Jbe(label);
                        }),
                new(AssemblyInstructions.Test, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var operand1 = p.ParseGeneralRegister32();
                                var operand2 = p.ParseGeneralRegister32();
                                return X86Instructions.Test(operand1,operand2);
                        }),
                new(AssemblyInstructions.Test, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var operand1 = p.ParseGeneralRegister32();
                                var operand2 = p.ParseRegisterOffset();
                                return X86Instructions.Test(operand1,operand2);
                        }),
                new(AssemblyInstructions.Cmp, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var operand1 = p.ParseGeneralRegister32();
                                var operand2 = p.ParseGeneralRegister32();
                                return X86Instructions.Cmp(operand1,operand2);
                        }),
                new(AssemblyInstructions.Cmp, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.Immediate],
                        (p) => {
                                var operand1 = p.ParseGeneralRegister32();
                                var operand2 = p.ParseImmediate();
                                return X86Instructions.Cmp(operand1,operand2);
                        }),
                new(AssemblyInstructions.Cmp, [InstructionParseUnit.ByteRegister,InstructionParseUnit.ByteRegister],
                        (p) => {
                                var operand1 = p.ParseByteRegister();
                                var operand2 = p.ParseByteRegister();
                                return X86Instructions.Cmp(operand1,operand2);
                        }),
                new(AssemblyInstructions.Call, [InstructionParseUnit.Symbol],
                        (p) => {
                                var callee = p.ParseSymbol();
                                return X86Instructions.Call(callee, false);
                        }),
                new(AssemblyInstructions.Call, [InstructionParseUnit.SymbolOffset],
                        (p) => {
                                var callee = p.ParseSymbolOffset();
                                return X86Instructions.Call(callee.Symbol, true);
                        }),
                new(AssemblyInstructions.Call, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var callee = p.ParseRegisterOffset();
                                return X86Instructions.Call(callee);
                        }),
                new(AssemblyInstructions.Call, [InstructionParseUnit.GeneralRegister32],
                        (p) => {
                                var callee = p.ParseGeneralRegister32();
                                return X86Instructions.Call(callee);
                        }),
                new(AssemblyInstructions.Label, [InstructionParseUnit.Symbol],
                        (p) => {
                                var text = p.ParseSymbol();
                                return X86Instructions.Label(text);
                        }),
                new(AssemblyInstructions.Ret, [],
                        (p) => {
                                return X86Instructions.Ret();
                        }),
                new(AssemblyInstructions.Ret, [InstructionParseUnit.Immediate],
                        (p) => {
                                var immediate = p.ParseImmediate();
                                return X86Instructions.Ret((ushort)immediate);
                        }),
                new(AssemblyInstructions.Fstp, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                return X86Instructions.Fstp(destination);
                        }),
                new(AssemblyInstructions.Fld, [InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Fld(source);
                        }),
                new(AssemblyInstructions.Movss, [InstructionParseUnit.RegisterOffset,InstructionParseUnit.XmmRegister],
                        (p) => {
                                var destination = p.ParseRegisterOffset();
                                var source = p.ParseXmmRegister();
                                return X86Instructions.Movss(destination,source);
                        }),
                new(AssemblyInstructions.Movss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Movss(destination,source);
                        }),
                new(AssemblyInstructions.Movss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.XmmRegister],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseXmmRegister();
                                return X86Instructions.Movss(destination,source);
                        }),
                new(AssemblyInstructions.Comiss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Comiss(destination,source);
                        }),
                new(AssemblyInstructions.Comiss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.XmmRegister],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseXmmRegister();
                                return X86Instructions.Comiss(destination,source);
                        }),
                new(AssemblyInstructions.Ucomiss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.XmmRegister],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseXmmRegister();
                                return X86Instructions.Ucomiss(destination,source);
                        }),
                new(AssemblyInstructions.Addss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Addss(destination,source);
                        }),
                new(AssemblyInstructions.Subss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Subss(destination,source);
                        }),
                new(AssemblyInstructions.Mulss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Mulss(destination,source);
                        }),
                new(AssemblyInstructions.Divss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Divss(destination,source);
                        }),
                new(AssemblyInstructions.Cvtsi2ss, [InstructionParseUnit.XmmRegister,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseXmmRegister();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Cvtsi2ss(destination,source);
                        }),
                new(AssemblyInstructions.Cvtss2si, [InstructionParseUnit.GeneralRegister32,InstructionParseUnit.RegisterOffset],
                        (p) => {
                                var destination = p.ParseGeneralRegister32();
                                var source = p.ParseRegisterOffset();
                                return X86Instructions.Cvtss2si(destination,source);
                        }),
    };
    public ProgramParser(TextTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
        OverrideCurrentOnNull = true;
    }

    public ProgramParser()
    {
        _tokenizer = Tokenizers.Default;
        OverrideCurrentOnNull = true;
    }
    public ParsingResult ParseFile(string path, out List<ParsingException> errors)
    {
        var result = ParseText(File.ReadAllText(path), out errors);
        result.SourceFilePath = path;
        return result;
    }

    public ParsingResult ParseText(string text, out List<ParsingException> errors)
    {
        var tokenizer = Tokenizers.Default;
        errors = new List<ParsingException>();
        var result = new ParsingResult();

        var tokens = tokenizer.Tokenize(text, false)
            .Where(token => token.Type != BuiltinTokenTypes.EndOfFile)
            .ToList();

        Initialize(tokens);
        while (!AtEnd())
        {
            try
            {
                _validScopedGenericTypeParameters = new();
                var next = ParseNext();
                if (next == null) break;
                if (next is TypeDefinition typeDefinition) result.TypeDefinitions.Add(typeDefinition);
                else if (next is GenericTypeDefinition genericTypeDefinition) result.GenericTypeDefinitions.Add(genericTypeDefinition);
                else if (next is FunctionDefinition functionDefinition) result.FunctionDefinitions.Add(functionDefinition);
                else if (next is GenericFunctionDefinition genericFunctionDefinition) result.GenericFunctionDefinitions.Add(genericFunctionDefinition);
                else if (next is ImportedFunctionDefinition importedFunctionDefinition) result.ImportedFunctionDefinitions.Add(importedFunctionDefinition);
                else if (next is ImportLibraryDefinition importLibraryDefinition) result.ImportLibraryDefinitions.Add(importLibraryDefinition);
                else if (next is ProgramIconStatement programIconStatement)
                {
                    if (result.ProgramIconStatement != null) throw new ParsingException(programIconStatement.IconFilePath, $"program icon has already been set");
                    result.ProgramIconStatement = programIconStatement;
                }
                else throw new ParsingException(next.Token, $"unsupported statement type {next.GetType().Name}");
            }
            catch (ParsingException e)
            {
                errors.Add(e);
                SeekToNextParsableUnit();
            }
        }
        return result;
    }

    private void SeekToNextParsableUnit()
    {
        while (!AtEnd())
        {
            Advance();
            if (Match(TokenTypes.LParen)) break;
        }
    }

    private StatementBase? ParseNext()
    {
        var startToken = Current();
        var statement = ParseStatement();
        if (statement == null) return null;
        var endToken = Previous();
        statement.StartToken = startToken;
        statement.EndToken = endToken;
        return statement;
    }

    private StatementBase ParseTypeDefinition()
    {
        var name = Consume(BuiltinTokenTypes.Word, "expect type name");
        List<GenericTypeSymbol> genericTypeParameters = new();
        if (AdvanceIfMatch(TokenTypes.LBracket))
        {
            do
            {
                genericTypeParameters.Add(ParseGenericTypeSymbol());
            } while (AdvanceIfMatch(TokenTypes.Comma));
            Consume(TokenTypes.RBracket, "expect enclosing ] after generic type parameter list");
        }
        var fields = new List<TypeDefinitionField>();
        do
        {
            Consume(TokenTypes.LParen, "expect field definition");
            Consume(TokenTypes.Field, "expect field definition. IE (field x int)");
            var fieldName = Consume(BuiltinTokenTypes.Word, "expect field name");
            ReclassifyToken(fieldName, ReclassifiedTokenTypes.TypeField);
            var typeInfo = ParseTypeSymbol();

            Consume(TokenTypes.RParen, "expect enclosing ) in field definition");
            fields.Add(new(typeInfo, fieldName));
        } while (!AtEnd() && !Match(TokenTypes.RParen));
        Consume(TokenTypes.RParen, "expect enclosing ) in type definition");
        if (genericTypeParameters.Any()) return new GenericTypeDefinition(name, genericTypeParameters, fields);
        return new TypeDefinition(name, fields);
    }

    public TypeSymbol ParseTypeSymbol()
    {
        Token typeName;
        if (AdvanceIfMatch(TokenTypes.IntrinsicType)) typeName = Previous();
        else typeName = Consume(BuiltinTokenTypes.Word, "expect type annotation");
        if (_validScopedGenericTypeParameters.TryGetValue(typeName.Lexeme, out var genericTypeSymbol)) return genericTypeSymbol;
        List<TypeSymbol> typeArguments = new();
        if (AdvanceIfMatch(TokenTypes.LBracket))
        {
            do
            {
                var typeArgument = ParseTypeSymbol();
                typeArguments.Add(typeArgument);
            } while (AdvanceIfMatch(TokenTypes.Comma));
            Consume(TokenTypes.RBracket, "expect enclosing ] after type arguments");
        }

        return new TypeSymbol(typeName, typeArguments);
    }

    private GenericTypeSymbol ParseGenericTypeSymbol()
    {
        Consume(TokenTypes.Gen, "expect generic symbol");
        var typeSymbol = Consume(BuiltinTokenTypes.Word, "expect type annotation");
        if (_validScopedGenericTypeParameters.ContainsKey(typeSymbol.Lexeme))
            throw new ParsingException(typeSymbol, $"redefintion of generic parameter with name {typeSymbol.Lexeme}");
        var genericTypeSymbol = new GenericTypeSymbol(typeSymbol);
        _validScopedGenericTypeParameters[typeSymbol.Lexeme] = genericTypeSymbol;
        return genericTypeSymbol;
    }

    public StatementBase? ParseStatement()
    {
        if (AtEnd()) return null;
        Consume(TokenTypes.LParen, "expect all statements to begin with (");
        if (AdvanceIfMatch(TokenTypes.Export)) return ParseFunctionDefinition(true);
        if (AdvanceIfMatch(TokenTypes.DefineFunction)) return ParseFunctionDefinition();
        if (AdvanceIfMatch(TokenTypes.Import)) return ParseImportedFunctionDefinition();
        if (AdvanceIfMatch(TokenTypes.Library)) return ParseImportLibraryDefinition();
        if (AdvanceIfMatch(TokenTypes.Type)) return ParseTypeDefinition();
        if (AdvanceIfMatch(TokenTypes.Icon)) return ParseProgramIconStatement();
        throw new ParsingException(Current(), $"unexpected token {Current()}");
    }

    public StatementBase ParseProgramIconStatement()
    {
        var iconFilePath = Consume(BuiltinTokenTypes.Word, "expect (icon `icon/file/path.ico`)");
        Consume(TokenTypes.RParen, "expect enclosing ) after icon specifier");
        return new ProgramIconStatement(iconFilePath);
    }

    public StatementBase ParseFunctionDefinition(bool isExported = false)
    {
        /*
         * (defn main:int (param argc int) (param argv ptr[string]
         *  //...
         * )
         * 
         *  (defn add[gen T]:T (param x gen T) (param y gen T)
         *  //...
         *  )
         *  
         *   (export defn main:int (param argc int) (param argv ptr[string]
         *  //...
         * )
         */
        if (isExported) Consume(TokenTypes.DefineFunction, "only functions can be exported");
        Token name = Consume(BuiltinTokenTypes.Word, "expect function name");
        var genericTypeParameters = new List<GenericTypeSymbol>();
        if (AdvanceIfMatch(TokenTypes.LBracket))
        {
            do
            {
                genericTypeParameters.Add(ParseGenericTypeSymbol());
            } while (AdvanceIfMatch(TokenTypes.Comma));
            Consume(TokenTypes.RBracket, "expect enclosing ] after generic type parameter list");
        }
        Consume(TokenTypes.Colon, "expect functionName:returnType");
        var returnType = ParseTypeSymbol();
        var parameters = new List<Parameter>();
        if (Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param))
        {
            do
            {
                Consume(TokenTypes.LParen, "expect parameter definition");
                Consume(TokenTypes.Param, "expect parameter definition");
                var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name");
                var parameterType = ParseTypeSymbol();
                Consume(TokenTypes.RParen, "expect enclosing ) in parameter definition");
                parameters.Add(new(parameterName, parameterType));
            } while (!AtEnd() && Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param));
        }
        var body = new List<ExpressionBase>();
        if (!AdvanceIfMatch(TokenTypes.RParen))
        {
            do
            {
                body.Add(ParseExpression());
            } while (!AtEnd() && !Match(TokenTypes.RParen));
            Consume(TokenTypes.RParen, "expect enclosing ) in function body");
        }
        if (genericTypeParameters.Any())
        {
            if (isExported) throw new ParsingException(name, "only non-generic functions can be exported");
            return new GenericFunctionDefinition(name, genericTypeParameters, returnType, parameters, body);
        }
        return new FunctionDefinition(name, returnType, parameters, body, CallingConvention.StdCall, isExported, name);
    }

    public FunctionDefinition ParseLambdaFunctionDefinition()
    {
        /*
         * (defn int (param argc int) (param argv ptr[string])
         *  //...
         * )
         * 
         */
        Token name = Previous();
        var returnType = ParseTypeSymbol();
        var parameters = new List<Parameter>();
        if (Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param))
        {
            do
            {
                Consume(TokenTypes.LParen, "expect parameter definition");
                Consume(TokenTypes.Param, "expect parameter definition");
                var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name");
                var parameterType = ParseTypeSymbol();
                Consume(TokenTypes.RParen, "expect enclosing ) in parameter definition");
                parameters.Add(new(parameterName, parameterType));
            } while (!AtEnd() && Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param));
        }
        var body = new List<ExpressionBase>();
        if (!AdvanceIfMatch(TokenTypes.RParen))
        {
            do
            {
                body.Add(ParseExpression());
            } while (!AtEnd() && !Match(TokenTypes.RParen));
            Consume(TokenTypes.RParen, "expect enclosing ) in function body");
        }
        return new FunctionDefinition(name, returnType, parameters, body);
    }

    public ImportedFunctionDefinition ParseImportedFunctionDefinition()
    {
        /*
         * (import mscvrt cdecl (symbol `_printf`) 
         *          printf:void (param string s))
         * 
         * 
         */
        var libraryAlias = Consume(BuiltinTokenTypes.Word, "expect import library alias");
        ReclassifyToken(libraryAlias, ReclassifiedTokenTypes.ImportLibrary);
        var callingConvention = ParseCallingConvention();
        var functionName = Consume(BuiltinTokenTypes.Word, "expect function name");
        Consume(TokenTypes.Colon, "expect functionName:returnType");
        var returnType = ParseTypeSymbol();
        Token importSymbol = functionName;
        if (Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Symbol))
        {
            Advance();
            Advance();
            importSymbol = Consume(BuiltinTokenTypes.Word, "expect import symbol");
            Consume(TokenTypes.RParen, "expect enclosing ) in symbol annotation");
        }
        var parameters = new List<Parameter>();
        if (Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param))
        {
            do
            {
                Consume(TokenTypes.LParen, "expect parameter definition");
                Consume(TokenTypes.Param, "expect parameter definition");
                var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name");
                var parameterType = ParseTypeSymbol();
                Consume(TokenTypes.RParen, "expect enclosing ) in parameter definition");
                parameters.Add(new(parameterName, parameterType));
            } while (!AtEnd() && Match(TokenTypes.LParen) && PeekMatch(1, TokenTypes.Param));
        }
        Consume(TokenTypes.RParen, "expect enclosing ) after imported function definition");
        return new ImportedFunctionDefinition(functionName, returnType, parameters, callingConvention, libraryAlias, importSymbol);
    }

    public ImportLibraryDefinition ParseImportLibraryDefinition()
    {
        /*
         * (library mscvrt `msvcrt.dll`)
         * 
         */
        var libraryAlias = Consume(BuiltinTokenTypes.Word, "expect import library alias");
        ReclassifyToken(libraryAlias, ReclassifiedTokenTypes.ImportLibrary);
        var libraryPath = Consume(BuiltinTokenTypes.Word, "expect path to dll");
        Consume(TokenTypes.RParen, "expect enclosing ) after import library definition");

        return new ImportLibraryDefinition(libraryAlias, libraryPath);
    }

    private CallingConvention ParseCallingConvention()
    {
        if (!AdvanceIfMatch(TokenTypes.CallingConvention))
            throw new ParsingException(Current(), "expect calling convention");
        if (!Enum.TryParse<CallingConvention>(Previous().Lexeme, true, out var callingConvention))
            throw new ParsingException(Previous(), $"unsupported calling convention {Previous().Lexeme}");
        return callingConvention;
    }

    public ExpressionBase ParseExpression()
    {
        var startToken = Current();
        ExpressionBase expression;
        if (AdvanceIfMatch(TokenTypes.LBracket)) expression = ParseCast();
        else expression = ParseCall();
        var endToken = Previous();
        expression.StartToken = startToken;
        expression.EndToken = endToken;
        return expression;
    }

    private ExpressionBase ParseCast()
    {
        var token = Previous();
        var typeInfo = ParseTypeSymbol();
        Consume(TokenTypes.RBracket, "expect enclosing ] in cast");
        var expression = ParseExpression();
        return new CastExpression(token, typeInfo, expression);
    }
    private ExpressionBase ParseCall()
    {
        if (AdvanceIfMatch(TokenTypes.LParen))
        {
            if (AdvanceIfMatch(TokenTypes.CompilerIntrinsicGet)) return ParseCompilerIntrinsicGet();
            if (AdvanceIfMatch(TokenTypes.CompilerIntrinsicSet)) return ParseCompilerIntrinsicSet();
            if (AdvanceIfMatch(TokenTypes.Return)) return ParseReturn();
            if (AdvanceIfMatch(TokenTypes.DefineFunction)) return ParseLambdaFunction();
            if (AdvanceIfMatch(TokenTypes.Local)) return ParseLocalVariable();
            if (AdvanceIfMatch(TokenTypes.Set)) return ParseSet();
            if (Match(TokenTypes.AssemblyInstruction)) return ParseAssemblyInstruction();
            var token = Previous();
            if (AdvanceIfMatch(TokenTypes.RParen))
                throw new ParsingException(Previous(), "empty call encountered");
            var callTarget = ParseExpression();
            var arguments = new List<ExpressionBase>();
            if (!AdvanceIfMatch(TokenTypes.RParen))
            {
                do
                {
                    arguments.Add(ParseExpression());
                } while (!AtEnd() && !Match(TokenTypes.RParen));
                Consume(TokenTypes.RParen, "expect enclosing ) after call");
            }
            return new CallExpression(token, callTarget, arguments);
        }
        else return ParseGet();

    }

    private ExpressionBase ParseLocalVariable()
    {
        var token = Consume(BuiltinTokenTypes.Word, "expect local variable identifier");
        var type = ParseTypeSymbol();
        ExpressionBase? initializer = null;
        if (!AdvanceIfMatch(TokenTypes.RParen))
        {
            initializer = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in local variable definition");
        }

        return new LocalVariableExpression(token, type, token, initializer);
    }

    private ExpressionBase ParseCompilerIntrinsicGet()
    {
        var token = Previous();
        Consume(TokenTypes.Colon, "expect _ci_get:returnType");
        var returnType = ParseTypeSymbol();
        var contextPointer = ParseExpression();
        int offset = int.Parse(Consume(BuiltinTokenTypes.Integer, "expect integer offset").Lexeme);
        Consume(TokenTypes.RParen, "expect enclosing ) after call to _ci_get");
        return new CompilerIntrinsic_GetExpression(token, returnType, contextPointer, offset);
    }

    private ExpressionBase ParseCompilerIntrinsicSet()
    {
        var token = Previous();
        var contextPointer = ParseExpression();
        int offset = int.Parse(Consume(BuiltinTokenTypes.Integer, "expect integer offset to memory location").Lexeme);
        var valueToAssign = ParseExpression();
        Consume(TokenTypes.RParen, "expect enclosing ) after call to _ci_set");
        return new CompilerIntrinsic_SetExpression(token, contextPointer, offset, valueToAssign);
    }

    private ExpressionBase ParseReturn()
    {
        var token = Previous();
        ExpressionBase? returnValue = null;
        if (!AdvanceIfMatch(TokenTypes.RParen))
        {
            returnValue = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) after return statement");
        }
        return new ReturnExpression(token, returnValue);
    }

    private ExpressionBase ParseLambdaFunction()
    {
        var startToken = Current();
        var functionDefinition = ParseLambdaFunctionDefinition();
        var endToken = Previous();
        functionDefinition.StartToken = startToken;
        functionDefinition.EndToken = endToken;
        return new LambdaExpression(startToken, functionDefinition);
    }

    private ExpressionBase ParseSet()
    {
        var assignmentTarget = ParseExpression();
        var valueToAssign = ParseExpression();
        Consume(TokenTypes.RParen, "expect enclosing ) after set statement");
        return new SetExpression(Previous(), assignmentTarget, valueToAssign);
    }

    private ExpressionBase ParseGet()
    {
        var start = Current();
        var expr = ParsePrimary();
        var end = Previous();
        expr.StartToken = start;
        expr.EndToken = end;
        if (expr is IdentifierExpression identifierExpression && AdvanceIfMatch(TokenTypes.LBracket))
        {
            var typeArguments = new List<TypeSymbol>();
            if (!AdvanceIfMatch(TokenTypes.RBracket))
            {
                do
                {
                    typeArguments.Add(ParseTypeSymbol());
                } while (AdvanceIfMatch(TokenTypes.Comma));
                Consume(TokenTypes.RBracket, "expect enclosing ] after type arguments list");
            }
            // return here, no chaining allowed
            return new GenericFunctionReferenceExpression(identifierExpression.Token, typeArguments);
        }

        if (expr is IdentifierExpression || expr is GetExpression)
        {
            while (Match(TokenTypes.Dot) || Match(TokenTypes.NullDot))
            {
                if (AdvanceIfMatch(TokenTypes.Dot))
                {
                    var targetField = Consume(BuiltinTokenTypes.Word, "expect member name after '.'");
                    expr = new GetExpression(Previous(), expr, targetField, false);
                    expr.StartToken = start;
                    expr.EndToken = targetField;
                }
                else
                {
                    Advance();
                    var targetField = Consume(BuiltinTokenTypes.Word, "expect member name after '?.'");
                    expr = new GetExpression(Previous(), expr, targetField, true);
                    expr.StartToken = start;
                    expr.EndToken = targetField;
                }
            }
        }

        return expr;
    }

    private ExpressionBase ParsePrimary()
    {
        if (AdvanceIfMatch(BuiltinTokenTypes.Word)) return new IdentifierExpression(Previous());
        return ParseLiteral();
    }

    public LiteralExpression ParseLiteral()
    {
        if (AdvanceIfMatch(BuiltinTokenTypes.Integer))
        {
            return new LiteralExpression(Previous(), int.Parse(Previous().Lexeme));
        }
        if (AdvanceIfMatch(BuiltinTokenTypes.Float))
        {
            return new LiteralExpression(Previous(), float.Parse(Previous().Lexeme));
        }
        if (AdvanceIfMatch(BuiltinTokenTypes.String))
        {
            return new LiteralExpression(Previous(), Previous().Lexeme);
        }
        if (AdvanceIfMatch(TokenTypes.True))
        {
            return new LiteralExpression(Previous(), 1);
        }
        if (AdvanceIfMatch(TokenTypes.False))
        {
            return new LiteralExpression(Previous(), 0);
        }
        if (AdvanceIfMatch(TokenTypes.Null))
        {
            return new LiteralExpression(Previous(), null);
        }
        throw new ParsingException(Current(), $"encountered unexpected token {Current()}");
    }

    private ExpressionBase ParseAssemblyInstruction()
    {
        foreach (var assemblyParsingRule in _assemblyParsingRules)
        {
            if (assemblyParsingRule.CanParse(this))
            {
                var token = Previous();
                var instruction = assemblyParsingRule.Parse(this);
                Consume(TokenTypes.RParen, "expect enclosing ) after assembly instruction");
                return new InlineAssemblyExpression(token, instruction);
            }
        }

        throw new ParsingException(Previous(), "expect inline assembly instruction");
    }

    public bool CanParse(AssemblyInstructions instruction, ref int tokenOffset)
    {
        if (Match(TokenTypes.AssemblyInstruction))
        {
            if (!Enum.TryParse<AssemblyInstructions>(Current().Lexeme, true, out var assemblyInstruction))
                return false;
            tokenOffset += 1;
            return true;
        }
        return false;
    }

    public bool CanParse(InstructionParseUnit unit, ref int tokenOffset)
    {
        if (unit == InstructionParseUnit.Symbol) return PeekMatch(tokenOffset, BuiltinTokenTypes.Word);
        if (unit == InstructionParseUnit.Immediate) return PeekMatch(tokenOffset, BuiltinTokenTypes.Integer);
        if (unit == InstructionParseUnit.GeneralRegister32) return PeekMatch(tokenOffset, TokenTypes.GeneralRegister32);
        if (unit == InstructionParseUnit.XmmRegister) return PeekMatch(tokenOffset, TokenTypes.XmmRegister);
        if (unit == InstructionParseUnit.RegisterOffset) return CanParseRegisterOffset(ref tokenOffset);
        if (unit == InstructionParseUnit.SymbolOffset) return CanParseSymbolOffset(ref tokenOffset);
        if (unit == InstructionParseUnit.ByteRegister) return PeekMatch(tokenOffset, TokenTypes.ByteRegister);
        if (unit == InstructionParseUnit.SymbolOffset_Byte) return CanParseSymbolOffset_Byte(ref tokenOffset);
        if (unit == InstructionParseUnit.RegisterOffset_Byte) return CanParseRegisterOffset_Byte(ref tokenOffset);
        return false;
    }

    private bool CanParseRegisterOffset(ref int tokenOffset)
    {
        if (PeekMatch(tokenOffset, TokenTypes.RBracket) && PeekMatch(tokenOffset + 1, TokenTypes.GeneralRegister32) && (PeekMatch(tokenOffset + 2, TokenTypes.Plus) || PeekMatch(tokenOffset + 2, TokenTypes.Minus)) && PeekMatch(tokenOffset + 3, BuiltinTokenTypes.Integer) && PeekMatch(tokenOffset + 4, TokenTypes.RBracket))
        {
            tokenOffset += 5;
            return true;
        }
        if (PeekMatch(tokenOffset, TokenTypes.RBracket) && PeekMatch(tokenOffset + 1, TokenTypes.GeneralRegister32) && PeekMatch(tokenOffset + 2, TokenTypes.RBracket))
        {
            tokenOffset += 3;
            return true;
        }
        return false;
    }

    private bool CanParseRegisterOffset_Byte(ref int tokenOffset)
    {
        if (PeekMatch(tokenOffset, TokenTypes.Byte) && PeekMatch(tokenOffset + 1, TokenTypes.RBracket) && PeekMatch(tokenOffset + 2, TokenTypes.GeneralRegister32) && (PeekMatch(tokenOffset + 3, TokenTypes.Plus) || PeekMatch(tokenOffset + 3, TokenTypes.Minus)) && PeekMatch(tokenOffset + 4, BuiltinTokenTypes.Integer) && PeekMatch(tokenOffset + 5, TokenTypes.RBracket))
        {
            tokenOffset += 6;
            return true;
        }
        if (PeekMatch(tokenOffset, TokenTypes.Byte) && PeekMatch(tokenOffset + 1, TokenTypes.RBracket) && PeekMatch(tokenOffset + 2, TokenTypes.GeneralRegister32) && PeekMatch(tokenOffset + 3, TokenTypes.RBracket))
        {
            tokenOffset += 4;
            return true;
        }
        return false;
    }

    private bool CanParseSymbolOffset(ref int tokenOffset)
    {
        if (PeekMatch(tokenOffset, TokenTypes.RBracket) && PeekMatch(tokenOffset + 1, BuiltinTokenTypes.Word) && (PeekMatch(tokenOffset + 2, TokenTypes.Plus) || PeekMatch(tokenOffset + 2, TokenTypes.Minus)) && PeekMatch(tokenOffset + 3, BuiltinTokenTypes.Integer) && PeekMatch(tokenOffset + 4, TokenTypes.RBracket))
        {
            tokenOffset += 5;
            return true;
        }
        if (PeekMatch(tokenOffset, TokenTypes.RBracket) && PeekMatch(tokenOffset + 1, BuiltinTokenTypes.Word) && PeekMatch(tokenOffset + 2, TokenTypes.RBracket))
        {
            tokenOffset += 3;
            return true;
        }
        return false;
    }

    private bool CanParseSymbolOffset_Byte(ref int tokenOffset)
    {
        if (PeekMatch(tokenOffset, TokenTypes.Byte) && PeekMatch(tokenOffset + 1, TokenTypes.RBracket) && PeekMatch(tokenOffset + 2, BuiltinTokenTypes.Word) && (PeekMatch(tokenOffset + 3, TokenTypes.Plus) || PeekMatch(tokenOffset + 3, TokenTypes.Minus)) && PeekMatch(tokenOffset + 4, BuiltinTokenTypes.Integer) && PeekMatch(tokenOffset + 5, TokenTypes.RBracket))
        {
            tokenOffset += 6;
            return true;
        }
        if (PeekMatch(tokenOffset, TokenTypes.Byte) && PeekMatch(tokenOffset + 1, TokenTypes.RBracket) && PeekMatch(tokenOffset + 2, BuiltinTokenTypes.Word) && PeekMatch(tokenOffset + 3, TokenTypes.RBracket))
        {
            tokenOffset += 4;
            return true;
        }
        return false;
    }

    private RegisterOffset ParseRegisterOffset()
    {
        Consume(TokenTypes.LBracket, "expect register offset");
        var register = ParseGeneralRegister32();
        if (AdvanceIfMatch(TokenTypes.Plus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in register offset");
            return new RegisterOffset(register, offset);
        }
        else if (AdvanceIfMatch(TokenTypes.Minus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in register offset");
            return new RegisterOffset(register, -offset);
        }
        else if (AdvanceIfMatch(TokenTypes.RBracket)) return new RegisterOffset(register, 0);
        else throw new ParsingException(Previous(), "expect register offset");
    }

    private SymbolOffset ParseSymbolOffset()
    {
        Consume(TokenTypes.LBracket, "expect symbol offset");
        var symbol = Consume(BuiltinTokenTypes.Word, "expect symbol offset").Lexeme;
        if (AdvanceIfMatch(TokenTypes.Plus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in symbol offset");
            return new SymbolOffset(symbol, offset);
        }
        else if (AdvanceIfMatch(TokenTypes.Minus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in symbol offset");
            return new SymbolOffset(symbol, -offset);
        }
        else if (AdvanceIfMatch(TokenTypes.RBracket)) return new SymbolOffset(symbol, 0);
        else throw new ParsingException(Previous(), "expect symbol offset");
    }

    private SymbolOffset ParseSymbolOffset_Byte()
    {
        Consume(TokenTypes.Byte, "expect byte offset");
        Consume(TokenTypes.LBracket, "expect symbol offset");
        var symbol = Consume(BuiltinTokenTypes.Word, "expect symbol offset").Lexeme;
        if (AdvanceIfMatch(TokenTypes.Plus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in symbol offset");
            return new SymbolOffset(symbol, offset);
        }
        else if (AdvanceIfMatch(TokenTypes.Minus))
        {
            Consume(BuiltinTokenTypes.Integer, "expect integer offset");
            var offset = int.Parse(Previous().Lexeme);
            Consume(TokenTypes.LBracket, "expect enclosing ] in symbol offset");
            return new SymbolOffset(symbol, -offset);
        }
        else if (AdvanceIfMatch(TokenTypes.RBracket)) return new SymbolOffset(symbol, 0);
        else throw new ParsingException(Previous(), "expect symbol offset");
    }

    private X86Register ParseGeneralRegister32()
    {
        if (!AdvanceIfMatch(TokenTypes.GeneralRegister32))
            throw new ParsingException(Previous(), $"expect general register");
        if (!Enum.TryParse<X86Register>(Previous().Lexeme, true, out var register))
            throw new ParsingException(Previous(), $"unsupported general register {Previous().Lexeme}");
        return register;
    }

    private XmmRegister ParseXmmRegister()
    {
        if (!AdvanceIfMatch(TokenTypes.XmmRegister))
            throw new ParsingException(Previous(), $"expect xmm register");
        if (!Enum.TryParse<XmmRegister>(Previous().Lexeme, true, out var register))
            throw new ParsingException(Previous(), $"unsupported xmm register {Previous().Lexeme}");
        return register;
    }

    private X86ByteRegister ParseByteRegister()
    {
        if (!AdvanceIfMatch(TokenTypes.ByteRegister))
            throw new ParsingException(Previous(), $"expect xmm register");
        if (!Enum.TryParse<X86ByteRegister>(Previous().Lexeme, true, out var register))
            throw new ParsingException(Previous(), $"unsupported byte register {Previous().Lexeme}");
        return register;
    }

    public AssemblyInstructions Consume(AssemblyInstructions instruction)
    {
        Consume(TokenTypes.AssemblyInstruction, $"expect {instruction} instruction");
        if (!Enum.TryParse<AssemblyInstructions>(Previous().Lexeme, true, out var assemblyInstruction))
            throw new ParsingException(Previous(), $"unsupported assembly instruction {Previous().Lexeme}");
        if (assemblyInstruction != instruction)
            throw new ParsingException(Previous(), $"expected {instruction} instruction but got {assemblyInstruction}");
        return assemblyInstruction;
    }

    public int ParseImmediate()
    {
        Consume(BuiltinTokenTypes.Integer, "expect integer offset");
        return int.Parse(Previous().Lexeme);
    }

    public string ParseSymbol()
    {
        return Consume(BuiltinTokenTypes.Word, "expect symbol").Lexeme;
    }

    protected Token ReclassifyToken(Token token, string newClassification)
    {
        token.Type = newClassification;
        return token;
    }
}