
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class TypedParameter
{
    public Token Name { get; set; }
    public TypeInfo TypeInfo { get; set; }
    public TypedParameter(Token name, TypeInfo typeInfo)
    {
        Name = name;
        TypeInfo = typeInfo;
    }
}