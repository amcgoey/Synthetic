using System;

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
        /// Return the BoundingBoxXYZ's transform
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

        /// <summary>
        /// Get the Revit XYZ Max point of a BoundingBoxXYZ.
        /// </summary>
        /// <param name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</param>
        /// <returns name="XYZ">A Revit XYZ point</returns>
        public static revitXYZ GetMax(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Max;
        }

        /// <summary>
        /// Get the Revit XYZ Min point of a BoundingBoxXYZ.
        /// </summary>
        /// <param name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</param>
        /// <returns name="XYZ">A Revit XYZ point</returns>
        public static revitXYZ GetMin(revitBBxyz BoundingBoxXYZ)
        {
            return BoundingBoxXYZ.Min;
        }

        /// <summary>
        /// Sets a Revit XYZ Bounding Box's Max point.
        /// </summary>
        /// <param name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</param>
        /// <param name="PointXYZ">A Revit XYZ point</param>
        /// <returns name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</returns>
        public static revitBBxyz SetMax(revitBBxyz BoundingBoxXYZ, revitXYZ PointXYZ)
        {
            BoundingBoxXYZ.Max = PointXYZ;
            return BoundingBoxXYZ;
        }

        /// <summary>
        /// Sets a Revit Bounding Box's Min point.
        /// </summary>
        /// <param name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</param>
        /// <param name="PointXYZ">A revit XYZ point</param>
        /// <returns name="BoundingBoxXYZ">A Revit BoundingBoxXYZ</returns>
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

        /// <summary>
        /// Get the Revit UV Max point of a BoundingBoxUV.
        /// </summary>
        /// <param name="BoundingBoxUV">A Revit BoundingBoxXYZ</param>
        /// <returns name="UV">A Revit UV point</returns>
        public static revitUV GetMax(revitBBuv BoundingBoxUV)
        {
            return BoundingBoxUV.Max;
        }

        /// <summary>
        /// Get the Revit UV Min point of a BoundingBoxUV.
        /// </summary>
        /// <param name="BoundingBoxUV">A Revit BoundingBoxXYZ</param>
        /// <returns name="UV">A Revit UV point</returns>
        public static revitUV GetMin(revitBBuv BoundingBoxUV)
        {
            return BoundingBoxUV.Min;
        }

        /// <summary>
        /// Sets a Revit UV Bounding Box's Max point.
        /// </summary>
        /// <param name="BoundingBoxUV">A Revit BoundingBoxUV</param>
        /// <param name="PointUV">A Revit UV point</param>
        /// <returns name="BoundingBoxUV">A Revit BoundingBoxUV</returns>
        public static revitBBuv SetMax(revitBBuv BoundingBoxUV, revitUV PointUV)
        {
            BoundingBoxUV.Max = PointUV;
            return BoundingBoxUV;
        }

        /// <summary>
        /// Sets a Revit UV Bounding Box's Min point.
        /// </summary>
        /// <param name="BoundingBoxUV">A Revit BoundingBoxUV</param>
        /// <param name="PointUV">A Revit UV point</param>
        /// <returns name="BoundingBoxUV">A Revit BoundingBoxUV</returns>
        public static revitBBuv SetMin(revitBBuv BoundingBoxUV, revitUV PointUV)
        {
            BoundingBoxUV.Min = PointUV;
            return BoundingBoxUV;
        }
    }
}
