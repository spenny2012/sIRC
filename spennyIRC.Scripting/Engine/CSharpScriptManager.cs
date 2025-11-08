using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using spennyIRC.Core.IRC;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace spennyIRC.Scripting.Engine;

/// <summary>
/// High-performance script manager optimized for speed and low memory usage
/// </summary>
public sealed class CSharpScriptManager : ICSharpScriptManager
{
    private const int MaxScriptSize = 1024 * 1024;
    private readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;
    private readonly string _cacheDirectory;
    private readonly ReaderWriterLockSlim _cacheLock = new();
    private readonly CSharpCompilationOptions _compilationOptions;
    private readonly ConcurrentDictionary<uint, CompiledScript> _compiledScripts = new();
    private readonly CSharpParseOptions _parseOptions;
    private readonly MetadataReference[] _references;

    public CSharpScriptManager(string? cacheDirectory = null)
    {
        _cacheDirectory = cacheDirectory ?? Path.Combine(Path.GetTempPath(), "IRCScriptCache");
        Directory.CreateDirectory(_cacheDirectory);

        // Pre-create all references once
        _references = CreateReferences();

        // Optimized compilation options
        _compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            platform: Platform.AnyCpu,
            allowUnsafe: false,
            deterministic: true, // Enables caching
            nullableContextOptions: NullableContextOptions.Disable,
            specificDiagnosticOptions: GetDiagnosticOptions()
        );

        _parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest)
            .WithKind(SourceCodeKind.Regular);

        // Pre-warm JIT
        WarmUp();
    }

    /// <summary>
    /// Clears cache from the script cache directory
    /// </summary>
    public void ClearCache()
    {
        _cacheLock.EnterWriteLock();
        try
        {
            foreach (CompiledScript script in _compiledScripts.Values)
            {
                script.MappedFile?.Dispose();
            }
            _compiledScripts.Clear();

            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, recursive: true);
                Directory.CreateDirectory(_cacheDirectory);
            }
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        foreach (CompiledScript script in _compiledScripts.Values)
        {
            script.MappedFile?.Dispose();
        }
        _cacheLock?.Dispose();
    }

    /// <summary>
    /// Loads and executes a script with maximum performance
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? ExecuteScript<T>(string scriptPath) where T : class
    {
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script not found: {scriptPath}");

        uint hash = ComputeFileHash(scriptPath);

        if (TryGetCachedAssembly(hash, out Assembly? cachedAssembly))
        {
            return FastCreateInstance<T>(cachedAssembly);
        }

        Assembly assembly = CompileAndCache(scriptPath, hash);
        return FastCreateInstance<T>(assembly);
    }

    /// <summary>
    /// Async version for non-blocking loads
    /// </summary>
    public ValueTask<T?> ExecuteScriptAsync<T>(string scriptPath) where T : class
    {
        return new ValueTask<T?>(Task.Run(() => ExecuteScript<T>(scriptPath)));
    }

    private static MetadataReference[] CreateReferences()
    {
        string[] refs =
        [
            typeof(object).Assembly.Location,                          // System.Private.CoreLib
            typeof(Console).Assembly.Location,                         // System.Console
            typeof(ICSharpScript).Assembly.Location,                   // Script interface
            typeof(IIrcSession).Assembly.Location,                     // IRC types
            Assembly.Load("System.Runtime").Location,
            Assembly.Load("System.Collections").Location,
            Assembly.Load("System.Windows").Location,
            Assembly.Load("System.Linq").Location
        ];

        MetadataReference[] references = new MetadataReference[refs.Length];
        for (int i = 0; i < refs.Length; i++)
        {
            references[i] = MetadataReference.CreateFromFile(refs[i]);
        }

        return references;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T? FastCreateInstance<T>(Assembly? assembly) where T : class
    {
        if (assembly == null) return null;

        // Find type without LINQ allocation
        Type? targetType = null;
        Type interfaceType = typeof(T);

        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type))
            {
                targetType = type;
                break;
            }
        }

        if (targetType == null)
            throw new InvalidOperationException($"No implementation of {typeof(T).Name} found");

        // Use compiled lambda for fastest instantiation
        return (T?) Activator.CreateInstance(targetType);
    }

    private Assembly CompileAndCache(string scriptPath, uint hash)
    {
        // Read source code
        ReadOnlySpan<char> sourceCode;
        using (StreamReader reader = new(scriptPath))
        {
            sourceCode = reader.ReadToEnd().AsSpan();
        }

        // Fast compile
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            sourceCode.ToString(),
            _parseOptions,
            path: scriptPath);

        CSharpCompilation compilation = CSharpCompilation.Create(
            $"Script_{hash:X8}",
            [syntaxTree],
            _references,
            _compilationOptions);

        using MemoryStream ms = new();
        EmitResult emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            throw new CSharpCompilationException(emitResult.Diagnostics);
        }

        byte[] assemblyBytes = ms.ToArray();
        SaveToCache(hash, assemblyBytes);

        // Load assembly
        ms.Position = 0;
        AssemblyLoadContext context = new(null, isCollectible: true);
        Assembly assembly = context.LoadFromStream(ms);

        // Create memory-mapped file
        MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(
            GetCachePath(hash),
            FileMode.Open,
            null,
            assemblyBytes.Length,
            MemoryMappedFileAccess.Read);

        _compiledScripts[hash] = new CompiledScript(assembly, hash, mmf, assemblyBytes.Length);

        return assembly;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe uint ComputeFileHash(string filePath)
    {
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        Span<byte> buffer = stackalloc byte[4096];

        uint hash = 2166136261u; // FNV-1a offset basis
        int bytesRead;

        while ((bytesRead = fs.Read(buffer)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                hash ^= buffer[i];
                hash *= 16777619u; // FNV-1a prime
            }
        }

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetCachePath(uint hash) => Path.Combine(_cacheDirectory, $"{hash:X8}.dll");

    private static FrozenDictionary<string, ReportDiagnostic> GetDiagnosticOptions()
    {
        // Suppress warnings for performance
        return new Dictionary<string, ReportDiagnostic>
        {
            ["CS1701"] = ReportDiagnostic.Suppress, // Assembly reference mismatch
            ["CS1702"] = ReportDiagnostic.Suppress,
            ["CS1591"] = ReportDiagnostic.Suppress  // Missing XML comments
        }.ToFrozenDictionary();
    }

    private Assembly? LoadFromDiskCache(string cachePath, uint hash)
    {
        _cacheLock.EnterReadLock();
        try
        {
            FileInfo fileInfo = new(cachePath);
            MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(
                cachePath,
                FileMode.Open,
                null,
                fileInfo.Length,
                MemoryMappedFileAccess.Read);

            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, fileInfo.Length, MemoryMappedFileAccess.Read);
            byte[] buffer = _bytePool.Rent((int) fileInfo.Length);
            try
            {
                accessor.ReadArray(0, buffer, 0, (int) fileInfo.Length);
                using MemoryStream ms = new(buffer, 0, (int) fileInfo.Length, false);
                AssemblyLoadContext context = new(null, isCollectible: true);
                Assembly assembly = context.LoadFromStream(ms);

                // Cache in memory
                _compiledScripts[hash] = new CompiledScript(assembly, hash, mmf, fileInfo.Length);

                return assembly;
            }
            finally
            {
                _bytePool.Return(buffer, clearArray: true);
            }
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    private Assembly? LoadFromMemoryMappedFile(CompiledScript cached)
    {
        try
        {
            using MemoryMappedViewAccessor accessor = cached.MappedFile.CreateViewAccessor(0, cached.Size, MemoryMappedFileAccess.Read);
            byte[] buffer = _bytePool.Rent((int) cached.Size);
            try
            {
                accessor.ReadArray(0, buffer, 0, (int) cached.Size);
                using MemoryStream ms = new(buffer, 0, (int) cached.Size, false);
                AssemblyLoadContext context = new(null, isCollectible: true);
                return context.LoadFromStream(ms);
            }
            finally
            {
                _bytePool.Return(buffer, clearArray: true);
            }
        }
        catch
        {
            return null;
        }
    }

    private void SaveToCache(uint hash, byte[] assemblyBytes)
    {
        _cacheLock.EnterWriteLock();
        try
        {
            string cachePath = GetCachePath(hash);
            File.WriteAllBytes(cachePath, assemblyBytes);
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetCachedAssembly(uint hash, out Assembly? assembly)
    {
        assembly = null;

        // Check in-memory cache
        if (_compiledScripts.TryGetValue(hash, out CompiledScript cached))
        {
            if (cached.AssemblyRef.Target is Assembly asm)
            {
                assembly = asm;
                return true;
            }

            // Assembly was GC'd, reload from memory-mapped file
            assembly = LoadFromMemoryMappedFile(cached);
            return assembly != null;
        }

        // Check disk cache
        string cachePath = GetCachePath(hash);
        if (File.Exists(cachePath))
        {
            assembly = LoadFromDiskCache(cachePath, hash);
            return assembly != null;
        }

        return false;
    }

    private void WarmUp()
    {
        // JIT warm-up with a minimal script
        try
        {
            const string warmupCode = "public class W : spennyIRC.Scripting.Engine.ICSharpScript { public string Name => \"W\"; public string Version => \"1\"; public string Author => \"W\"; public string Description => \"W\"; public void Initialize() { } public void Execute() { } public void Shutdown() { } }";
            SyntaxTree tree = CSharpSyntaxTree.ParseText(warmupCode, _parseOptions);
            CSharpCompilation comp = CSharpCompilation.Create("Warmup", [tree], _references, _compilationOptions);
            using MemoryStream ms = new();
            comp.Emit(ms);
        }
        catch { /* Ignore warm-up failures */ }
    }

    private readonly struct CompiledScript(Assembly assembly, uint hash, MemoryMappedFile mappedFile, long size)
    {
        public readonly WeakReference AssemblyRef = new(assembly);
        public readonly uint Hash = hash;
        public readonly MemoryMappedFile MappedFile = mappedFile;
        public readonly long Size = size;
        public readonly long Timestamp = Environment.TickCount64;
    }
}