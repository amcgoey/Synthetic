using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitView = Autodesk.Revit.DB.View;
using RevitView3D = Autodesk.Revit.DB.View3D;
using RevitViewOrientation = Autodesk.Revit.DB.ViewOrientation3D;
using RevitXYZ = Autodesk.Revit.DB.XYZ;
using RevitBB = Autodesk.Revit.DB.BoundingBoxXYZ;

using Revit.Elements;
using DynaElem = Revit.Elements.Element;
using DynaView = Revit.Elements.Views.View;
using DynaView3D = Revit.Elements.Views.View3D;
using DynaPoint = Autodesk.DesignScript.Geometry.Point;

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
        public static DynaView3D View3dOrientToggleIsometric (DynaView3D View3D, DynaView SourceView, double Offset)
        {
            string transactionName = "View3dOrientToggleIsometric";

            RevitView3D rView = (RevitView3D)View3D.InternalElement;
            RevitView rSourceView = (RevitView)SourceView.InternalElement;

            RevitDoc document = rView.Document;

            // Cropbox
            RevitBB cropbox = rSourceView.CropBox;
            RevitXYZ origin = cropbox.Transform.Origin;

            // View Orientation
            RevitXYZ rEyePosition = origin.Add(rSourceView.ViewDirection.Normalize().Multiply(Offset));
            RevitXYZ rUpDirection = rSourceView.UpDirection;
            RevitXYZ rForwardDirection = rSourceView.ViewDirection.Negate();

            RevitViewOrientation viewOrient = new RevitViewOrientation(rEyePosition, rUpDirection, rForwardDirection);

            Action<RevitView3D, RevitViewOrientation, RevitBB> orientView = (view, orient, cropbb) =>
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
