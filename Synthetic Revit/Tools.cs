using System;
using System.Collections.Generic;
using System.Linq;

using revitDoc = Autodesk.Revit.DB.Document;
using revitView = Autodesk.Revit.DB.View;
using revitView3D = Autodesk.Revit.DB.View3D;
using revitViewOrientation = Autodesk.Revit.DB.ViewOrientation3D;
using revitXYZ = Autodesk.Revit.DB.XYZ;
using revitBB = Autodesk.Revit.DB.BoundingBoxXYZ;

using RevitServices.Transactions;
using dynaView = Revit.Elements.Views.View;
using dynaView3D = Revit.Elements.Views.View3D;
using dynaPoint = Autodesk.DesignScript.Geometry.Point;

namespace Synthetic.Revit
{
    /// <summary>
    /// Methods that are complete workflows
    /// </summary>
    public class Tools
    {
        internal Tools () { }

        /// <summary>
        /// Matches View3D's orientation and cropbox to an orthagonal view.  Also sets the View3D to isometric.
        /// </summary>
        /// <param name="View3D">The 3D view to change orientation and crop</param>
        /// <param name="SourceView">The source orthogonal view to acquire orientation and cropbox</param>
        /// <param name="Offset">Offset of the camera from the view direction of the source view</param>
        /// <returns name="View3D"></returns>
        public static dynaView3D View3dOrientToggleIsometric (dynaView3D View3D, dynaView SourceView, double Offset)
        {
            string transactionName = "View3dOrientToggleIsometric";

            revitView3D rView = (revitView3D)View3D.InternalElement;
            revitView rSourceView = (revitView)SourceView.InternalElement;

            revitDoc document = rView.Document;

            // Cropbox
            revitBB cropbox = rSourceView.CropBox;
            revitXYZ origin = cropbox.Transform.Origin;

            // View Orientation
            revitXYZ rEyePosition = origin.Add(rSourceView.ViewDirection.Normalize().Multiply(Offset));
            revitXYZ rUpDirection = rSourceView.UpDirection;
            revitXYZ rForwardDirection = rSourceView.ViewDirection.Negate();

            revitViewOrientation viewOrient = new revitViewOrientation(rEyePosition, rUpDirection, rForwardDirection);

            Action<revitView3D, revitViewOrientation, revitBB> orientView = (view, orient, cropbb) =>
            {
                view.ToggleToIsometric();
                view.SetOrientation(orient);
                view.CropBox = cropbb;
            };

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                orientView(rView, viewOrient, cropbox);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    orientView(rView, viewOrient, cropbox);
                    trans.Commit();
                }
            }
            return View3D;
        }
    }
}
