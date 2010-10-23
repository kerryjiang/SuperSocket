using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SuperSocket.SocketServiceCore
{
    [DataContract]
    public enum CreateUserResult
    {
        [EnumMember]
        UserNameAlreadyExist,

        [EnumMember]
        Success,

        [EnumMember]
        UnknownError
    }
}
