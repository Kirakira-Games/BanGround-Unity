using System.Collections.Generic;

namespace BGEditor
{
    public class IDPool
    {
        private int curId;

        public HashSet<int> pool;
        public HashSet<int> used;

        public IDPool()
        {
            curId = 1;
            pool = new HashSet<int>();
            used = new HashSet<int>();
        }

        private void ExpandPool()
        {
            while (used.Contains(curId))
                curId++;
            pool.Add(curId);
        }

        public bool Used(int id)
        {
            return used.Contains(id);
        }

        public void Recycle(int id)
        {
            if (used.Contains(id))
                used.Remove(id);
            if (!pool.Contains(id))
                pool.Add(id);
        }

        public void Register(int id)
        {
            if (!used.Contains(id))
                used.Add(id);
            if (pool.Contains(id))
                pool.Remove(id);
        }

        public int RegisterNext()
        {
            int id = Next();
            Register(id);
            return id;
        }

        public int Next()
        {
            if (pool.Count == 0)
                ExpandPool();
            var ret = pool.GetEnumerator();
            ret.MoveNext();
            return ret.Current;
        }
    }
}
