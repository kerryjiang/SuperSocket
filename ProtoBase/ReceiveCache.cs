using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public sealed class ReceiveCache : IList<ArraySegment<byte>>
    {
        private List<KeyValuePair<ArraySegment<byte>, object>> m_List = new List<KeyValuePair<ArraySegment<byte>, object>>();

        public ArraySegment<byte> Current
        {
            get
            {
                var count = Count;

                if (count == 0)
                    return new ArraySegment<byte>();

                return m_List[count - 1].Key;
            }
        }

        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public ArraySegment<byte> this[int index]
        {
            get
            {
                return m_List[index].Key;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Add(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void Add(ArraySegment<byte> item, object state)
        {
            var segmentState = state as ISegmentState;
            if (segmentState != null)
                segmentState.IncreaseReference();

            m_List.Add(new KeyValuePair<ArraySegment<byte>, object>(item, state));
            m_Total += item.Count;
        }

        public void SetLastItemLength(int length)
        {
            var lastPos = m_List.Count - 1;
            var lastItem = m_List[lastPos];
            var lastSegment = lastItem.Key;
            m_List[lastPos] = new KeyValuePair<ArraySegment<byte>, object>(new ArraySegment<byte>(lastSegment.Array, lastSegment.Offset, length), lastItem.Value);
            m_Total += length - lastSegment.Count;
        }

        public void Clear()
        {
            m_List.Clear();
            m_Total = 0;
        }

        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        private int m_Total;

        public int Total
        {
            get { return m_Total; }
        }

        public int Count
        {
            get { return m_List.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            var length = m_List.Count;

            for (int i = 0; i < length; i++)
            {
                yield return m_List[i].Key;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IList<KeyValuePair<ArraySegment<byte>, object>> GetAllCachedItems()
        {
            return m_List;
        }
    }
}
