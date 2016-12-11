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
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string Encode (System.Object element)
        {
            return j.JsonConvert.SerializeObject(element);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static System.Object Decode (string json)
        {
            return (System.Object)j.JsonConvert.DeserializeObject(json);
        }
    }
}
