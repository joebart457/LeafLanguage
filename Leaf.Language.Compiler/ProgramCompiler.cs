using Assembler.Core;
using Assembler.Core.Constants;
using Leaf.Language.Compiler.Models;
using Leaf.Language.Compiler.Optimizer;
using Leaf.Language.Core.Parser;
using Leaf.Language.Core.StaticAnalysis;
using Logger;

namespace Leaf.Language.Compiler;


public class X86ProgramCompiler
{
    private readonly ProgramParser _parser = new();
    private readonly TypeResolver _typeResolver = new();
    public string? EmitBinary(CompilationOptions options)
    {
        var result = Compile(options);
        var error = result.OutputToPeFile(out var peFile);
        if (error != null) return error;
        if (!string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            File.WriteAllText(options.AssemblyPath, peFile.OutputAsText());
            if (options.LogSuccess) CliLogger.LogSuccess($"{options.InputPath} -> {options.AssemblyPath}");
        }
        
        try
        {
            var assembledBytes = peFile.AssembleProgram(null);
            File.WriteAllBytes(options.OutputPath, assembledBytes);
            if (options.LogSuccess) CliLogger.LogSuccess($"{options.InputPath} -> {options.OutputPath}");
            return null;
        } catch(Exception ex)
        {
            return ex.Message;
        }
    }

    public X86AssemblyContext Compile(CompilationOptions options)
    {
        var parserResult = _parser.ParseFile(options.InputPath, out var errors);
        if (errors.Any()) throw new AggregateException(errors);
        var resolverResult = _typeResolver.Resolve(parserResult);
        return Compile(resolverResult, options);
    }

    public X86AssemblyContext Compile(TypeResolverResult resolverResult, CompilationOptions options)
    {
        var context = new X86AssemblyContext();
        context.SetOutputTarget(options.OutputTarget);
        if (options.OutputTarget == OutputTarget.Dll) 
            context.SetExportFileName(Path.GetFileName(options.OutputPath));
        
        resolverResult.ImportLibraries.ForEach(x => x.Compile(context));
        resolverResult.ImportedFunctions.ForEach(x => x.Compile(context));
        resolverResult.Functions.ForEach(x => x.Compile(context));
        resolverResult.ProgramIcon?.Compile(context);
        
        if (options.EnableOptimizations)
        {
            for (int i = 0; i < options.OptimizationPasses; i++)
                X86AssemblyOptimizer.Optimize(context);
        }
        return context;
    }
}