using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.BaseStatements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokenizer.Core.Models;

namespace Leaf.Language.Api.Models;
public class GenericFunctionContext
{
    public GenericFunctionDefinition FunctionDefinition { get; set; }
    public ProgramContext ProgramContext { get; set; }
    public GenericFunctionContext(GenericFunctionDefinition functionDefinition, ProgramContext programContext)
    {
        FunctionDefinition = functionDefinition;
        ProgramContext = programContext;
    }

    public Location Start => FunctionDefinition.StartToken.Start;
    public Location End => FunctionDefinition.EndToken.End;

    public bool Contains(int line, int column)
    {
        if (line == Start.Line) return Start.Column <= column;
        if (line == End.Line) return End.Column >= column;
        return Start.Line <= line && End.Line >= line;
    }

    public Parameter? GetParameter(Token paramterName) => FunctionDefinition.Parameters.Find(x => x.Name.Lexeme == paramterName.Lexeme);
    public LocalVariableExpression? GetLocalVariableExpression(Token localVariableName) => FunctionDefinition.ExtractLocalVariableExpressions().FirstOrDefault(x => x.Identifier.Lexeme == localVariableName.Lexeme);

}