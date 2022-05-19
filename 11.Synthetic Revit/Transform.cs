using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitTransform = Autodesk.Revit.DB.Transform;
using revitXYZ = Autodesk.Revit.DB.XYZ;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Revit Transform
    /// </summary>
    public class Transform
    {
        internal Transform () { }

        /// <summary>
        /// Get the origin of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="XYZ">Autodesk.Revit.DB.XYZ</returns>
        public static revitXYZ GetOrigin (revitTransform Transform)
        {
            return Transform.Origin;
        }

        /// <summary>
        /// Get the Basis of the X Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="XYZ">Autodesk.Revit.DB.XYZ</returns>
        public static revitXYZ GetBasisX(revitTransform Transform)
        {
            return Transform.BasisX;
        }

        /// <summary>
        /// Get the Basis of the Y Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="XYZ">Autodesk.Revit.DB.XYZ</returns>
        public static revitXYZ GetBasisY(revitTransform Transform)
        {
            return Transform.BasisY;
        }

        /// <summary>
        /// Get the Basis of the Z Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="XYZ">Autodesk.Revit.DB.XYZ</returns>
        public static revitXYZ GetBasisZ(revitTransform Transform)
        {
            return Transform.BasisZ;
        }

        /// <summary>
        /// Set the origin of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetOrigin (revitTransform Transform, revitXYZ XYZ)
        {
            Transform.Origin = XYZ;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the X Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisX(revitTransform Transform, revitXYZ XYZ)
        {
            Transform.BasisX = XYZ;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the Y Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisY(revitTransform Transform, revitXYZ XYZ)
        {
            Transform.BasisY = XYZ;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the Z Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisZ(revitTransform Transform, revitXYZ XYZ)
        {
            Transform.BasisZ = XYZ;
            return Transform;
        }

        /// <summary>
        /// Applies the Transform to a point.
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="XYZ">A transformed Revit XYZ point</returns>
        public static revitXYZ OfPoint(revitTransform Transform, revitXYZ XYZ)
        {
            return Transform.OfPoint(XYZ);
        }

        /// <summary>
        /// Applies the Transform to a vector.
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <param name="XYZ">A Revit XYZ vector</param>
        /// <returns name="XYZ">A transformed Revit XYZ vector</returns>
        public static revitXYZ OfVector(revitTransform Transform, revitXYZ XYZ)
        {
            return Transform.OfVector(XYZ);
        }

        /// <summary>
        /// The inverse transformation of this Transom
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="Transom">The inverted Autodesk.Revit.DB.Transform</returns>
        public static revitTransform Inverse(revitTransform Transform)
        {
            return Transform.Inverse;
        }

    }
}
