﻿
using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class GenericTypeSymbol : TypeSymbol
{
    public GenericTypeSymbol(Token typeName) : base(typeName, new())
    {
    }

    public override bool IsGenericTypeSymbol => true;


    public override int GetHashCode()
    {
        return TypeName.Lexeme.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is GenericTypeSymbol typeSymbol)
        {
            return typeSymbol.IsGenericTypeSymbol && TypeName.Lexeme == typeSymbol.TypeName.Lexeme;
        }
        return false;
    }

    public override TypeSymbol ReplaceGenericTypeParameter(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        if (genericToConcreteTypeMap.TryGetValue(this, out var concreteTypeSymbol))
            return concreteTypeSymbol;
        return this;
    }
}