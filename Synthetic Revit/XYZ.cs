﻿using System;
using System.Collections.Generic;
using System.Linq;

using revitXYZ = Autodesk.Revit.DB.XYZ;

using dynaVector = Autodesk.DesignScript.Geometry.Vector;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Autodesk.Revit.DB XYZ object.
    /// </summary>
    public class XYZ
    {
        internal XYZ () { }

        /// <summary>
        /// Converts a Dynamo Vector into a Revit XYZ object
        /// </summary>
        /// <param name="Vector"></param>
        /// <returns></returns>
        public static revitXYZ ConvertVectorToXYZ (dynaVector Vector)
        {
            return new revitXYZ(Vector.X, Vector.Y, Vector.Z);
        }

        /// <summary>
        /// Converts a Revit XYZ object into a Dynamo Vector
        /// </summary>
        /// <param name="XYZ">A Autodesk.Revit.DB.XYZ object</param>
        /// <returns name="Vector">Returns a dynamo Vector</returns>
        public static dynaVector ConvertXYZToVector (revitXYZ XYZ)
        {
            return dynaVector.ByCoordinates(XYZ.X, XYZ.Y, XYZ.Z);
        }

        /// <summary>
        /// The X value of a Revit XYZ point.
        /// </summary>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="X">A double</returns>
        public static double X (revitXYZ XYZ)
        {
            return XYZ.X;
        }

        /// <summary>
        /// The Y value of a Revit XYZ point.
        /// </summary>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Y">A double</returns>
        public static double Y (revitXYZ XYZ)
        {
            return XYZ.Y;
        }

        /// <summary>
        /// The Z value of a Revit XYZ point.
        /// </summary>
        /// <param name="XYZ">A Revit XYZ point</param>
        /// <returns name="Z">A double</returns>
        public static double Z (revitXYZ XYZ)
        {
            return XYZ.Z;
        }
    }
}
