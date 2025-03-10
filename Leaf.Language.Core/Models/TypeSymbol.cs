﻿using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class TypeSymbol
{
    public NamespaceSymbol? Namespace { get; set; }
    public Token TypeName { get; set; }
    public List<TypeSymbol> TypeArguments { get; set; }
    public TypeSymbol(NamespaceSymbol? @namespace, Token typeName, List<TypeSymbol> typeArguments)
    {
        Namespace = @namespace;
        TypeName = typeName;
        TypeArguments = typeArguments;
    }

    public virtual bool IsGenericTypeSymbol => false;
    public virtual bool ContainsGenericTypeSymbol => TypeArguments.Any(x => x.ContainsGenericTypeSymbol || x.IsGenericTypeSymbol);

    public override int GetHashCode()
    {
        return TypeName.Lexeme.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is TypeSymbol typeSymbol)
        {
            if (typeSymbol.IsGenericTypeSymbol) return false;
            if (Namespace == null && typeSymbol.Namespace != null) return false;
            if (Namespace != null && typeSymbol.Namespace == null) return false;
            if (Namespace?.Equals(typeSymbol.Namespace) == false) return false;
            if (typeSymbol.TypeArguments.Count != TypeArguments.Count) return false;
            if (TypeName.Lexeme != typeSymbol.TypeName.Lexeme) return false;
            for (int i = 0; i < TypeArguments.Count; i++)
            {
                if (!TypeArguments[i].Equals(typeSymbol.TypeArguments[i])) return false;
            }
            return true;
        }
        return false;
    }

    public virtual TypeSymbol ReplaceGenericTypeParameter(Dictionary<GenericTypeSymbol, TypeSymbol> genericToConcreteTypeMap)
    {
        if (!ContainsGenericTypeSymbol)
            return new TypeSymbol(Namespace, TypeName, TypeArguments);
        return new TypeSymbol(Namespace, TypeName, TypeArguments.Select(x => x.ReplaceGenericTypeParameter(genericToConcreteTypeMap)).ToList());
    }

    public string GetFlattenedName()
    {
        if (TypeArguments.Any())
            return $"{TypeName.Lexeme}?{string.Join("_", TypeArguments.Select(x => x.GetFlattenedName()))}?";
        return $"{TypeName.Lexeme}";
    }

    public override string ToString()
    {
        if (TypeArguments.Any())
            return $"{TypeName.Lexeme}[{string.Join(", ", TypeArguments.Select(x => x.GetFlattenedName()))}]";
        return $"{TypeName.Lexeme}";
    }

}