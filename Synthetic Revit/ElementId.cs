using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitDB = Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes for managing Autodesk.Revit.DB.ElementId objects.
    /// </summary>
    public class ElementId
    {
        internal ElementId () { }

        /// <summary>
        /// Creates a Autodesk.Revit.DB.ElementId object from an integer
        /// </summary>
        /// <param name="integer">The ElementId as an integer</param>
        /// <returns name="ElementId">Returns an Autodesk.Revit.DB.ElementId object</returns>
        public static revitElemId ByInt (int integer)
        {
            return new revitElemId(integer);
        }

        /// <summary>
        /// Creates a Autodesk.Revit.DB.ElementId object from a string representation of a integer
        /// </summary>
        /// <param name="str">The ElementId as an string.</param>
        /// <returns name="ElementId">Returns an Autodesk.Revit.DB.ElementId object</returns>
        public static revitElemId ByString(string str)
        {
            return new revitElemId(Convert.ToInt32(str));
        }

        /// <summary>
        /// Returns the integer value of the Autodesk.Revit.DB.ElementId
        /// </summary>
        /// <param name="elementId">A Autodesk.Revit.DB.ElementId</param>
        /// <returns name="integer">The integer value of the Autodesk.Revit.DB.ElementId</returns>
        public static int ToInt (revitElemId elementId)
        {
            return elementId.IntegerValue;
        }
    }
}
