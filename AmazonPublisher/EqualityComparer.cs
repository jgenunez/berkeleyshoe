using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmazonPublisher
{
    class EqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> _equalsFn;
        private Func<T, int> _getHashCodefn;

        public EqualityComparer(Func<T, T, bool> equalsFn, Func<T, int> getHashCodefn)
        {
            _equalsFn = equalsFn;
            _getHashCodefn = getHashCodefn;
        }

        public bool Equals(T x, T y)
        {
            return _equalsFn(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHashCodefn(obj);
        }
    }
}
