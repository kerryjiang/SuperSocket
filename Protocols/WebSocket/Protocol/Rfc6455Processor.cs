using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455
    /// </summary>
    class Rfc6455Processor : DraftHybi10Processor
    {
        public Rfc6455Processor()
            : base(13, new CloseStatusCodeRfc6455())
        {

        }

        protected override string OriginKey
        {
            get
            {
                return WebSocketConstant.Origin;
            }
        }

        public override bool IsValidCloseCode(int code)
        {
            var closeCode = this.CloseStatusClode;

            if (code >= 0 && code <= 999)
                return false;

            if (code >= 1000 && code <= 2999)
            {
                if (code == closeCode.NormalClosure
                    || code == closeCode.GoingAway
                    || code == closeCode.ProtocolError
                    || code == closeCode.NotAcceptableData
                    || code == closeCode.TooLargeFrame
                    || code == closeCode.InvalidUTF8
                    || code == closeCode.ViolatePolicy
                    || code == closeCode.ExtensionNotMatch
                    || code == closeCode.UnexpectedCondition)
                {
                    return true;
                }

                return false;
            }

            if (code >= 3000 && code <= 4999)
                return true;

            return false;
        }
    }
}
