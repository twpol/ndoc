using System;
using System.Collections.Generic;

namespace NDoc3.Core
{
	///<summary>
    ///</summary>
    [Serializable]
    internal class SortedStringDictionary : ReferenceTypeDictionary<string,string>
    {
		///<summary>
		/// Creates a new instance, using <see cref="SortedList{TKey,TVal}"/> 
		/// for storing items.
		///</summary>
		public SortedStringDictionary() 
			: base(new SortedList<string,string>())
		{
		}
    }
}
