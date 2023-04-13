using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Utilities.GeneralCSharp
{
    public class CounterDictionary<TKey> : IEnumerable<KeyValuePair<TKey, int>>
    {
        private readonly Dictionary<TKey, int> _counts = new Dictionary<TKey, int>();

        public int this[TKey key] => this._counts.TryGetValue(key, out int previousCount) ? previousCount : 0;

        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator()
        {
            return this._counts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(TKey key, int countDelta)
        {
            this._counts[key] = this[key] + countDelta;
        }

        public string ToHumanReadable()
        {
            return string.Join(", ", this.Select(kvp => $"{kvp.Value} {kvp.Key}"));
        }
    }
}