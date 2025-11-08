using Microsoft.CodeAnalysis;

namespace spennyIRC.Scripting.Engine;

public sealed class CSharpCompilationException(IEnumerable<Diagnostic> diagnostics) : Exception("Compilation failed")
{
    public IEnumerable<Diagnostic> Diagnostics { get; } = diagnostics;
}