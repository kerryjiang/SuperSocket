// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace SuperSocket.Kestrel.IOCP;

public readonly struct SocketOperationResult
{
    public readonly SocketException? SocketError;

    public readonly int BytesTransferred;

    [MemberNotNullWhen(true, nameof(SocketError))]
    public readonly bool HasError => SocketError != null;

    public SocketOperationResult(int bytesTransferred)
    {
        SocketError = null;
        BytesTransferred = bytesTransferred;
    }

    public SocketOperationResult(SocketException exception)
    {
        SocketError = exception;
        BytesTransferred = 0;
    }
}
