using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Source: http://www.inlandstudios.com/en/datafiles/HashSet.cs
namespace SparrowXNA
{
    public class SPHashSet<T> where T : class
    {
        Dictionary<T, bool> data = new Dictionary<T, bool>(4);
        List<T> dataList = new List<T>(4);

        public SPHashSet()
        {
        }

        public T Current { get { return (dataList.Count > 0) ? dataList[0] : null; } }
        public List<T> EnumerableSet { get { return dataList; } }

        public void Add(T t)
        {
            if (Contains(t) == false)
            {
                data.Add(t, true);
                dataList.Add(t);
            }
        }

        public void Remove(T t)
        {
            if (Contains(t))
            {
                data.Remove(t);
                dataList.Remove(t);
            }
        }

        public void Clear()
        {
            data.Clear();
            dataList.Clear();
        }

        public int Count { get { return data.Count; } }

        public bool Contains(T t)
        {
            return data.ContainsKey(t);
        }

        /*
        struct ValueEnumerator : IEnumerator<T>
        {
            internal ValueEnumerator(Dictionary<T, bool> hashset)
            {
                data = hashset;
                values = data.GetEnumerator();
            }

            Dictionary<T, bool> data;
            Dictionary<T, bool>.Enumerator values;

            public T Current
            {
                get { return values.Current.Key; }
            }

            public void Dispose()
            {
                values.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return values.Current; }
            }

            public void Reset()
            {
                values = data.GetEnumerator();
            }

            public bool MoveNext()
            {
                return values.MoveNext();
            }
        }
        */

        /*
        public IEnumerator<T> GetEnumerator()
        {
            return dataList.GetEnumerator(); // new ValueEnumerator(data);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dataList.GetEnumerator(); // new ValueEnumerator(data);
        }
        */
    }
}
