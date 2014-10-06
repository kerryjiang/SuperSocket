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
            var context = WebSocketContext.Get(session);

            var responseBuilder = new StringBuilder();

            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine00);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);

            if (!string.IsNullOrEmpty(context.Origin))
                responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseOriginLine, context.Origin);

            responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseLocationLine, context.UriScheme, context.Host, context.Path);

            var subProtocol = context.GetAvailableSubProtocol();

            if (!string.IsNullOrEmpty(subProtocol))
                responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseProtocolLine, subProtocol);

            responseBuilder.AppendWithCrCf();
            var response = responseBuilder.ToString();
            var encoding = Encoding.UTF8;
            var data = bufferManager.GetBuffer(encoding.GetMaxByteCount(response.Length));
            var length = encoding.GetBytes(response, 0, response.Length, data, 0);

            var secKey1 = context.HandshakeRequest.Get(WebSocketConstant.SecWebSocketKey1);
            var secKey2 = context.HandshakeRequest.Get(WebSocketConstant.SecWebSocketKey2);

            byte[] secret;

            if(packageData.Count == 1)
                secret = GetResponseSecurityKey(secKey1, secKey2, packageData[0]);
            else
            {
                var secKey3 = bufferManager.GetBuffer(8);
                try
                {
                    secret = GetResponseSecurityKey(secKey1, secKey2, new ArraySegment<byte>(secKey3));
                }
                finally
                {
                    bufferManager.ReturnBuffer(secKey3);
                }
            }

            session.SocketSession.TrySend(new ArraySegment<byte>(secret));
            NextReceiveFilter = new DraftHybi00DataReceiveFilter();
            return null;
        }

        private const string m_SecurityKeyRegex = "[^0-9]";

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, ArraySegment<byte> secKey3)
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
            ArraySegment<byte> b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            byte[] bChallenge = new byte[b1.Length + b2.Length + b3.Count];
            Array.Copy(b1, 0, bChallenge, 0, b1.Length);
            Array.Copy(b2, 0, bChallenge, b1.Length, b2.Length);
            Array.Copy(b3.Array, b3.Offset, bChallenge, b1.Length + b2.Length, b3.Count);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge);
            return hash;
        }
    }
}
