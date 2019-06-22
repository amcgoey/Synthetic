using System;
using System.Collections.Generic;
using j = Newtonsoft.Json;
using Autodesk.DesignScript.Runtime;

namespace Synthetic.Core
{
    /// <summary>
    /// Convert Revit Elements to and from JSON.
    /// </summary>
    public class Json
    {
        internal Json () { }

        /// <summary>
        /// Serializes an element into JSON.
        /// </summary>
        /// <param name="object">An object to serialize.</param>
        /// <returns name="JSON">A string of JSON.</returns>
        public static string Encode (System.Object @object)
        {
            return j.JsonConvert.SerializeObject(@object);
        }

        /// <summary>
        /// Deseralizes an element from JSON.
        /// </summary>
        /// <param name="json">A string of JSON</param>
        /// <returns name="object">An object to deserialize.</returns>
        public static System.Object Decode (string json)
        {
            return (System.Object)j.JsonConvert.DeserializeObject(json);
        }
    }
}
