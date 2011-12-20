using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    class ArraySegmentInfo<T>
        where T : IEquatable<T>
    {
        public ArraySegment<T> Segment { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }

    public class ArraySegmentList<T> : IList<T>
        where T : IEquatable<T>
    {
        private IList<ArraySegmentInfo<T>> m_Segments;
        private ArraySegmentInfo<T> m_PrevSegment;
        private int m_PrevSegmentIndex;

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
            int index = 0;

            for (int i = 0; i < m_Segments.Count; i++)
            {
                var currentSegment = m_Segments[i].Segment;
                int offset = currentSegment.Offset;

                for (int j = 0; j < currentSegment.Count; j++)
                {
                    if (currentSegment.Array[j + offset].Equals(item))
                        return index;

                    index++;
                }
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

                if (index == 0)
                {
                    m_PrevSegment = m_Segments[0];
                    m_PrevSegmentIndex = 0;
                    var seg = m_PrevSegment.Segment;
                    return seg.Array[seg.Offset];
                }

                int compareValue = 0;

                if (m_PrevSegment != null)
                {
                    if (index >= m_PrevSegment.From)
                    {
                        if (index <= m_PrevSegment.To)
                        {
                            var prevSegInfo = m_PrevSegment.Segment;
                            return prevSegInfo.Array[prevSegInfo.Offset + index - m_PrevSegment.From];
                        }
                        else
                        {
                            compareValue = 1;
                        }
                    }
                    else
                    {
                        compareValue = -1;
                    }
                }

                int from, to;

                if (compareValue != 0)
                {
                    from = m_PrevSegmentIndex + compareValue;

                    var segment = m_Segments[from];
                    if (index >= segment.From && index <= segment.To)
                    {
                        m_PrevSegment = segment;
                        m_PrevSegmentIndex = from;
                        var currentSegInfo = segment.Segment;
                        return currentSegInfo.Array[currentSegInfo.Offset + index - segment.From];
                    }

                    if (compareValue > 0)
                    {
                        from++;
                        to = m_Segments.Count - 1;
                    }
                    else
                    {
                        var tmp = from - 1;
                        from = 0;
                        to = tmp;
                    }
                }
                else
                {
                    from = 0;
                    to = m_Segments.Count - 1;
                }

                int segmentIndex;

                var result = QuickSearchSegment(from, to, index, out segmentIndex);

                if (result != null)
                {
                    m_PrevSegment = result;
                    m_PrevSegmentIndex = segmentIndex;
                    var currentSegment = result.Segment;
                    return currentSegment.Array[currentSegment.Offset + index - result.From];
                }

                m_PrevSegment = null;
                throw new IndexOutOfRangeException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private ArraySegmentInfo<T> QuickSearchSegment(int from, int to, int index, out int segmentIndex)
        {
            ArraySegmentInfo<T> segment;
            segmentIndex = -1;

            int diff = to - from;            

            if (diff == 0)
            {
                segment = m_Segments[from];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = from;
                    return segment;
                }

                return null;
            }
            else if (diff == 1)
            {
                segment = m_Segments[from];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = from;
                    return segment;
                }

                segment = m_Segments[to];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = to;
                    return segment;
                }

                return null;
            }

            int middle = from + diff / 2;

            segment = m_Segments[middle];

            if (index >= segment.From)
            {
                if (index <= segment.To)
                {
                    segmentIndex = middle;
                    return segment;
                }
                else
                {
                    return QuickSearchSegment(middle + 1, to, index, out segmentIndex);
                }
            }
            else
            {
                return QuickSearchSegment(from, middle - 1, index, out segmentIndex);
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
            int removedLen = removedSegment.To - removedSegment.From + 1;

            m_Segments.RemoveAt(index);

            m_PrevSegment = null;

            //the removed item is not the the last item 
            if(index != m_Segments.Count)
            {
                for (int i = index; i < m_Segments.Count; i++)
                {
                    m_Segments[i].From -= removedLen;
                    m_Segments[i].To -= removedLen;
                }
            }

            m_Count -= removedLen;
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

        public void ClearSegements()
        {
            m_Segments.Clear();
            m_PrevSegment = null;
            m_Count = 0;
        }

        public T[] ToArrayData()
        {
            return ToArrayData(0, m_Count);
        }

        public T[] ToArrayData(int startIndex, int length)
        {
            var result = new T[length];
            int from = 0, len = 0, total = 0;

            var startSegmentIndex = 0;

            if (startIndex != 0)
            {
                var startSegment = QuickSearchSegment(0, m_Segments.Count, startIndex, out startSegmentIndex);
                from = startIndex - startSegment.From;
                if (startSegment == null)
                    throw new IndexOutOfRangeException();
            }            

            for (var i = startSegmentIndex; i < m_Segments.Count; i++)
            {
                var currentSegmentInfo = m_Segments[i];
                var currentSegment = currentSegmentInfo.Segment;
                len = Math.Min(currentSegment.Count - from, length - total);
                Array.Copy(currentSegment.Array, currentSegment.Offset + from, result, total, len);
                total += len;

                if (total >= length)
                    break;

                from = 0;
            }

            return result;
        }

        public void TrimEnd(int trimSize)
        {
            if (trimSize <= 0)
                return;

            int expectedTo = this.Count - trimSize - 1;

            for (int i = m_Segments.Count - 1; i >= 0; i--)
            {
                var s = m_Segments[i];

                if (s.From <= expectedTo && expectedTo < s.To)
                {
                    s.To = expectedTo;
                    m_Count -= trimSize;
                    return;
                }

                RemoveSegmentAt(i);
            }
        }

        public int SearchLastSegment(SearchMarkState<T> state)
        {
            if(m_Segments.Count <= 0)
                return -1;

            var lastSegmentInfo = m_Segments[m_Segments.Count - 1];

            if (lastSegmentInfo == null)
                return -1;

            var lastSegment = lastSegmentInfo.Segment;

            var result = lastSegment.Array.SearchMark(lastSegment.Offset, lastSegment.Count, state.Mark);

            if (!result.HasValue)
                return -1;

            if (result.Value > 0)
            {
                state.Matched = 0;
                return result.Value - lastSegment.Offset + lastSegmentInfo.From;
            }

            state.Matched = 0 - result.Value;
            return -1;
        }
    }
}
