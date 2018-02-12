using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using revitViewOrient = Autodesk.Revit.DB.ViewOrientation3D;
using revitXYZ = Autodesk.Revit.DB.XYZ;

namespace Synthetic.Revit
{
    /// <summary>
    /// Wrapper for Autodesk.Revit.DB.ViewOrientation
    /// </summary>
    public class ViewOrientation3D
    {
        internal ViewOrientation3D () { }

        /// <summary>
        /// Creates a new ViewOrientation3D given a EyePosition, UpDirection and ForwardDirection
        /// </summary>
        /// <param name="EyePosition">The coordinates for the EyePosition</param>
        /// <param name="UpDirection">A vector pointing to the Up direciton of the view (top of the screen)</param>
        /// <param name="ForwardDirection">A vector pointing from the camera towards the screen</param>
        /// <returns></returns>
        public static revitViewOrient ByEyeUpForwardDirections(revitXYZ EyePosition, revitXYZ UpDirection, revitXYZ ForwardDirection)
        {
            return new revitViewOrient(EyePosition, UpDirection, ForwardDirection);
        }

        /// <summary>
        /// Gets the coordinates of the EyePosition
        /// </summary>
        /// <param name="ViewOrient">Autodesk.Revit.DB.ViewOrientation3D</param>
        /// <returns name="XYZ">The coordinates of the EyePosition</returns>
        public static revitXYZ GetEyePosition (revitViewOrient ViewOrient)
        {
            return ViewOrient.EyePosition;
        }

        /// <summary>
        /// Gets the vector of the UpDirection that points to the top of the screen.
        /// </summary>
        /// <param name="ViewOrient">Autodesk.Revit.DB.ViewOrientation3D</param>
        /// <returns name="XYZ">The vector of the UpDirection</returns>
        public static revitXYZ GetUpDirection(revitViewOrient ViewOrient)
        {
            return ViewOrient.UpDirection;
        }

        /// <summary>
        /// Gets the vector of the FowardDirection that points from the camera towards the screen.
        /// </summary>
        /// <param name="ViewOrient">Autodesk.Revit.DB.ViewOrientation3D</param>
        /// <returns name="XYZ">The vector of the ForwardDirection</returns>
        public static revitXYZ GetForwardDirection(revitViewOrient ViewOrient)
        {
            return ViewOrient.ForwardDirection;
        }
    }
}
