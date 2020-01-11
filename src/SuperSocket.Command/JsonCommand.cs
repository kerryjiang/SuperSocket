using System;
using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;


namespace SuperSocket.Command
{
    public abstract class JsonCommand<TKey, TJsonObject> : JsonCommand<TKey, IAppSession, TJsonObject>
    {

    }

    public abstract class JsonCommand<TKey, TAppSession, TJsonObject> : ICommand<TKey, TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public abstract TKey Key { get; }

        public virtual string Name
        {
            get
            {
                return Key.ToString();
            }
        }

        public virtual void Execute(TAppSession session, IStringPackage package)
        {
            var content = package.Body;            
            ExecuteJson(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected abstract void ExecuteJson(TAppSession session, TJsonObject jsonObject);

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content);
        }
    }

    public abstract class JsonAsyncCommand<TKey, TJsonObject> : JsonAsyncCommand<TKey, IAppSession, TJsonObject>
    {

    }

    public abstract class JsonAsyncCommand<TKey, TAppSession, TJsonObject> : IAsyncCommand<TKey, TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        public TKey Key { get; }

        public virtual string Name
        {
            get
            {
                return Key.ToString();
            }
        }

        public virtual async ValueTask ExecuteAsync(TAppSession session, IStringPackage package)
        {
            var content = package.Body;
            await ExecuteJsonAsync(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content);
        }

        protected abstract ValueTask ExecuteJsonAsync(TAppSession session, TJsonObject jsonObject);
    }
}
