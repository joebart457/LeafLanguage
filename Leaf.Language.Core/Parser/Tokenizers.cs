using Leaf.Language.Core.Constants;
using System.Runtime.InteropServices;
using Tokenizer.Core.Constants;
using Tokenizer.Core.Models;
using Tokenizer.Core;
using Assembler.Core.Constants;

namespace Leaf.Language.Core.Parser;

public static class Tokenizers
{
    public static TextTokenizer Default => new TextTokenizer(_defaultRules, DefaultSettings);

    public static TextTokenizer CreateFromDefault(List<TokenizerRule> rules, TokenizerSettings? tokenizerSettings = null)
    {
        var defaultRulesCopy = _defaultRules.Select(x => x).ToList();
        defaultRulesCopy.AddRange(rules);
        return new TextTokenizer(defaultRulesCopy, tokenizerSettings ?? DefaultSettings);
    }

    public static TokenizerSettings DefaultSettings => new TokenizerSettings
    {
        AllowNegatives = true,
        NegativeChar = '-',
        NewlinesAsTokens = false,
        WordStarters = "_",
        WordIncluded = "_",
        IgnoreCase = false,
        TabSize = 1,
        CommentsAsTokens = false,
    };
    private static List<TokenizerRule> _defaultRules => new List<TokenizerRule>()
        {
                    new TokenizerRule(TokenTypes.LParen, "("),
                    new TokenizerRule(TokenTypes.RParen, ")"),
                    new TokenizerRule(TokenTypes.LBracket, "["),
                    new TokenizerRule(TokenTypes.RBracket, "]"),
                    new TokenizerRule(TokenTypes.Dot, "."),
                    new TokenizerRule(TokenTypes.Colon, ":"),
                    new TokenizerRule(TokenTypes.Comma, ","),

                    new TokenizerRule(TokenTypes.IntrinsicType, "string"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "func"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "cfunc"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "float"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "int"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "ptr"),
                    new TokenizerRule(TokenTypes.IntrinsicType, "void"),

                    new TokenizerRule(TokenTypes.CallingConvention, CallingConvention.Cdecl.ToString(), ignoreCase: true),
                    new TokenizerRule(TokenTypes.CallingConvention, CallingConvention.StdCall.ToString(), ignoreCase: true),

                    new TokenizerRule(TokenTypes.DefineFunction, "defn"),
                    new TokenizerRule(TokenTypes.Import, "import"),
                    new TokenizerRule(TokenTypes.Library, "library"),
                    new TokenizerRule(TokenTypes.Symbol, "symbol"),
                    new TokenizerRule(TokenTypes.Param, "param"),
                    new TokenizerRule(TokenTypes.Return, "return"),
                    new TokenizerRule(TokenTypes.Set, "set"),
                    new TokenizerRule(TokenTypes.Gen, "gen"),

                    new TokenizerRule(TokenTypes.Type, "type"),
                    new TokenizerRule(TokenTypes.Field, "field"),

                    new TokenizerRule(TokenTypes.True, "true"),
                    new TokenizerRule(TokenTypes.False, "false"),
                    new TokenizerRule(TokenTypes.Null, "null"),
                    new TokenizerRule(TokenTypes.Local, "local"),
                    new TokenizerRule(TokenTypes.Export, "export"),

                    new TokenizerRule(TokenTypes.InlineAssembly, "__asm {", enclosingLeft: "__asm {", enclosingRight: "}", ignoreCase: true),
                    new TokenizerRule(TokenTypes.CompilerIntrinsicGet, "_ci_get"),
                    new TokenizerRule(TokenTypes.CompilerIntrinsicSet, "_ci_set"),

                    new TokenizerRule(TokenTypes.Namespace, "namespace"),               

                    new TokenizerRule(BuiltinTokenTypes.EndOfLineComment, "//"),
                    new TokenizerRule(BuiltinTokenTypes.MultiLineComment, "/*", enclosingLeft: "/*", enclosingRight: "*/"),
                    new TokenizerRule(BuiltinTokenTypes.String, "\"", enclosingLeft: "\"", enclosingRight: "\""),
                    new TokenizerRule(BuiltinTokenTypes.String, "'", enclosingLeft: "'", enclosingRight: "'"),
                    new TokenizerRule(BuiltinTokenTypes.Word, "`", enclosingLeft: "`", enclosingRight: "`"),
        }.AddAsssemblyInstructionParsingRules();


    private static List<TokenizerRule> AddAsssemblyInstructionParsingRules(this List<TokenizerRule> rules)
    {
        rules.AddRange(GetAsssemblyInstructionParsingRules);
        rules.Add(new TokenizerRule(TokenTypes.ByteRegister, X86ByteRegister.al.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.ByteRegister, X86ByteRegister.bl.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.ByteRegister, X86ByteRegister.ah.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.ByteRegister, X86ByteRegister.bh.ToString()));

        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.eax.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.ebx.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.ecx.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.edx.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.esi.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.edi.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.esp.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.GeneralRegister32, X86Register.ebp.ToString())); 
        
        rules.Add(new TokenizerRule(TokenTypes.XmmRegister, XmmRegister.xmm0.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.XmmRegister, XmmRegister.xmm1.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.XmmRegister, XmmRegister.xmm2.ToString()));
        rules.Add(new TokenizerRule(TokenTypes.XmmRegister, XmmRegister.xmm3.ToString()));
        return rules;
    }

    private static IEnumerable<TokenizerRule> GetAsssemblyInstructionParsingRules => Enum.GetNames<AssemblyInstructions>().Select(x => new TokenizerRule(TokenTypes.AssemblyInstruction, $"_{x.ToLower()}"));

}
