using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    class ArraySegmentInfo<T>
    {
        public ArraySegment<T> Segment { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }

    public class ArraySegmentList<T> : IList<T>
    {
        private IList<ArraySegmentInfo<T>> m_Segments;
        private ArraySegmentInfo<T> m_PrevSegment;

        private int m_Count;

        public ArraySegmentList()
        {
            m_Segments = new List<ArraySegmentInfo<T>>();
        }

        public ArraySegmentList(IList<ArraySegment<T>> segments) : this()
        {
            CalculateSegmentsInfo(segments);
        }

        private void CalculateSegmentsInfo(IList<ArraySegment<T>> segments)
        {
            int total = 0;

            foreach (var segment in segments)
            {
                if (segment.Count <= 0)
                    continue;

                m_Segments.Add(new ArraySegmentInfo<T>
                {
                    Segment = segment,
                    From = total,
                    To = total + segment.Count - 1
                });
                total += segment.Count;
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
                        return m_PrevSegment.Segment.Array[index - m_PrevSegment.From];
                    }
                }

                for (int i = 0; i < m_Segments.Count; i++)
                {
                    var segment = m_Segments[i];
                    if (index >= segment.From && index <= segment.To)
                    {
                        m_PrevSegment = segment;
                        return segment.Segment.Array[index - segment.From];
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

        public void RemoveSegmentAt(int index)
        {
            var removedSegment = m_Segments[index];
            m_Segments.RemoveAt(index);

            m_PrevSegment = null;

            //the removed item is not the the last item 
            if(index != m_Segments.Count)
            {
                for (int i = index; i < m_Segments.Count; i++)
                {
                    m_Segments[i].From -= removedSegment.Segment.Count;
                    m_Segments[i].To -= removedSegment.Segment.Count;
                }
            }

            m_Count -= removedSegment.Segment.Count;
        }

        public void AddSegment(ArraySegment<T> segment)
        {
            if (segment.Count <= 0)
                return;

            var currentTotal = m_Count;

            m_Segments.Add(new ArraySegmentInfo<T>
            {
                Segment = segment,
                From = currentTotal,
                To = currentTotal + segment.Count - 1
            });

            m_Count = currentTotal + segment.Count;
        }

        public int SegmentCount
        {
            get { return m_Segments.Count; }
        }
    }
}
