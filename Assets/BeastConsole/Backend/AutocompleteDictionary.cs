namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AutoCompleteDictionary<T> : SortedDictionary<string, T>
    {
        public AutoCompleteDictionary()
        : base(new AutoCompleteComparer())
        {
            m_comparer = this.Comparer as AutoCompleteComparer;
        }

        public T LowerBound(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[m_comparer.LowerBound];
        }

        public T UpperBound(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[m_comparer.UpperBound];
        }

        public T AutoCompleteLookup(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            string key = (m_comparer.UpperBound == null) ? m_comparer.LowerBound : m_comparer.UpperBound;
            return this[key];
        }

        private class AutoCompleteComparer : IComparer<string>
        {
            private string m_lowerBound = null;
            private string m_upperBound = null;
            public string LowerBound { get { return m_lowerBound; } }
            public string UpperBound { get { return m_upperBound; } }
            public int Compare(string x, string y)
            {
                int comparison = Comparer<string>.Default.Compare(x, y);
                if (comparison >= 0)
                {
                    m_lowerBound = y;
                }
                if (comparison <= 0)
                {
                    m_upperBound = y;
                }
                return comparison;
            }
            public void Reset()
            {
                m_lowerBound = null;
                m_upperBound = null;
            }
        }
        private AutoCompleteComparer m_comparer;
    }
}