using Leaf.Language.TypedExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;
using Tokenizer.Core.Models;

namespace Leaf.Language.Api.Models;


public class FunctionContext
{
    public TypedFunctionDefinition FunctionDefinition { get; set; }
    public ProgramContext ProgramContext { get; set; }
    public FunctionContext(TypedFunctionDefinition functionDefinition, ProgramContext programContext)
    {
        FunctionDefinition = functionDefinition;
        ProgramContext = programContext;
    }

    public Location Start => FunctionDefinition.OriginalStatement.StartToken.Start;
    public Location End => FunctionDefinition.OriginalStatement.EndToken.End;

    public bool Contains(int line, int column)
    {
        if (line == Start.Line) return Start.Column <= column;
        if (line == End.Line) return End.Column >= column;
        return Start.Line <= line && End.Line >= line;
    }

    public ExpressionContext? GetExpressionContext(int line, int column)
    {
        if (!Contains(line, column)) return null;
        foreach (var expression in FunctionDefinition.BodyStatements)
        {
            var expressionContext = new ExpressionContext(expression, this);
            if (expressionContext.TryGetContainingExpression(line, column, out var containingExpression) && containingExpression != null) return new ExpressionContext(containingExpression, this);
        }
        return null;
    }

    public TypedParameter? GetParameter(Token paramterName) => FunctionDefinition.Parameters.Find(x => x.Name.Lexeme == paramterName.Lexeme);
    public TypedLocalVariableExpression? GetLocalVariableExpression(Token localVariableName) => FunctionDefinition.ExtractLocalVariableExpressions().FirstOrDefault(x => x.Identifier.Lexeme == localVariableName.Lexeme);

}