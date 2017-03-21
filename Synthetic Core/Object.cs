using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthetic.Core
{
    /// <summary>
    /// General utility nodes that operate on any object
    /// </summary>
    public class Object
    {
        internal Object ()
        { }

        /// <summary>
        /// Returns the type of the object.
        /// </summary>
        /// <param name="obj">A object</param>
        /// <returns>Returns the type of the object</returns>
        public static System.Type Type(System.Object obj)
        {
            return obj.GetType();
        }
    }
}
