namespace spennyIRC.Scripting.Engine
{
    public interface ICSharpScriptManager : IDisposable
    {
        void ClearCache();

        T? ExecuteScript<T>(string scriptPath) where T : class;

        ValueTask<T?> ExecuteScriptAsync<T>(string scriptPath) where T : class;

        void UnloadAllScripts();

        void UnloadScript(string scriptPath);
    }
}