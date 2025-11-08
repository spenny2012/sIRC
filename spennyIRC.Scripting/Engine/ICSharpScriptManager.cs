
namespace spennyIRC.Scripting.Engine
{
    public interface ICSharpScriptManager
    {
        void ClearCache();
        void Dispose();
        T? ExecuteScript<T>(string scriptPath) where T : class;
        ValueTask<T?> ExecuteScriptAsync<T>(string scriptPath) where T : class;
    }
}