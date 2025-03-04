using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class StructFieldInfo
{
    public TypeInfo TypeInfo { get; set; }
    public Token Name { get; set; }

    public StructFieldInfo(TypeInfo typeInfo, Token name)
    {
        TypeInfo = typeInfo;
        Name = name;
    }
}