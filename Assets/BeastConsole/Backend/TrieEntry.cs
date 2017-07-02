namespace BeastConsole.Backend {

    /// <summary>
    /// Defines a key/value pair that can be set or retrieved from <see cref="Trie{TValue}"/>.
    /// </summary>
    public struct TrieEntry<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrieEntry{TValue}"/> structure with the specified key and value.
        /// </summary>
        /// <param name="key">The <see cref="string"/> object defined in each key/value pair.</param>
        /// <param name="value">The definition associated with key.</param>
        public TrieEntry(string key, TValue value)
            : this()
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets the key in the key/value pair.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the value in the key/value pair.
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", Key, Value);
        }
    }
}