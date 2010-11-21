using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public class ArraySegmentItem<T>
    {
        public IList<T> Data { get; private set; }

        public int Offset { get; private set; }

        public int Length { get; private set; }

        public ArraySegmentItem(IList<T> data)
            : this(data, 0, data.Count)
        {

        }

        public ArraySegmentItem(IList<T> data, int offset, int length)
        {
            Data = data;
            Offset = offset;
            Length = length;
        }
    }

    class ArraySegmentItemInfo<T>
    {
        public ArraySegmentItem<T> Segment { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }

    public class ArraySegmentList<T> : IList<T>
    {
        private IList<ArraySegmentItemInfo<T>> m_Segments;
        private ArraySegmentItemInfo<T> m_PrevSegment;

        private int m_Count;

        public ArraySegmentList(IList<ArraySegmentItem<T>> segments)
        {
            PrepareData(segments);
        }

        private void PrepareData(IList<ArraySegmentItem<T>> segments)
        {
            int total = 0;

            m_Segments = new List<ArraySegmentItemInfo<T>>();

            foreach (var segment in segments)
            {
                if (segment.Length <= 0)
                    continue;

                m_Segments.Add(new ArraySegmentItemInfo<T>
                {
                    Segment = segment,
                    From = total,
                    To = total + segment.Length - 1
                });
                total += segment.Length;
            }

            m_Count = total;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index > Count - 1)
                    throw new IndexOutOfRangeException();

                if (m_PrevSegment != null)
                {
                    if (index >= m_PrevSegment.From && index <= m_PrevSegment.To)
                    {
                        return m_PrevSegment.Segment.Data[m_PrevSegment.Segment.Offset + index - m_PrevSegment.From];
                    }
                }

                for (int i = 0; i < m_Segments.Count; i++)
                {
                    var segment = m_Segments[i];
                    if (index >= segment.From && index <= segment.To)
                    {
                        m_PrevSegment = segment;
                        return segment.Segment.Data[segment.Segment.Offset + index - segment.From];
                    }
                }

                throw new IndexOutOfRangeException();

            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return m_Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
