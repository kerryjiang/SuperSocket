using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;

namespace SuperSocket.Command
{
    public abstract class JsonCommand<TJsonObject> : JsonCommand<IAppSession, TJsonObject>
    {

    }

    public abstract class JsonCommand<TAppSession, TJsonObject> : ICommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public JsonSerializerOptions JsonSerializerOptions { get; }

        public JsonCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public virtual void Execute(TAppSession session, IStringPackage package)
        {
            var content = package.Body;            
            ExecuteJson(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected abstract void ExecuteJson(TAppSession session, TJsonObject jsonObject);

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content, JsonSerializerOptions);
        }
    }

    public abstract class JsonAsyncCommand<TJsonObject> : JsonAsyncCommand<IAppSession, TJsonObject>
    {

    }

    public abstract class JsonAsyncCommand<TAppSession, TJsonObject> : IAsyncCommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public JsonSerializerOptions JsonSerializerOptions { get; }

        public JsonAsyncCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public virtual async ValueTask ExecuteAsync(TAppSession session, IStringPackage package)
        {
            var content = package.Body;
            await ExecuteJsonAsync(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content, JsonSerializerOptions);
        }

        protected abstract ValueTask ExecuteJsonAsync(TAppSession session, TJsonObject jsonObject);
    }
}
