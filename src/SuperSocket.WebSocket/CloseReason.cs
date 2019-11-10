using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public enum CloseReason : short
    {
        NormalClosure = 1000,
        GoingAway = 1001,
        ProtocolError = 1002,
        NotAcceptableData = 1003,
        TooLargeFrame = 1009,
        InvalidUTF8 = 1007,
        ViolatePolicy = 1008,
        ExtensionNotMatch = 1010,
        UnexpectedCondition = 1011,
        NoStatusCode = 1005
    }
}
