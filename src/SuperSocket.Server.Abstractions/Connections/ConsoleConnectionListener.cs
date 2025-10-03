using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Listener for console connections that immediately creates a single connection.
    /// </summary>
    public class ConsoleConnectionListener : IConnectionListener
    {
        private readonly ILogger _logger;
        private bool _isRunning;
        private IConnection _consoleConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleConnectionListener"/> class.
        /// </summary>
        /// <param name="options">The options for the listener.</param>
        /// <param name="connectionFactory">The factory for creating connections.</param>
        /// <param name="logger">The logger for logging events.</param>
        public ConsoleConnectionListener(ListenOptions options, IConnectionFactory connectionFactory, ILogger logger)
        {
            Options = options;
            ConnectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public ListenOptions Options { get; }

        /// <inheritdoc/>
        public IConnectionFactory ConnectionFactory { get; }

        /// <inheritdoc/>
        public bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public event NewConnectionAcceptHandler NewConnectionAccept;

        /// <inheritdoc/>
        public bool Start()
        {
            if (_isRunning)
                return true;

            try
            {
                _isRunning = true;
                
                // Create the console connection immediately
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _consoleConnection = await ConnectionFactory.CreateConnection(null, default).ConfigureAwait(false);
                        
                        if (_consoleConnection != null && NewConnectionAccept != null)
                        {
                            await NewConnectionAccept(Options, _consoleConnection).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to create console connection");
                    }
                });

                _logger?.LogInformation("Console connection listener started");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start console connection listener");
                _isRunning = false;
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            if (_consoleConnection != null)
            {
                try
                {
                    await _consoleConnection.CloseAsync(CloseReason.ServerShutdown).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error closing console connection during shutdown");
                }
                finally
                {
                    _consoleConnection = null;
                }
            }

            _logger?.LogInformation("Console connection listener stopped");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isRunning)
            {
                StopAsync().GetAwaiter().GetResult();
            }
        }
    }
}