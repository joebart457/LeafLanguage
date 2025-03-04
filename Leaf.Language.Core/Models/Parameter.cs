
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class Parameter
{
    public Token Name { get; set; }
    public TypeSymbol TypeSymbol { get; set; }
    public Parameter(Token name, TypeSymbol typeSymbol)
    {
        Name = name;
        TypeSymbol = typeSymbol;
    }
}