using Leaf.Language.Core.Models;
using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;

using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Statements.BaseStatements;

public class TypeDefinition : StatementBase
{
    public Token TypeName { get; set; }
    public List<TypeDefinitionField> Fields { get; set; }
    public TypeDefinition(Token typeName, List<TypeDefinitionField> fields) : base(typeName)
    {
        TypeName = typeName;
        Fields = fields;
    }

    public override void GatherSignature(TypeResolver typeResolver)
    {
        typeResolver.GatherSignature(this);
    }

    public override TypedStatement Resolve(TypeResolver typeResolver)
    {
        return typeResolver.Resolve(this);
    }
}
