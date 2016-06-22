using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase
{
    class SingleItemList<T> : IList<T>
    {
        private T m_Item;

        public SingleItemList(T item)
        {
            m_Item = item;
        }


        public T this[int index]
        {
            get
            {
                if (index != 0)
                    throw new IndexOutOfRangeException();

                return m_Item;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return 1;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

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

        public IEnumerator<T> GetEnumerator()
        {
            yield return m_Item;
        }

        public int IndexOf(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}
