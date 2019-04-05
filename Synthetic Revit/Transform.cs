using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitTransform = Autodesk.Revit.DB.Transform;
using revitXYZ = Autodesk.Revit.DB.XYZ;

namespace Synthetic.Revit
{
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
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetOrigin (revitTransform Transform, revitXYZ Point)
        {
            Transform.Origin = Point;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the X Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisX(revitTransform Transform, revitXYZ Point)
        {
            Transform.BasisX = Point;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the Y Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisY(revitTransform Transform, revitXYZ Point)
        {
            Transform.BasisY = Point;
            return Transform;
        }

        /// <summary>
        /// Set the Basis of the Z Axis of the Transform
        /// </summary>
        /// <param name="Transform">Autodesk.Revit.DB.Transform</param>
        /// <returns name="Transform">Modified Transform</returns>
        public static revitTransform SetBasisZ(revitTransform Transform, revitXYZ Point)
        {
            Transform.BasisZ = Point;
            return Transform;
        }

        public static revitXYZ OfPoint(revitTransform Transform, revitXYZ Point)
        {
            return Transform.OfPoint(Point);
        }

        public static revitXYZ OfVector(revitTransform Transform, revitXYZ Vector)
        {
            return Transform.OfPoint(Vector);
        }

        public static revitTransform Inverse(revitTransform Transform)
        {
            return Transform.Inverse;
        }

    }
}
