using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.Http
{
    /// <summary>
    /// Represents a pipeline filter for parsing HTTP requests with keep-alive support.
    /// This filter can handle multiple requests over a single connection.
    /// </summary>
    public class HttpKeepAliveFilter : IPipelineFilter<HttpRequest>
    {
        private readonly HttpPipelineFilter _innerFilter;
        private bool _connectionClosed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpKeepAliveFilter"/> class.
        /// </summary>
        public HttpKeepAliveFilter()
        {
            _innerFilter = new HttpPipelineFilter();
        }

        /// <summary>
        /// Gets or sets the package decoder for the HTTP request.
        /// </summary>
        public IPackageDecoder<HttpRequest> Decoder 
        { 
            get => _innerFilter.Decoder; 
            set => _innerFilter.Decoder = value; 
        }

        /// <summary>
        /// Gets or sets the next pipeline filter in the chain.
        /// </summary>
        public IPipelineFilter<HttpRequest> NextFilter 
        { 
            get => _connectionClosed ? null : this;
            set { } // Keep-alive filter manages its own chain
        }

        /// <summary>
        /// Gets or sets the context associated with the pipeline filter.
        /// </summary>
        public object Context 
        { 
            get => _innerFilter.Context; 
            set => _innerFilter.Context = value; 
        }

        /// <summary>
        /// Filters the data stream to parse an HTTP request with keep-alive support.
        /// </summary>
        /// <param name="reader">The sequence reader for the data stream.</param>
        /// <returns>The parsed <see cref="HttpRequest"/>, or <c>null</c> if more data is needed.</returns>
        public HttpRequest Filter(ref SequenceReader<byte> reader)
        {
            if (_connectionClosed)
                return null;

            var request = _innerFilter.Filter(ref reader);
            
            if (request != null)
            {
                // Reset the inner filter for the next request
                _innerFilter.Reset();
                
                // Check if this request should close the connection
                if (!request.KeepAlive)
                {
                    _connectionClosed = true;
                }
            }

            return request;
        }

        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        public void Reset()
        {
            _innerFilter.Reset();
            _connectionClosed = false;
        }

        /// <summary>
        /// Marks the connection as closed, preventing further request processing.
        /// </summary>
        public void CloseConnection()
        {
            _connectionClosed = true;
        }
    }
}