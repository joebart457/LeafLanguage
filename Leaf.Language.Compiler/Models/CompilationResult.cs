using Assembler.Core;
using Assembler.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assembler.Core.X86AssemblyContext;

namespace Leaf.Language.Compiler.Models;

public class CompilationResult
{
    public CompilationOptions CompilationOptions { get; private set; }
    public List<X86Function> FunctionData { get; private set; }
    public List<ImportLibrary> ImportLibraries { get; private set; }
    public List<(string functionIdentifier, string exportedSymbol)> ExportedFunctions { get; private set; }
    public List<StringData> StaticStringData { get; private set; }
    public List<SinglePrecisionFloatingPointData> StaticFloatingPointData { get; private set; }
    public List<IntegerData> StaticIntegerData { get; private set; }
    public List<ByteData> StaticByteData { get; private set; }
    public List<PointerData> StaticPointerData { get; private set; }
    public List<UnitializedData> StaticUnitializedData { get; private set; }
    public CompilationResult(X86AssemblyContext context, CompilationOptions options)
    {
        CompilationOptions = options;
        FunctionData = context.FunctionData;
        ImportLibraries = context.ImportLibraries;
        ExportedFunctions = context.ExportedFunctions;
        StaticStringData = context.StaticStringData;
        StaticFloatingPointData = context.StaticFloatingPointData;
        StaticIntegerData = context.StaticIntegerData;
        StaticByteData = context.StaticByteData;
        StaticPointerData = context.StaticPointerData;
        StaticUnitializedData = context.StaticUnitializedData;

    }
}