
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class TypeDefinitionField
{
    public TypeSymbol TypeSymbol { get; set; }
    public Token Name { get; set; }
    public TypeDefinitionField(TypeSymbol typeSymbol, Token name)
    {
        TypeSymbol = typeSymbol;
        Name = name;
    }

}