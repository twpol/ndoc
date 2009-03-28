using System;
using System.Collections.Generic;

namespace NDoc3.Core
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class SortedStringDictionary : SortedList<string,string>, IDictionary<string,string>
    {
        public new string this[string key]
        {
            get
            {
                string result;
                if (TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                base[key] = value;
            }
        }

        string IDictionary<string,string>.this[string key]
        {
            get
            {
                string result;
                if (TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
