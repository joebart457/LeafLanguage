using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using Leaf.Language.Core.Statements.TypedStatements;
using System.Diagnostics.CodeAnalysis;
using Tokenizer.Core.Exceptions;
using Tokenizer.Core.Models;

namespace Leaf.Language.Api.Models;

public class ProgramContext
{
    private class TokenEqualityComparer : EqualityComparer<Token>
    {
        public override bool Equals(Token? x, Token? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.Lexeme == y.Lexeme && x.Type == y.Type;
        }

        public override int GetHashCode([DisallowNull] Token obj)
        {
            return obj.Lexeme.GetHashCode();
        }
    }
    public List<TypedFunctionDefinition> FunctionDefinitions { get; set; } = new();
    public List<TypedImportedFunctionDefinition> ImportedFunctionDefinitions { get; set; } = new();
    public List<TypedImportLibraryDefinition> ImportLibraryDefinitions { get; set; } = new();
    public List<GenericTypeDefinition> GenericTypeDefinitions { get; set; } = new();
    public List<GenericFunctionDefinition> GenericFunctionDefinitions { get; set; } = new();
    public List<StructTypeInfo> UserDefinedTypes { get; set; } = new();
    public List<(Token, string)> ValidationErrors { get; set; } = new();
    public List<Token> Tokens { get; set; } = new();
    public Dictionary<Token, List<Token>> References { get; set; } = new(new TokenEqualityComparer());
    public FunctionContext? GetFunctionContext(int line, int column)
    {
        FunctionContext? match = null;
        foreach (var function in FunctionDefinitions)
        {
            var functionContext = new FunctionContext(function, this);
            if (functionContext.Contains(line, column))
            {
                if (match == null) match = functionContext;
                // search for inner most matching function
                else if (match.Contains(functionContext.Start.Line, functionContext.Start.Column)) match = functionContext;
            }
        }
        return match;
    }

    public GenericFunctionContext? GetGenericFunctionContext(int line, int column)
    {
        GenericFunctionContext? match = null;
        foreach (var function in GenericFunctionDefinitions)
        {
            var functionContext = new GenericFunctionContext(function, this);
            if (functionContext.Contains(line, column))
            {
                if (match == null) match = functionContext;
                // search for inner most matching function
                else if (match.Contains(functionContext.Start.Line, functionContext.Start.Column)) match = functionContext;
            }
        }
        return match;
    }

    public void AddValidationError(ParsingException parsingException)
    {
        ValidationErrors.Add((parsingException.Token, parsingException.Message));
    }

    public Token? GetTokenAt(int line, int column)
    {
        foreach (var token in Tokens)
        {
            if (ContainsToken(token, line, column)) return token;
        }
        return null;
    }

    public (int index, Token? token) GetTokenAndIndexAt(int line, int column)
    {
        int index = 0;
        foreach (var token in Tokens)
        {
            if (ContainsToken(token, line, column)) return (index, token);
            index++;
        }
        return (-1, null);
    }

    public Token? GetPreviousToken(int index)
    {
        index--;
        if (index < 0 || index >= Tokens.Count) return null;
        return Tokens[index];
    }

    public bool ContainsToken(Token token, int line, int column)
    {
        if (line == token.Start.Line && line == token.End.Line) return token.Start.Column <= column && column <= token.End.Column;
        if (line == token.Start.Line) return token.Start.Column <= column;
        if (line == token.End.Line) return token.End.Column >= column;
        return token.Start.Line <= line && token.End.Line >= line;
    }
}