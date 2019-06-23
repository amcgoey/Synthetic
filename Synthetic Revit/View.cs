using System;
using System.Collections;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitView = Autodesk.Revit.DB.View;
using revitView3D = Autodesk.Revit.DB.View3D;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitViewOrientation = Autodesk.Revit.DB.ViewOrientation3D;
using revitXYZ = Autodesk.Revit.DB.XYZ;
using revitBBxyz = Autodesk.Revit.DB.BoundingBoxXYZ;
using revitBBuv = Autodesk.Revit.DB.BoundingBoxUV;
using revitParam = Autodesk.Revit.DB.Parameter;
using revitSheet = Autodesk.Revit.DB.ViewSheet;
using revitViewport = Autodesk.Revit.DB.Viewport;

using RevitServices.Transactions;
using Revit.Elements;
using dynaElem = Revit.Elements.Element;
using dynaView = Revit.Elements.Views.View;
using dynaView3D = Revit.Elements.Views.View3D;
using dynaSheet = Revit.Elements.Views.Sheet;

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
        /// Gets a view's cropbox
        /// </summary>
        /// <param name="View">A Dynamo wrapped View</param>
        /// <returns name="CropBox">Revit BoundingBoxXYZ object that represents the cropbox of the view.</returns>
        public static revitBBxyz GetCropbox (dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.CropBox;
        }

        /// <summary>
        /// Sets the cropbox of a view
        /// </summary>
        /// <param name="View">A Dynamo wrapped View</param>
        /// <param name="CropBox">A Revit BoundBoxXYZ</param>
        /// <returns name="View">A Dynamo wrapped View with an updated cropbox</returns>
        public static dynaView SetCropbox(dynaView View, revitBBxyz CropBox)
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
            return View;
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
        /// <param name="View">A Dynamo wrapped View</param>
        /// <returns name="XYZ">A Revit XYZ object pointing towards the viewer</returns>
        public static revitXYZ GetViewDirection(dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.ViewDirection;
        }

        /// <summary>
        /// Gets the 3D view's orientation.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <returns name="ViewOrient">A Revit ViewOrientation3D element</returns>
        public static revitViewOrientation GetViewOrientation (dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;
            return rView.GetOrientation();
        }



        /// <summary>
        /// Sets the 3D view's orientation.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <param name="ViewOrient">A Revit ViewOrientation3D element</param>
        /// <returns name="View3D">A Dynamo wrapped View3D</returns>
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
            return View3D;
        }

        /// <summary>
        /// Changes a view from Perspective to Isometric.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <returns name="View3D">A Dynamo wrapped View3D</returns>
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
        /// Changes a view from Isometric to Perspective.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <returns name="View3D">A Dynamo wrapped View3D</returns>
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

        /// <summary>
        /// Duplciates a view and renames it.
        /// </summary>
        /// <param name="Name">The name of the duplicated view</param>
        /// <param name="SourceView">The view to duplicate</param>
        /// <param name="DuplicateOptions">Enum ViewDuplicateOptions</param>
        /// <returns name="View">The duplicated view</returns>
        public static dynaView DuplicateView (string Name,
            dynaView SourceView,
            revitDB.ViewDuplicateOption DuplicateOptions)
        {
            string transactionName = "Duplicate View";
            Func<revitView, revitDB.ViewDuplicateOption, revitDoc, revitView> dupView = (v, vdo, doc) =>
            {
                revitElemId viewId = v.Duplicate(vdo);
                revitView newView = (revitView)doc.GetElement(viewId);
                newView.Name = Name;
                return newView;
            };

            revitView rView = (revitView)SourceView.InternalElement;
            revitDoc document = rView.Document;
            revitView view;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                view = dupView(rView, DuplicateOptions, document);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    view = dupView(rView, DuplicateOptions, document);
                    trans.Commit();
                }
            }
            return (dynaView)view.ToDSType(true);
        }

        /// <summary>
        /// Gets a View's outline.
        /// </summary>
        /// <param name="View">A Dynamo wrapped View</param>
        /// <returns name="BoundingBoxUV">A Revit BoundingBoxUV representing the outline of the view.</returns>
        public static revitBBuv GetOutline (dynaView View)
        {
            revitView rView = (revitView)View.InternalElement;
            return rView.Outline;
        }

        /// <summary>
        /// Gets a View's distance to the Far Clipping Plane.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <returns name="Distance">Distance to the Far Clipping Plane as a Double.</returns>
        public static double GetFarClippingDistance (dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;

            return Math.Abs(rView.CropBox.Min.Z);
        }

        /// <summary>
        /// Gets a View's distance to the Near Clipping Plane.
        /// </summary>
        /// <param name="View3D">A Dynamo wrapped View3D</param>
        /// <returns name="Distance">Distance to the Near Clipping Plane as a Double.</returns>
        public static double GetNearClippingDistance(dynaView3D View3D)
        {
            revitView3D rView = (revitView3D)View3D.InternalElement;

            return Math.Abs(rView.CropBox.Max.Z);
        }

        /// <summary>
        /// Set the far clipping distance of a view with far clipping active.  This method preserves the near clip distance, however setting the far clip distance using any other method will reset the near clip distance.
        /// </summary>
        /// <param name="View">A Dynamo wrapped view</param>
        /// <param name="FarClipping">The Far Clipping Offset distance as a number from the camera</param>
        /// <returns name="View">Returns the modified view</returns>
        public static dynaView SetFarClippingDistance(dynaView View, double FarClipping)
        {
            string transactionName = "Set Far Clipping Distance";

            revitView rView = (revitView)View.InternalElement;

            double NearClipping = rView.CropBox.Max.Z;

            revitDoc document = rView.Document;

            Action _SetFarClip = () =>
            {
                rView.get_Parameter(revitDB.BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(FarClipping);
                revitBBxyz CropBox = rView.CropBox;
                revitXYZ oldMax = CropBox.Max;
                revitXYZ newMax = new revitXYZ(oldMax.X, oldMax.Y, -Math.Abs(NearClipping));
                revitBBxyz NewCropBox = new revitBBxyz();
                NewCropBox.Max = newMax;
                NewCropBox.Min = CropBox.Min;
                rView.CropBox = NewCropBox;
            };

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                _SetFarClip();
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    _SetFarClip();
                    trans.Commit();
                }
            }

            return View;
        }

        /// <summary>
        /// Sets the near clipping plane distance of the view.  This is done by modifying the cropbox.  Please note that modifying the far clip distance will reset any near clip modifications.
        /// </summary>
        /// <param name="View">A Dynamo wrapped view</param>
        /// <param name="NearClipping">The Near Clipping Offset distance as a number from the camera</param>
        /// <returns name="View">Returns the modified view</returns>
        public static dynaView SetNearClippingDistance(dynaView3D View, double NearClipping)
        {
            string transactionName = "Set Near Clipping Distance";

            revitView rView = (revitView)View.InternalElement;

            revitBBxyz CropBox = rView.CropBox;
            revitXYZ oldMax = CropBox.Max;
            revitXYZ newMax = new revitXYZ(oldMax.X, oldMax.Y, -Math.Abs(NearClipping));
            revitBBxyz NewCropBox = new revitBBxyz();
            NewCropBox.Max = newMax;
            NewCropBox.Min = CropBox.Min;

            revitDoc document = rView.Document;

            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                rView.CropBox = NewCropBox;
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    rView.CropBox = NewCropBox;
                    trans.Commit();
                }
            }

            return View;
        }

        /// <summary>
        /// Renumbers the views on the sheet based on the view grid.
        /// </summary>
        /// <param name="Sheet">A dynamo Sheet element</param>
        /// <param name="gridX">Size of the layout grid in the X direction</param>
        /// <param name="gridY">Size of the layout grid in the Y direction</param>
        /// <param name="originX">Location of the layout grid origin on the X axis</param>
        /// <param name="originY">Location of the layout grid origin on the Y axis</param>
        /// <returns name="Viewports">Revit viewport objects on the sheet.</returns>
        public static List<revitViewport> RenumberOnSheet (dynaSheet Sheet, double gridX, double gridY, double originX, double originY)
        {
            string transactionName = "Renumber views on sheet";

            //  Initialize variables
            revitSheet rSheet = (revitSheet)Sheet.InternalElement;
            revitDoc document = rSheet.Document;
            List<revitElemId> rViewports = (List < revitElemId > )rSheet.GetAllViewports();
            List<revitViewport> viewports;

            double viewportOffset = 0.0114;

            //  Function for use inside of transaction.

            //  Function filters out Legend views,
            //  temporarily renumbers all non-legend views,
            //  then calculates the views correct number,
            //  then renumbers the views.
            Func<List<revitElemId>, revitDoc, List<revitViewport>> _renumber = (rvp, d) =>
              {
                  List<revitViewport> filteredViewports = new List<revitViewport>();
                  int i = 1;

                  foreach (revitElemId id in rvp)
                  {
                      revitViewport vp = (revitViewport)d.GetElement(id);
                      revitView v = (revitView)d.GetElement(vp.ViewId);

                      if (v.ViewType != revitDB.ViewType.Legend)
                      {
                          filteredViewports.Add(vp);

                          vp.get_Parameter(revitDB.BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set("!!" + i);
                          i++;
                      }
                  }

                  i = 1;
                  foreach (revitViewport vp in filteredViewports)
                  {
                      revitXYZ minPt = vp.GetBoxOutline().MinimumPoint;

                      double x = Math.Floor(((minPt.X - originX) + viewportOffset) / gridX + 1);
                      double y = Math.Floor(((minPt.Y - originY) + viewportOffset) / gridY + 1);

                      if (x > 0 && y > 0)
                      {
                          char yChar = (char)('A' - 1 + y);
                          string yString = yChar.ToString();

                          vp.get_Parameter(revitDB.BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set(yString + x);
                      }
                      else
                      {
                          vp.get_Parameter(revitDB.BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set("!!" + i + "!!");
                      }
                  }

                  return filteredViewports;
              };

            //  
            if (document.IsModifiable)
            {
                TransactionManager.Instance.EnsureInTransaction(document);
                viewports = _renumber(rViewports, document);
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                {
                    trans.Start(transactionName);
                    viewports = _renumber(rViewports, document);
                    trans.Commit();
                }
            }

            return viewports;
        }

        public static List<revitViewport> RenumberOnActiveSheet(string FamilyName, string FamilyTypeName, string xGridName, string yGridName)
        {
            revitDoc doc = Document.Current();
            revitSheet sheet = (revitSheet)doc.ActiveView;

            revitCollector collector = new revitCollector(doc);

            collector.OfClass(typeof(revitDB.Family));

            //  Get the family Symbol

            revitDB.ParameterValueProvider provider
                = new revitDB.ParameterValueProvider(
                    new revitDB.ElementId
                    (revitDB.BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM));

            revitDB.FilterStringRuleEvaluator evaluator = new revitDB.FilterStringEquals();
            revitDB.FilterRule rule = new revitDB.FilterStringRule(provider, evaluator, FamilyName, false);

            revitElementFilter filterFamily = new revitDB.ElementParameterFilter(rule);

            List<revitElemId> symbolIds = (List<revitElemId>)collector.OfClass(typeof(revitDB.Family)).WherePasses(filterFamily).ToElementIds();

            revitElemId symbolId = symbolIds[0];

            //  Get the family Instances in view
            revitElementFilter filterInstance = new revitDB.FamilyInstanceFilter(doc, symbolId);

            collector = new revitCollector(doc, sheet.Id);
            revitDB.FamilyInstance originFamily = (revitDB.FamilyInstance)collector.OfClass(typeof(revitDB.FamilyInstance)).WherePasses(filterInstance).First();

            //List<revitDB.FamilyInstance> instances = new revitDB.FilteredElementCollector(doc)
            //    .OfClass(typeof(revitDB.FamilyInstance))
            //    .Cast<revitDB.FamilyInstance>()
            //    .Where(x => x.Symbol.Family.Name.Equals(FamilyName)) //Family
            //    .Where(x => x.Name.Equals(FamilyTypeName)).ToList<revitDB.FamilyInstance>(); // family type

            revitDB.LocationPoint location = (revitDB.LocationPoint)originFamily.Location;
            revitXYZ point = location.Point;

            originFamily.get_Parameter(xGridName);
        }

        /// <summary>
        /// Gets the active view in the document.  Returns an unwrapped Revit view.
        /// </summary>
        /// <returns name="View">Returns the Revit view that is the current active view.  This is an unwrapped Revit element not the dynamo wrapped version.</returns>
        public static revitView ActiveView ()
        {
            revitDoc doc = Document.Current();

            return doc.ActiveView;
        }

    }
}
