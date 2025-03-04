

using Assembler.Core.Constants;

namespace Leaf.Language.Compiler.Models;

public class CompilationOptions
{
    public string InputPath { get; set; } = "";
    public string EntryPoint { get; set; } = "Main";
    public string AssemblyPath { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public OutputTarget OutputTarget { get; set; } = OutputTarget.Exe;
    public bool EnableOptimizations { get; set; } = false;
    public int OptimizationPasses { get; set; } = 3;
    public bool SourceComments { get; set; } = false;
    public bool LogSuccess { get; set; } = false;

}