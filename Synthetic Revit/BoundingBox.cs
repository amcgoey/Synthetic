using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitBBxyz = Autodesk.Revit.DB.BoundingBoxXYZ;
using revitBBuv = Autodesk.Revit.DB.BoundingBoxUV;
using revitTransform = Autodesk.Revit.DB.Transform;
using revitXYZ = Autodesk.Revit.DB.XYZ;
using revitUV = Autodesk.Revit.DB.UV;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Revit BoundingBoxXYZ
    /// </summary>
    public class BoundingBoxXYZ
    {
        internal BoundingBoxXYZ() { }

        /// <summary>
        /// Return the bounding box's transform
        /// </summary>
        /// <param name="BoundingBoxXYZ"></param>
        /// <returns></returns>
        public static revitTransform GetTransform(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Transform;
        }

        /// <summary>
        /// Given a Revit BoundingBoxXYZ, returns its origin point.
        /// </summary>
        /// <param name="BoundingBoxXYZ"></param>
        /// <returns name="Origin"></returns>
        public static revitXYZ GetOrigin(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Transform.Origin;
        }

        public static revitXYZ GetMax(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Max;
        }

        public static revitXYZ GetMin(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Min;
        }

        public static revitBBxyz SetMax(revitBBxyz BoundingBoxXYZ, revitXYZ PointXYZ)
        {
            BoundingBoxXYZ.Max = PointXYZ;
            return BoundingBoxXYZ;
        }

        public static revitBBxyz SetMin(revitBBxyz BoundingBoxXYZ, revitXYZ PointXYZ)
        {
            BoundingBoxXYZ.Min = PointXYZ;
            return BoundingBoxXYZ;
        }
    }

    /// <summary>
    /// Wrapper for Revit BoundingBoxUV elements
    /// </summary>
    public class BoundingBoxUV
    {
        internal BoundingBoxUV () { }

        public static revitUV GetMax(revitBBuv BoundingBoxUV)
        {
            return BoundingBoxUV.Max;
        }

        public static revitUV GetMin(revitBBuv BoundingBoxUV)
        {
            return BoundingBoxUV.Min;
        }

        public static revitBBuv SetMax(revitBBuv BoundingBoxUV, revitUV PointUV)
        {
            BoundingBoxUV.Max = PointUV;
            return BoundingBoxUV;
        }

        public static revitBBuv SetMin(revitBBuv BoundingBoxUV, revitUV PointUV)
        {
            BoundingBoxUV.Min = PointUV;
            return BoundingBoxUV;
        }
    }
}
