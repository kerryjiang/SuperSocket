using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.SocketBase
{
    public class AppContext
    {
        private const string m_SesionDataSlotName = "Session";

        public static IAppSession CurrentSession
        {
            get
            {
                return Thread.GetData(Thread.GetNamedDataSlot(m_SesionDataSlotName)) as IAppSession;
            }
        }

        internal static LocalDataStoreSlot SetCurrentSession(IAppSession session)
        {
            var slot = Thread.GetNamedDataSlot(m_SesionDataSlotName);
            Thread.SetData(slot, session);
            return slot;
        }
    }
}
