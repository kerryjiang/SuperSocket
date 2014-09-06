using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class DraftHybi00ReceiveFilter : FixedSizeReceiveFilter<StringPackageInfo>
    {
        public DraftHybi00ReceiveFilter()
            : base(8)
        {

        }

        public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var session = AppContext.CurrentSession;
            var bufferManager = session.AppServer.BufferManager;
            var websocketContext = WebSocketContext.Get(session) as IWebSocketSession;

            var responseBuilder = new StringBuilder();

            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine00);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);

            if (!string.IsNullOrEmpty(session.Origin))
                responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseOriginLine, session.Origin);

            responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseLocationLine, session.UriScheme, session.Host, session.Path);

            var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty));

            if (!string.IsNullOrEmpty(subProtocol))
                responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseProtocolLine, subProtocol);

            responseBuilder.AppendWithCrCf();
            var response = responseBuilder.ToString();
            var encoding = Encoding.UTF8;
            var data = bufferManager.GetBuffer(encoding.GetMaxByteCount(response.Length));
            var length = encoding.GetBytes(response, 0, response.Length, data, 0);
            //Encrypt message
            byte[] secret = GetResponseSecurityKey(secKey1, secKey2, secKey3);

            throw new NotImplementedException();
        }

        private const string m_SecurityKeyRegex = "[^0-9]";

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, m_SecurityKeyRegex, String.Empty);
            string k2 = Regex.Replace(secKey2, m_SecurityKeyRegex, String.Empty);

            //Convert received string to 64 bit integer.
            Int64 intK1 = Int64.Parse(k1);
            Int64 intK2 = Int64.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
            byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            byte[] bChallenge = new byte[b1.Length + b2.Length + b3.Length];
            Array.Copy(b1, 0, bChallenge, 0, b1.Length);
            Array.Copy(b2, 0, bChallenge, b1.Length, b2.Length);
            Array.Copy(b3, 0, bChallenge, b1.Length + b2.Length, b3.Length);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge);
            return hash;
        }
    }
}
