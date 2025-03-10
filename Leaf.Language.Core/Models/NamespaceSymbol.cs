using Tokenizer.Core.Models;

namespace Leaf.Language.Core.Models;

public class NamespaceSymbol
{
    public List<Token> EnclosingNamespacePath { get; set; } = new();
    public Token Name { get; set; }

    public NamespaceSymbol(Token name)
    {
        EnclosingNamespacePath = new();
        Name = name;
    }

    public NamespaceSymbol(List<Token> enclosingNamespacePath, Token name)
    {
        EnclosingNamespacePath = enclosingNamespacePath;
        Name = name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is NamespaceSymbol ns)
        {
            if (EnclosingNamespacePath.Count != ns.EnclosingNamespacePath.Count) return false;
            for(int i = 0; i < EnclosingNamespacePath.Count; i++)
            {
                if (!EnclosingNamespacePath[i].Equals(ns.EnclosingNamespacePath[i])) return false;
            }
            return Name.Equals(ns.Name);
        }
        return false;
    }

    public override string ToString()
    {
        if (EnclosingNamespacePath.Count == 0) return Name.Lexeme;
        return $"{string.Join(".", EnclosingNamespacePath.Select(x => x.Lexeme))}.{Name.Lexeme}";
    }
}