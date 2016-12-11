using System;
using c = System.Collections.Generic;


namespace Synthetic.Core
{
    /// <summary>
    /// Implements Dictionary object in Dynamo.  Dictionary uses strings as keys and System.Objects as values.
    /// </summary>
    public class Dictionary
    {
        internal c.Dictionary<string, System.Object> internalDictionary { get; private set; }

        internal Dictionary (c.Dictionary<string,System.Object> d) 
        {
            internalDictionary = d;
        }

        internal Dictionary()
        {
            internalDictionary = new c.Dictionary<string,System.Object>();
        }

        /// <summary>
        /// Creates a Dictionary made of key value pairs.
        /// </summary>
        /// <param name="keys">Keys to be used in the dictionary.</param>
        /// <param name="values">Values in the dictionary.</param>
        /// <returns name="Dictionary">A new dictionary object.</returns>
        public static Dictionary ByKeysValues (c.List<string> keys, c.List<System.Object> values)
        {
            c.Dictionary<string, System.Object> dict = new c.Dictionary<string, System.Object>();
            
            int i = 0;
            int vLength = values.Count;
            foreach (string key in keys)
            {
                if (i < vLength)
                {
                    dict.Add(key, values[i]);
                }
                i++;
            }

            return new Dictionary(dict);
        }


        /// <summary>
        /// Creates a Dictionary from a list of key value pair lists.
        /// </summary>
        /// <param name="keyValuePairs">A list of paired lists representing Keys and Values.</param>
        /// <returns name="Dictionary">A new dictionary object.</returns>
        public static Dictionary ByKeyValuePairs(c.List<c.List<System.Object>> keyValuePairs)
        {
            c.Dictionary<string, System.Object> dict = new c.Dictionary<string, System.Object>();

            foreach (c.List<System.Object> pair in keyValuePairs)
            {
                    dict.Add((string)pair[0], pair[1]);
            }

            return new Dictionary(dict);
        }

        /// <summary>
        /// Add a key-value pair from a dictionary.
        /// </summary>
        /// <param name="dictionary">A dictionary object.</param>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        /// <returns name="dictionary">The dictionary witht the key-valu pair added.</returns>
        public static Dictionary Add (Dictionary dictionary, string key, System.Object value)
        {
            if (dictionary.internalDictionary.ContainsKey(key) == false)
            {
                dictionary.internalDictionary.Add(key, value);
                return dictionary;
            }
            else { return null; }
        }

        /// <summary>
        /// Remove a key-value pair from a dictionary.
        /// </summary>
        /// <param name="dictionary">A dictionary object.</param>
        /// <param name="key">The key to the pair to be removed.</param>
        /// <returns name="dictionary">Returns the dictionary with the key-value pair removed.</returns>
        public static Dictionary Remove(Dictionary dictionary, string key)
        {
            if (dictionary.internalDictionary.ContainsKey(key) )
            {
                dictionary.internalDictionary.Remove(key);
                return dictionary;
            }
            else { return null; }
        }

        /// <summary>
        /// Retrieves a value at a given key.
        /// </summary>
        /// <param name="dictionary">A dictionary object.</param>
        /// <param name="key">A key.</param>
        /// <returns name="value">A value at the given key.</returns>
        public static System.Object Value (Dictionary dictionary, string key)
        {
            System.Object value;
            if (dictionary.internalDictionary.ContainsKey(key))
            {
                value = dictionary.internalDictionary[key];
            }
            else { value = null; }
            return value;
        }

        /// <summary>
        /// Retrieves the keys in the dictionary as a list.
        /// </summary>
        /// <param name="dictionary">A dictionary object.</param>
        /// <returns name="keys">A list of keys.</returns>
        public static c.List<string> GetKeys (Dictionary dictionary)
        {
            c.ICollection<string> keys = dictionary.internalDictionary.Keys;

            c.List<string> k = new c.List<string>();

            foreach (string key in keys)
            {
                k.Add(key);
            }
            return k;
        }

        /// <summary>
        /// Retrieves the values in the dictionary as a list.
        /// </summary>
        /// <param name="dict">A dictionary object.</param>
        /// <returns name="values">A list of values.</returns>
        public static c.List<System.Object> GetValues(Dictionary dict)
        {
            c.ICollection<System.Object> values = dict.internalDictionary.Values;

            c.List<System.Object> v = new c.List<System.Object>();

            foreach (System.Object value in values)
            {
                v.Add(value);
            }
            return v;
        }

        /// <summary>
        /// Converts the dictionary into a list of key-value pair lists.
        /// </summary>
        /// <param name="dict">A dictionary object</param>
        /// <returns name="Keys+Values">Returns a list of key-value pair lists.</returns>
        public static c.List<c.List<System.Object>> GetKeyValuePairs (Dictionary dict)
        {
            c.List< c.List <System.Object> > l = new c.List< c.List <System.Object> >();
            c.List<string> k = new c.List<string>();
            c.List<System.Object> v = new c.List<System.Object>();

            foreach (c.KeyValuePair<string, System.Object> kvp in dict.internalDictionary)
            {
                l.Add(new c.List<System.Object>{kvp.Key, kvp.Value} );
            }
            return l;
        }

        /// <summary>
        /// Sets the value at a given key.
        /// </summary>
        /// <param name="dictionary">a dictionary</param>
        /// <param name="key">A key in the dictionary.</param>
        /// <param name="value">The value to set</param>
        /// <returns name="dictionary">The modified dictionary.  Returns null if no key is found.</returns>
        public static Dictionary SetValueAtKey(Dictionary dictionary, string key, System.Object value)
        {
            c.Dictionary<string, System.Object> unwrapped = Dictionary.Unwrap(dictionary);
            if (dictionary.internalDictionary.ContainsKey(key))
            {
                dictionary.internalDictionary.Remove(key);
                dictionary.internalDictionary.Add(key, value);
                return dictionary;
            }
            else { return null; }
        }

        /// <summary>
        /// Unwraps and returns a .Net Dictionary object.
        /// </summary>
        /// <param name="dictionary">A Synthetic.Core.Dictionary object.</param>
        /// <returns name="dotNetDictionary">Returns a .Net Dictionary object.</returns>
        public static c.Dictionary<string, System.Object> Unwrap(Dictionary dictionary)
        {
            return dictionary.internalDictionary;
        }

        /// <summary>
        /// Wraps a .Net Dictionary object into a Synthetic.Core.Dictionary object.
        /// </summary>
        /// <param name="dotNetDictionary">.Net Dictionary object.</param>
        /// <returns name="dictionary">Returns a A Synthetic.Core.Dictionary object.</returns>
        public static Dictionary Wrap(c.Dictionary<string, System.Object> dotNetDictionary)
        {
            return new Dictionary(dotNetDictionary);
        }

        /// <summary>
        /// Prints the Dictionary as a string.
        /// </summary>
        /// <returns name="string">Converts to a string.</returns>
        public override string ToString()
        {
            c.Dictionary<string, System.Object> dict = this.internalDictionary;
            Type t = typeof(Dictionary);

            string s = "";
            s = string.Concat(s, t.Namespace, ".", GetType().Name);
            int i = 0;

            foreach (c.KeyValuePair<string, System.Object> keyValue in dict)
            {
                s = string.Concat(s, string.Format("\n  {0} Key-> \"{1}\", Value-> {2}", i, keyValue.Key, keyValue.Value));
                i++;
            }
            return s;
        }

        /// <summary>
        /// Creates an empty dictionary.
        /// </summary>
        /// <returns name="Dictionary">A empty dictionary.</returns>
        public static Dictionary ByEmpty ()
        {
            return new Dictionary();
        }
    }
}
