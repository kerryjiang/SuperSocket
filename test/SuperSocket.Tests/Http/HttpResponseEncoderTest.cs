using System;
using System.Buffers;
using System.Text;
using SuperSocket.Http;
using Xunit;

namespace SuperSocket.Tests.Http
{
    [Trait("Category", "HttpResponseEncoder")]
    public class HttpResponseEncoderTest
    {
        [Fact]
        public void TestEncodeBasicResponse()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "Hello World!"
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check status line
            Assert.Contains("HTTP/1.1 200 OK", responseText);
            
            // Check Content-Length header is set automatically
            Assert.Contains("Content-Length: 12", responseText);
            
            // Check Connection header
            Assert.Contains("Connection: keep-alive", responseText);
            
            // Check Server header is added
            Assert.Contains("Server: SuperSocket", responseText);
            
            // Check Date header is added
            Assert.Contains("Date:", responseText);
            
            // Check body
            Assert.Contains("Hello World!", responseText);
            
            // Check proper HTTP structure (headers followed by empty line and body)
            Assert.Contains("\r\n\r\nHello World!", responseText);
        }

        [Fact]
        public void TestEncodeResponseWithCustomHeaders()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 404,
                StatusMessage = "Not Found",
                Body = "Resource not found"
            };
            
            response.Headers["Content-Type"] = "text/plain";
            response.Headers["Custom-Header"] = "CustomValue";

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check status line
            Assert.Contains("HTTP/1.1 404 Not Found", responseText);
            
            // Check custom headers
            Assert.Contains("Content-Type: text/plain", responseText);
            Assert.Contains("Custom-Header: CustomValue", responseText);
            
            // Check body
            Assert.Contains("Resource not found", responseText);
        }

        [Fact]
        public void TestEncodeResponseWithoutBody()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 204,
                StatusMessage = "No Content"
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check status line
            Assert.Contains("HTTP/1.1 204 No Content", responseText);
            
            // Content-Length should not be set for empty body
            Assert.DoesNotContain("Content-Length:", responseText);
            
            // Should end with double CRLF (no body)
            Assert.EndsWith("\r\n\r\n", responseText);
        }

        [Fact]
        public void TestEncodeResponseWithKeepAliveFalse()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "Test",
                KeepAlive = false
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check Connection header reflects KeepAlive setting
            Assert.Contains("Connection: close", responseText);
        }

        [Fact]
        public void TestEncodeResponseWithCustomEncoding()
        {
            // Arrange
            var encoding = Encoding.ASCII;
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "ASCII Content"
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = encoding.GetString(writer.WrittenSpan);
            
            // Check that the response can be decoded with ASCII
            Assert.Contains("HTTP/1.1 200 OK", responseText);
            Assert.Contains("ASCII Content", responseText);
        }

        [Fact]
        public void TestEncodeResponseWithSpecialCharacters()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "Content with special chars: áéíóú"
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check that special characters are preserved
            Assert.Contains("Content with special chars: áéíóú", responseText);
            
            // Check Content-Length accounts for UTF-8 bytes
            var bodyBytes = Encoding.UTF8.GetByteCount("Content with special chars: áéíóú");
            Assert.Contains($"Content-Length: {bodyBytes}", responseText);
        }

        [Fact]
        public void TestEncodeResponsePreservesExistingHeaders()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "Test"
            };
            
            // Set custom Date and Server headers
            response.Headers["Date"] = "Wed, 21 Oct 2015 07:28:00 GMT";
            response.Headers["Server"] = "CustomServer/1.0";

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check that custom headers are preserved (not overwritten)
            Assert.Contains("Date: Wed, 21 Oct 2015 07:28:00 GMT", responseText);
            Assert.Contains("Server: CustomServer/1.0", responseText);
        }

        [Fact]
        public void TestEncodeThrowsOnNullArguments()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse();
            var writer = new ArrayBufferWriter<byte>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => encoder.Encode(null, response));
            Assert.Throws<ArgumentNullException>(() => encoder.Encode(writer, null));
        }

        [Fact]
        public void TestEncodeWithLargeBody()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var largeBody = new string('A', 10000); // 10KB of 'A' characters
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = largeBody
            };

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 10000); // Should be larger than body due to headers

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check Content-Length is correct
            Assert.Contains("Content-Length: 10000", responseText);
            
            // Check body is present
            Assert.Contains(largeBody, responseText);
        }

        [Fact]
        public void TestEncodeServerSentEventsResponse()
        {
            // Arrange
            var encoder = new HttpResponseEncoder();
            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "data: Hello SSE\n\n"
            };
            
            response.SetupForServerSentEvents();

            var writer = new ArrayBufferWriter<byte>();

            // Act
            var bytesWritten = encoder.Encode(writer, response);

            // Assert
            Assert.True(bytesWritten > 0);

            var responseText = Encoding.UTF8.GetString(writer.WrittenSpan);
            
            // Check SSE-specific headers
            Assert.Contains("Content-Type: text/event-stream", responseText);
            Assert.Contains("Cache-Control: no-cache", responseText);
            Assert.Contains("Connection: keep-alive", responseText);
            Assert.Contains("Access-Control-Allow-Origin: *", responseText);
            Assert.Contains("Access-Control-Allow-Headers: Cache-Control", responseText);
            
            // Check body
            Assert.Contains("data: Hello SSE", responseText);
        }
    }
}
