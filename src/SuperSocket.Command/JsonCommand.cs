using System;
using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;


namespace SuperSocket.Command
{
    public abstract class JsonCommand<TKey, TJsonObject> : ICommand<TKey, IStringPackage>
    {
        public abstract TKey Key { get; }

        public virtual string Name
        {
            get
            {
                return Key.ToString();
            }
        }

        public virtual void Execute(IAppSession session, IStringPackage package)
        {
            var content = package.Body;            
            ExecuteJson(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected abstract void ExecuteJson(IAppSession session, TJsonObject jsonObject);

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content);
        }
    }

    public abstract class JsonAsyncCommand<TKey, TJsonObject> : IAsyncCommand<TKey, IStringPackage>
    {
        public TKey Key { get; }

        public virtual string Name
        {
            get
            {
                return Key.ToString();
            }
        }

        public virtual async Task ExecuteAsync(IAppSession session, IStringPackage package)
        {
            var content = package.Body;
            await ExecuteJsonAsync(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content);
        }

        protected abstract Task ExecuteJsonAsync(IAppSession session, TJsonObject jsonObject);
    }
}
