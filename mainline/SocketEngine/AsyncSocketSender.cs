using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketEngine
{
    using System;

    public class AsyncSocketSender
    {
        #region Events & Delegates

        public delegate void EventError(Exception ex);
        public event EventError OnError;

        public delegate void EventSocketClosed();
        public event EventSocketClosed OnSocketClosed;

        #endregion Events & Delegates

        #region Fields

        private System.Net.Sockets.Socket mSocket;
        private SocketBuffer mSocketBuffer;
        private byte[] mWorkBuffer;
        private int mToSend = 0;
        private AsyncCallback mCallback;
        private object mLocker = new object();

        private System.Threading.ReaderWriterLockSlim mReaderWriterLocker = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.SupportsRecursion);
        private DateTime mLastSendTime = DateTime.MaxValue;

        #endregion Fields

        #region Constructors

        public AsyncSocketSender(System.Net.Sockets.Socket socket)
        {
            mSocket = socket;
            mCallback = new AsyncCallback(this.Callback);
            mSocketBuffer = new SocketBuffer();
        }

        #endregion Constructors

        #region Properties

        public DateTime LastSendTime
        {
            get
            {
                mReaderWriterLocker.EnterReadLock();
                try
                {
                    return mLastSendTime;
                }
                finally
                {
                    mReaderWriterLocker.ExitReadLock();
                }
            }
            set
            {
                mReaderWriterLocker.EnterWriteLock();
                try
                {
                    mLastSendTime = value;
                }
                finally
                {
                    mReaderWriterLocker.ExitWriteLock();
                }
            }
        }

        public Int32 BytesToSend
        {
            get
            {
                lock (mLocker)
                {
                    return mSocketBuffer.Length;
                }
            }
        }

        #endregion Properties

        #region Public Methods

        public void Send(byte[] buffer, int start, int length)
        {
            lock (mLocker)
            {
                mSocketBuffer.Write(buffer, start, length);
                Send();
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Callback(IAsyncResult result)
        {
            try
            {
                lock (mLocker)
                {
                    int sent = mSocket.EndSend(result);
                    if (sent == 0)
                    {
                        if (OnSocketClosed != null)
                        {
                            OnSocketClosed();
                        }
                        return;
                    }
                    LastSendTime = DateTime.Now;
                    mToSend -= sent;
                    if (mToSend > 0)
                    {
                        mSocket.BeginSend(mWorkBuffer, mWorkBuffer.Length - mToSend, mToSend,
                                          System.Net.Sockets.SocketFlags.None, mCallback, null);
                    }
                    else
                    {
                        Send();
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                if (OnSocketClosed != null)
                {
                    OnSocketClosed();
                }
            }
            catch (Exception exp)
            {
                if (OnError != null)
                {
                    OnError(exp);
                }
            }
        }

        private void Send()
        {
            lock (mLocker)
            {
                if (mToSend > 0)
                {//Still sending bytes from mWorkBuffer. So do NOT overwrite it before being fully sent!!!
                    return;// The data will not be sent now, it will be sent by Callback...
                }
                mWorkBuffer = mSocketBuffer.Read(Math.Min(mSocketBuffer.Length, 16348));
                mToSend = mWorkBuffer.Length;
                if (mToSend > 0)
                {
                    mSocket.BeginSend(mWorkBuffer, 0, mToSend, System.Net.Sockets.SocketFlags.None, mCallback, null);
                }
            }
        }

        #endregion Private Methods
    }
}
