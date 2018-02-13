using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitBB = Autodesk.Revit.DB.BoundingBoxXYZ;
using revitTransform = Autodesk.Revit.DB.Transform;
using revitXYZ = Autodesk.Revit.DB.XYZ;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Revit BoundingBoxXYZ
    /// </summary>
    public class BoundingBoxXYZ
    {
        internal BoundingBoxXYZ () { }

        /// <summary>
        /// Return the bounding box's transform
        /// </summary>
        /// <param name="BoundingBoxXYZ"></param>
        /// <returns></returns>
        public static revitTransform GetTransform (revitBB BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Transform;
        }

        public static revitXYZ GetOrigin (revitBB BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Transform.Origin;
        }
    }
}
