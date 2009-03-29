using System;
using System.Collections;
using System.Collections.Generic;

namespace NDoc3.Core
{
	///<summary>
	/// An <see cref="IDictionary{TKey,TValue}"/> implementation for reference type values. 
	/// Avoids the need for calling <see cref="IDictionary{TKey,TValue}.TryGetValue"/> and the impact of semantics change
	/// of calling <see cref="Dictionary{TKey,TValue}.this"/> by throwing an exception if the
	/// key doesn't exist.
	///</summary>
	///<typeparam name="TKey">the type of the key - no restrictions</typeparam>
	///<typeparam name="TVal">the type of the value - must be a reference type</typeparam>
	[Serializable]
	internal class ReferenceTypeDictionary<TKey,TVal> 
		: IDictionary<TKey,TVal>
		where TVal:class
	{
		private readonly IDictionary<TKey,TVal> _inner;

		/// <summary>
		/// Creates a default instance, using <see cref="Dictionary{TKey,TVal}"/> as underlying dictionary.
		/// </summary>
		public ReferenceTypeDictionary()
			: this(new Dictionary<TKey, TVal>())
		{
		}

		/// <summary>
		/// Creates a default instance, wrapping <paramref name="inner"/> as underlying dictionary.
		/// </summary>
		public ReferenceTypeDictionary(IDictionary<TKey,TVal> inner)
		{
			if (inner == null) throw new ArgumentNullException("inner");
			_inner = inner;
		}

		public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator()
		{
			return _inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, TVal> item)
		{
			_inner.Add(item);
		}

		public void Clear()
		{
			_inner.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TVal> item)
		{
			return _inner.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex)
		{
			_inner.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TVal> item)
		{
			return _inner.Remove(item);
		}

		public int Count
		{
			get { return _inner.Count; }
		}

		public bool IsReadOnly
		{
			get { return _inner.IsReadOnly; }
		}

		public bool ContainsKey(TKey key)
		{
			return _inner.ContainsKey(key);
		}

		public void Add(TKey key, TVal value)
		{
			_inner.Add(key, value);
		}

		public bool Remove(TKey key)
		{
			return _inner.Remove(key);
		}

		bool IDictionary<TKey, TVal>.TryGetValue(TKey key, out TVal value)
		{
			return _inner.TryGetValue(key, out value);
		}

		public TVal this[TKey key]
		{
			get
			{
				TVal res;
				if (_inner.TryGetValue(key, out res))
				{
					return res;
				}
				return null;
			}
			set { _inner[key] = value; }
		}

		public ICollection<TKey> Keys
		{
			get { return _inner.Keys; }
		}

		public ICollection<TVal> Values
		{
			get { return _inner.Values; }
		}
	}
}