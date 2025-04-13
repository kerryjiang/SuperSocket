using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents a base class for handling JSON-based commands in a SuperSocket application.
    /// </summary>
    /// <typeparam name="TJsonObject">The type of the JSON object to deserialize.</typeparam>
    public abstract class JsonCommand<TJsonObject> : JsonCommand<IAppSession, TJsonObject>
    {
    }

    /// <summary>
    /// Represents a base class for handling JSON-based commands with a specific session type.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TJsonObject">The type of the JSON object to deserialize.</typeparam>
    public abstract class JsonCommand<TAppSession, TJsonObject> : ICommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the options for JSON serialization and deserialization.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCommand{TAppSession, TJsonObject}"/> class.
        /// </summary>
        public JsonCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Executes the command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The string package containing the JSON content.</param>
        public virtual void Execute(TAppSession session, IStringPackage package)
        {
            var content = package.Body;            
            ExecuteJson(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content));
        }

        /// <summary>
        /// Executes the command with the deserialized JSON object.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="jsonObject">The deserialized JSON object.</param>
        protected abstract void ExecuteJson(TAppSession session, TJsonObject jsonObject);

        /// <summary>
        /// Deserializes the JSON content into an object of type <typeparamref name="TJsonObject"/>.
        /// </summary>
        /// <param name="content">The JSON content to deserialize.</param>
        /// <returns>The deserialized JSON object.</returns>
        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content, JsonSerializerOptions);
        }
    }

    /// <summary>
    /// Represents a base class for handling JSON-based asynchronous commands in a SuperSocket application.
    /// </summary>
    /// <typeparam name="TJsonObject">The type of the JSON object to deserialize.</typeparam>
    public abstract class JsonAsyncCommand<TJsonObject> : JsonAsyncCommand<IAppSession, TJsonObject>
    {
    }

    /// <summary>
    /// Represents a base class for handling JSON-based asynchronous commands with a specific session type.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TJsonObject">The type of the JSON object to deserialize.</typeparam>
    public abstract class JsonAsyncCommand<TAppSession, TJsonObject> : IAsyncCommand<TAppSession, IStringPackage>
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the options for JSON serialization and deserialization.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAsyncCommand{TAppSession, TJsonObject}"/> class.
        /// </summary>
        public JsonAsyncCommand()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Asynchronously executes the command with the specified session and package.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="package">The string package containing the JSON content.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        public virtual async ValueTask ExecuteAsync(TAppSession session, IStringPackage package, CancellationToken cancellationToken)
        {
            var content = package.Body;
            await ExecuteJsonAsync(session, string.IsNullOrEmpty(content) ? default(TJsonObject) : Deserialize(content), cancellationToken);
        }

        /// <summary>
        /// Deserializes the JSON content into an object of type <typeparamref name="TJsonObject"/>.
        /// </summary>
        /// <param name="content">The JSON content to deserialize.</param>
        /// <returns>The deserialized JSON object.</returns>
        protected virtual TJsonObject Deserialize(string content)
        {
            return JsonSerializer.Deserialize<TJsonObject>(content, JsonSerializerOptions);
        }

        /// <summary>
        /// Asynchronously executes the command with the deserialized JSON object.
        /// </summary>
        /// <param name="session">The application session.</param>
        /// <param name="jsonObject">The deserialized JSON object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        protected abstract ValueTask ExecuteJsonAsync(TAppSession session, TJsonObject jsonObject, CancellationToken cancellationToken);
    }
}
