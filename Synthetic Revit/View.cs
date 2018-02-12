using System;
using System.Collections;
using System.Collections.Generic;

using revitView = Autodesk.Revit.DB.View;
using revitView3D = Autodesk.Revit.DB.View3D;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitViewOrientation = Autodesk.Revit.DB.ViewOrientation3D;
using revitXYZ = Autodesk.Revit.DB.XYZ;

using RevitServices.Transactions;
using dynaView = Revit.Elements.Views.View;
using dynaView3D = Revit.Elements.Views.View3D;

using rBoundingBox = Autodesk.Revit.DB.BoundingBoxXYZ;

namespace Synthetic.Revit
{
    /// <summary>
    /// Revit API extensions to the Dynamo View wrapper
    /// </summary>
    public class View
    {
        internal View () { }

        /// <summary>
        /// Applies source view's parameters to the view.  Applies parameters based on GetTemplateParameterIds.
        /// </summary>
        /// <param name="View">View to modify based on source view's parameters</param>
        /// <param name="SourceView">Source view to take parameters from</param>
        /// <returns name="View">The modified view</returns>
        public static dynaView ApplyViewParameters (dynaView View, dynaView SourceView)
        {
            revitView dView = (revitView)View.InternalElement;
            revitView sview = (revitView)SourceView.InternalElement;

            revitDoc document = dView.Document;
            
            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                dView.ApplyViewTemplateParameters(sview);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Apply View Parameters");
                    dView.ApplyViewTemplateParameters(sview);
                    trans.Commit();
                }
            }

            return View;
        }

        /// <summary>
        /// Gets the ElementIds of the view's parameters that are controlled by a view template.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <returns name="ElementIds">A list of ElementIds that are controlled by a view template.</returns>
        public static IList<revitElemId> GetTemplateParameterIds(dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.GetTemplateParameterIds();
        }

        /// <summary>
        /// Gets the ElementIds of the view's parameters that are NOT controlled by a view template.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <returns name="ElementIds">A list of ElementIds that are NOT controlled by a view template.</returns>
        public static IList<revitElemId> GetNonControlledTemplateParameterIds(dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return (IList<revitElemId>)rView.GetNonControlledTemplateParameterIds();
        }

        /// <summary>
        /// Sets the view's parameters not controlled by templates.
        /// </summary>
        /// <param name="View">A dynamo wrapped View</param>
        /// <param name="ElementIds">A list of ElementIds</param>
        public static void SetNonControlledTemplateParameterIds(dynaView View, IList<revitElemId> ElementIds)
        {
            revitView rView = (revitView)View.InternalElement;
            rView.SetNonControlledTemplateParameterIds(ElementIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View"></param>
        /// <returns name="CropBox">Revit BoundingBoxXYZ object that represents the cropbox of the view.</returns>
        public static rBoundingBox GetCropbox (dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.CropBox;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View"></param>
        /// <returns name="View"></returns>
        public static dynaView SetCropbox(dynaView View, rBoundingBox CropBox)
        {
            revitView rView = (revitView)View.InternalElement;
            revitDoc document = rView.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                rView.CropBox = CropBox;
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Set CropBox of " + rView.Name);
                    rView.CropBox = CropBox;
                    trans.Commit();
                }
            }

            if (rView.CropBox == CropBox)
            {
                return View;
            }
            else { return null; }
        }

        /// <summary>
        /// Returns the UpDirection of a view.
        /// </summary>
        /// <param name="View">A view</param>
        /// <returns name="XYZ">A Revit XYZ object pointing in the Up direction</returns>
        public static revitXYZ GetUpDirection (dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.UpDirection;
        }

        /// <summary>
        /// Returns the direction towards the viewer
        /// </summary>
        /// <param name="View">A view</param>
        /// <returns name="XYZ">A Revit XYZ object pointing towards the viewer</returns>
        public static revitXYZ GetViewDirection(dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.ViewDirection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View3D"></param>
        /// <returns name="ViewOrient"></returns>
        public static revitViewOrientation GetViewOrientation (dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;
            return rView.GetOrientation();
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View3D"></param>
        /// <param name="ViewOrient"></param>
        /// <returns></returns>
        public static dynaView3D SetViewOrientation(dynaView3D View3D, revitViewOrientation ViewOrient)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;
            revitDoc document = rView.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                rView.SetOrientation(ViewOrient);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Set view orientation");
                    rView.SetOrientation(ViewOrient);
                    trans.Commit();
                }
            }

            if (rView.GetOrientation() == ViewOrient)
            {
                return View3D;
            }
            else { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View3D"></param>
        /// <returns></returns>
        public static dynaView3D ToggleToIsometric (dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;
            revitDoc document = rView.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                rView.ToggleToIsometric();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Toggle " + rView.Name + " To Isometric");
                    rView.ToggleToIsometric();
                    trans.Commit();
                }
            }
            
            return View3D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="View3D"></param>
        /// <returns></returns>
        public static dynaView3D ToggleToPerspective(dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;
            revitDoc document = rView.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                rView.ToggleToPerspective();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start("Toggle " + rView.Name + " To Perspective");
                    rView.ToggleToPerspective();
                    trans.Commit();
                }
            }

            return View3D;
        }
    }
}
